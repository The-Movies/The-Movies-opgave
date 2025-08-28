// System-navneområder (standard)
using System;                             // Exception, DateTime, DateTimeKind, etc.
using System.Collections.Generic;         // List<T>, IEnumerable<T>
using System.IO;                          // File, Directory, Path
using System.Linq;                        // LINQ (OrderBy, RemoveAll, Select)
using System.Text.Json;                   // JsonSerializer, JsonSerializerOptions

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Screening
using The_movie_egen.Model.Repositories;  // IScreeningRepository bruges

namespace The_movie_egen.Data.Json
{
    /// <summary>
    /// JSON-baseret repository til persistering af forestillings-data.
    /// - Gemmer forestillinger organiseret pr. biograf og måned
    /// - Håndterer CRUD-operationer med automatisk ID-generering
    /// - Struktureret fil-organisation: data/screenings/cinema-X/YYYY-MM.json
    /// - UTC-tidshåndtering for konsistent dato/klokkeslæt
    /// - Fejlhåndtering og automatisk directory-oprettelse
    /// </summary>
    public sealed class JsonScreeningRepository : IScreeningRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter og konfiguration
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Base-directory for alle forestillings-filer.
        /// </summary>
        private readonly string _baseDir;

        /// <summary>
        /// Sti til index-fil der holder næste ledige ID.
        /// </summary>
        private readonly string _indexFile;

        /// <summary>
        /// Movie repository til at indlæse film-data.
        /// </summary>
        private readonly IMovieRepository? _movieRepo;

        /// <summary>
        /// JSON-serialiseringsindstillinger for læsbar output.
        /// </summary>
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // ─────────────────────────────────────────────────────────────────────────
        //  DTO-klasser (Data Transfer Objects)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// DTO til index-fil der holder næste ledige ID.
        /// </summary>
        private sealed class IndexDto 
        { 
            public int NextId { get; set; } = 1; 
        }

        /// <summary>
        /// DTO(Data Transfer Object) til forestillings-data i JSON-filer.
        /// Bruges til serialisering/deserialisering af Screening-objekter.
        /// </summary>
        private sealed class ScreeningDto
        {
            public int Id { get; set; }
            public int CinemaId { get; set; }
            public int AuditoriumId { get; set; }
            public int MovieId { get; set; }
            public DateTime StartUtc { get; set; }
            public int AdsMinutes { get; set; } = 15;
            public int CleaningMinutes { get; set; } = 15;
        }

