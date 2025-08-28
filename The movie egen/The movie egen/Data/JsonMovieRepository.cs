// System-navneområder (standard)
using System;                             // Exception, ArgumentException, etc.
using System.Collections.Generic;         // List<T>, IReadOnlyList<T>
using System.IO;                          // File, Directory, Path
using System.Linq;                        // LINQ (OrderBy, Max, FirstOrDefault, Any)
using System.Text.Json;                   // JsonSerializer, JsonSerializerOptions
using System.Text.Json.Serialization;     // JsonStringEnumConverter

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Movie
using The_movie_egen.Model.Enums;         // FilmStatus
using The_movie_egen.Model.Repositories;  // IMovieRepository

namespace The_movie_egen.Data
{
    /// <summary>
    /// JSON-baseret repository til persistering af film-data.
    /// - Gemmer film i JSON-fil på disk
    /// - Håndterer CRUD-operationer (Create, Read, Update, Delete)
    /// - Automatisk ID-generering og sortering
    /// - Validering af unikke titler for aktive film
    /// - Fejlhåndtering med meningsfulde exception-beskeder
    /// </summary>
    public sealed class JsonMovieRepository : IMovieRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter og konfiguration
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sti til JSON-filen hvor film-data gemmes.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// In-memory liste over alle film (synkroniseres med JSON-fil).
        /// </summary>
        private readonly List<Movie> _items;

        /// <summary>
        /// Næste ledige ID til nye film (auto-increment).
        /// </summary>
        private int _nextId;

        /// <summary>
        /// Event that is raised when movies are added, updated, or deleted.
        /// </summary>
        public event EventHandler? MoviesChanged;

        /// <summary>
        /// JSON-serialiseringsindstillinger for læsbar output.
        /// - Indenteret JSON for læsbarhed
        /// - Enum-værdier som tekst (fx "Action, Comedy" i stedet for tal)
        /// </summary>
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Ctor: initialiserer repository og indlæser eksisterende data.
        /// </summary>
        /// <param name="path">Relativ sti til JSON-fil (fx "movies.json")</param>
        /// <exception cref="ArgumentException">Hvis path er null eller tom</exception>
        public JsonMovieRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
                
            _path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
            _items = Load(_path);
            _nextId = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (IMovieRepository implementation)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle film sorteret alfabetisk efter titel.
        /// </summary>
        /// <returns>Read-only liste over alle film</returns>
        public IReadOnlyList<Movie> GetAll() => _items.OrderBy(x => x.Title).ToList();

        /// <summary>
        /// Finder en film baseret på ID.
        /// </summary>
        /// <param name="id">Film-ID at søge efter</param>
        /// <returns>Film hvis fundet, ellers null</returns>
        public Movie? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// Tilføjer en ny film til repository.
        /// 1) Tildeler automatisk et unikt ID
        /// 2) Tilføjer til in-memory liste
        /// 3) Gemmer til JSON-fil
        /// </summary>
        /// <param name="movie">Filmen der skal tilføjes</param>
        public void Add(Movie movie)
        {
            movie.Id = _nextId++;
            _items.Add(movie);
            Save();
            MoviesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Opdaterer en eksisterende film.
        /// 1) Finder filmen baseret på ID
        /// 2) Erstatter med ny data
        /// 3) Gemmer til JSON-fil
        /// </summary>
        /// <param name="movie">Film med opdateret data</param>
        /// <exception cref="KeyNotFoundException">Hvis film ikke findes</exception>
        public void Update(Movie movie)
        {
            var i = _items.FindIndex(x => x.Id == movie.Id);
            if (i < 0) throw new KeyNotFoundException($"Movie {movie.Id} not found");
            _items[i] = movie;
            Save();
            MoviesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sletter en film baseret på ID.
        /// 1) Finder filmen i listen
        /// 2) Fjerner fra in-memory liste
        /// 3) Gemmer til JSON-fil
        /// </summary>
        /// <param name="id">ID på filmen der skal slettes</param>
        public void Delete(int id)
        {
            var idx = _items.FindIndex(x => x.Id == id);
            if (idx >= 0)
            {
                _items.RemoveAt(idx);
                Save();
                MoviesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Tjekker om der findes en aktiv film med samme titel.
        /// Bruges til validering - forhindrer duplikater af aktive film.
        /// </summary>
        /// <param name="title">Titel at søge efter</param>
        /// <returns>True hvis der findes en aktiv film med denne titel</returns>
        public bool ExistsActiveTitle(string title) =>
            _items.Any(x => x.Status == FilmStatus.Active &&
                            string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));

        // ─────────────────────────────────────────────────────────────────────────
        //  Private hjælpefunktioner (I/O)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gemmer alle film til JSON-fil på disk.
        /// 1) Opretter directory hvis den ikke findes
        /// 2) Serialiserer film-liste til JSON
        /// 3) Skriver til fil
        /// </summary>
        /// <exception cref="InvalidOperationException">Hvis gemning fejler</exception>
        private void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(_path, JsonSerializer.Serialize(_items, JsonOpts));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save movies to {_path}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Indlæser film fra JSON-fil på disk.
        /// 1) Tjekker om filen findes
        /// 2) Læser JSON-indhold
        /// 3) Deserialiserer til List<Movie>
        /// </summary>
        /// <param name="path">Sti til JSON-fil</param>
        /// <returns>Liste over film (tom hvis fil ikke findes)</returns>
        /// <exception cref="InvalidOperationException">Hvis indlæsning fejler</exception>
        private static List<Movie> Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return new List<Movie>();
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<Movie>>(json, JsonOpts) ?? new List<Movie>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load movies from {path}: {ex.Message}", ex);
            }
        }
    }
}