        /// <summary>
        /// Ctor: initialiserer repository og opretter nødvendige directories.
        /// </summary>
        /// <param name="baseDir">Base-directory for forestillings-filer (default: "data/screenings")</param>
        /// <param name="movieRepo">Optional movie repository til at indlæse film-data</param>
        public JsonScreeningRepository(string baseDir = "data/screenings", IMovieRepository? movieRepo = null)
        {
            _baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, baseDir));
            Directory.CreateDirectory(_baseDir);
            _indexFile = Path.Combine(_baseDir, "index.json");
            _movieRepo = movieRepo;
            EnsureIndex();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (IScreeningRepository implementation)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle forestillinger for en specifik biograf og måned.
        /// 1) Bestemmer fil-sti baseret på cinemaId, year og month
        /// 2) Læser JSON-fil hvis den findes
        /// 3) Konverterer DTO'er til Screening-objekter
        /// 4) Sikrer UTC-tidshåndtering
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="year">År (fx 2025)</param>
        /// <param name="month">Måned (1-12)</param>
        /// <returns>Liste over forestillinger for den måned</returns>
        public IEnumerable<Screening> GetForCinemaMonth(int cinemaId, int year, int month)
        {
            var file = GetMonthFile(cinemaId, year, month);
            if (!File.Exists(file)) return Enumerable.Empty<Screening>();

            var json = File.ReadAllText(file);
            var list = JsonSerializer.Deserialize<List<ScreeningDto>>(json, _json) ?? new();

            return list.Select(d => new Screening
            {
                Id = d.Id,
                CinemaId = d.CinemaId,
                AuditoriumId = d.AuditoriumId,
                MovieId = d.MovieId,
                StartUtc = DateTime.SpecifyKind(d.StartUtc, DateTimeKind.Utc),
                AdsMinutes = d.AdsMinutes,
                CleaningMinutes = d.CleaningMinutes,
                // Indlæs Movie data hvis movie repository er tilgængelig
                Movie = _movieRepo?.GetById(d.MovieId)
                // EndUtc er beregnet i modellen
            });
        }

        /// <summary>
        /// Tilføjer en ny forestilling til repository.
        /// 1) Bestemmer måned baseret på StartUtc
        /// 2) Tildeler globalt unikt ID hvis nødvendigt
        /// 3) Konverterer til DTO og gemmer til fil
        /// </summary>
        /// <param name="s">Forestillingen der skal tilføjes</param>
        public void Add(Screening s)
        {
            // Bestem måned ud fra StartUtc (UTC i modellen)
            var startUtc = s.StartUtc.Kind == DateTimeKind.Utc ? s.StartUtc : s.StartUtc.ToUniversalTime();
            var file = GetMonthFile(s.CinemaId, startUtc.Year, startUtc.Month);

            var list = ReadMonthFile(file);

            // Tildel globalt unikt Id hvis 0
            if (s.Id == 0) s.Id = TakeNextId();

            list.Add(ToDto(s));
            WriteMonthFile(file, list);
        }

        /// <summary>
        /// Sletter en forestilling baseret på ID.
        /// 1) Søger gennem alle biograf-directories
        /// 2) Søger gennem alle måned-filer
        /// 3) Fjerner forestillingen når fundet
        /// 4) Gemmer opdateret fil
        /// </summary>
        /// <param name="screeningId">ID på forestillingen der skal slettes</param>
        public void Delete(int screeningId)
        {
            // Søg alle cinema-foldere og måned-filer
            if (!Directory.Exists(_baseDir)) return;
            foreach (var cinemaDir in Directory.GetDirectories(_baseDir, "cinema-*"))
            {
                foreach (var file in Directory.GetFiles(cinemaDir, "*.json"))
                {
                    if (Path.GetFileName(file).Equals("index.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var list = ReadMonthFile(file);
                    var removed = list.RemoveAll(x => x.Id == screeningId);
                    if (removed > 0)
                    {
                        WriteMonthFile(file, list);
                        return;
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Private hjælpefunktioner (fil-håndtering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Genererer sti til måned-fil for en specifik biograf.
        /// Format: baseDir/cinema-{cinemaId}/{year:0000}-{month:00}.json
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="year">År</param>
        /// <param name="month">Måned</param>
        /// <returns>Fuld sti til måned-fil</returns>
        private string GetMonthFile(int cinemaId, int year, int month)
        {
            var dir = Path.Combine(_baseDir, $"cinema-{cinemaId}");
            Directory.CreateDirectory(dir);
            var name = $"{year:D4}-{month:D2}.json";
            return Path.Combine(dir, name);
        }

        /// <summary>
        /// Læser forestillings-data fra en måned-fil.
        /// </summary>
        /// <param name="file">Sti til måned-fil</param>
        /// <returns>Liste over ScreeningDto-objekter</returns>
        private List<ScreeningDto> ReadMonthFile(string file)
        {
            if (!File.Exists(file)) return new List<ScreeningDto>();
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<List<ScreeningDto>>(json, _json) ?? new();
        }

        /// <summary>
        /// Skriver forestillings-data til en måned-fil.
        /// Sorterer forestillinger efter starttidspunkt før gemning.
        /// </summary>
        /// <param name="file">Sti til måned-fil</param>
        /// <param name="list">Liste over ScreeningDto-objekter</param>
        private void WriteMonthFile(string file, List<ScreeningDto> list)
        {
            var json = JsonSerializer.Serialize(list.OrderBy(x => x.StartUtc), _json);
            File.WriteAllText(file, json);
        }

        /// <summary>
        /// Sikrer at index-fil eksisterer med initial værdi.
        /// </summary>
        private void EnsureIndex()
        {
            if (File.Exists(_indexFile)) return;
            var dto = new IndexDto { NextId = 1 };
            File.WriteAllText(_indexFile, JsonSerializer.Serialize(dto, _json));
        }

        /// <summary>
        /// Tager næste ledige ID fra index-fil og opdaterer den.
        /// Sikrer globalt unikke ID'er på tværs af alle forestillinger.
        /// </summary>
        /// <returns>Næste ledige ID</returns>
        private int TakeNextId()
        {
            var dto = JsonSerializer.Deserialize<IndexDto>(File.ReadAllText(_indexFile), _json) ?? new IndexDto();
            var current = dto.NextId;
            dto.NextId = Math.Max(1, current + 1);
            File.WriteAllText(_indexFile, JsonSerializer.Serialize(dto, _json));
            return current;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Konverteringshjælpere (DTO(Data Transfer object) ↔ Model)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Konverterer Screening-model til ScreeningDto for JSON-serialisering.
        /// </summary>
        /// <param name="s">Screening-objekt</param>
        /// <returns>ScreeningDto-objekt</returns>
        private static ScreeningDto ToDto(Screening s) => new ScreeningDto
        {
            Id = s.Id,
            CinemaId = s.CinemaId,
            AuditoriumId = s.AuditoriumId,
            MovieId = s.MovieId,
            StartUtc = s.StartUtc,
            AdsMinutes = s.AdsMinutes,
            CleaningMinutes = s.CleaningMinutes
        };
    }
}
