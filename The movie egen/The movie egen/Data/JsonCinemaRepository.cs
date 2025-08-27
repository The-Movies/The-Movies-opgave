// System-navneområder (standard)
using System;                             // Exception, ArgumentException, etc.
using System.Collections.Generic;         // List<T>, IEnumerable<T>
using System.IO;                          // File, Directory, Path
using System.Linq;                        // LINQ (FirstOrDefault, Any, Max, SelectMany)
using System.Text.Json;                   // JsonSerializer, JsonSerializerOptions

// Domænemodeller og infrastruktur
using The_movie_egen.Infrastructure;      // SeedData
using The_movie_egen.Model.Cinemas;       // Cinema, Auditorium
using The_movie_egen.Model.Repositories;  // ICinemaRepository

namespace The_movie_egen.Data.Json
{
    /// <summary>
    /// JSON-baseret repository til persistering af biograf-data.
    /// - Gemmer biografer og deres auditorier i JSON-fil på disk
    /// - Håndterer CRUD-operationer for både biografer og auditorier
    /// - Automatisk ID-generering for auditorier
    /// - Seed-data oprettelse ved første kørsel
    /// - Fejlhåndtering med meningsfulde exception-beskeder
    /// - Validering af eksistens før operationer
    /// </summary>
    public sealed class JsonCinemaRepository : ICinemaRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter og konfiguration
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sti til JSON-filen hvor biograf-data gemmes.
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// JSON-serialiseringsindstillinger for læsbar output.
        /// - Case-insensitive property matching
        /// - Indenteret JSON for læsbarhed
        /// </summary>
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        /// <summary>
        /// Ctor: initialiserer repository og sikrer at directory eksisterer.
        /// </summary>
        /// <param name="filePath">Relativ sti til JSON-fil (fx "cinemas.json")</param>
        public JsonCinemaRepository(string filePath)
        {
            _filePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, filePath));
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (ICinemaRepository implementation)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sikrer at seed-data eksisterer ved at oprette fil hvis den ikke findes.
        /// Kaldes typisk ved applikationens opstart.
        /// </summary>
        public void EnsureSeeded()
        {
            if (!File.Exists(_filePath))
            {
                var seed = GetSeed();
                SaveAll(seed);
            }
        }

        /// <summary>
        /// Henter alle biografer fra JSON-filen.
        /// </summary>
        /// <returns>Liste over alle biografer, eller tom liste hvis fil ikke findes</returns>
        public IEnumerable<Cinema> GetAll()
        {
            if (!File.Exists(_filePath)) 
            {
                return Enumerable.Empty<Cinema>();
            }
            
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Cinema>>(json, _json) ?? new List<Cinema>();
        }

        /// <summary>
        /// Finder en biograf baseret på ID.
        /// </summary>
        /// <param name="id">Biograf-ID at søge efter</param>
        /// <returns>Biograf hvis fundet, ellers null</returns>
        public Cinema? GetById(int id) => GetAll().FirstOrDefault(c => c.Id == id);

        /// <summary>
        /// Gemmer alle biografer til JSON-filen.
        /// Overskriver eksisterende fil med nye data.
        /// </summary>
        /// <param name="cinemas">Liste over biografer der skal gemmes</param>
        public void SaveAll(IEnumerable<Cinema> cinemas)
        {
            var json = JsonSerializer.Serialize(cinemas, _json);
            File.WriteAllText(_filePath, json);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Auditorium operationer
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tilføjer et nyt auditorium til en specifik biograf.
        /// 1) Validerer at biografen eksisterer
        /// 2) Genererer automatisk et unikt ID for auditoriet
        /// 3) Opretter nyt Auditorium-objekt
        /// 4) Tilføjer til biografens auditorium-liste
        /// 5) Gemmer ændringer til JSON-fil
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="name">Navn på auditoriet</param>
        /// <param name="seats">Antal sæder i auditoriet</param>
        /// <returns>Det nye auditorium-objekt</returns>
        /// <exception cref="ArgumentException">Hvis biografen ikke findes</exception>
        public Auditorium AddAuditorium(int cinemaId, string name, int seats)
        {
            var cinemas = GetAll().ToList();
            var cinema = cinemas.FirstOrDefault(c => c.Id == cinemaId);
            if (cinema == null)
                throw new ArgumentException($"Cinema with ID {cinemaId} not found");

            // Find next available auditorium ID - handle empty list
            var existingAuditoriums = cinemas.SelectMany(c => c.Auditoriums);
            var nextId = existingAuditoriums.Any() ? existingAuditoriums.Max(a => a.Id) + 1 : 1;
            
            var auditorium = new Auditorium
            {
                Id = nextId,
                CinemaId = cinemaId,
                Name = name,
                Seats = seats
            };

            cinema.Auditoriums.Add(auditorium);
            SaveAll(cinemas);
            
            return auditorium;
        }

        /// <summary>
        /// Opdaterer et eksisterende auditorium.
        /// 1) Finder auditoriet baseret på ID
        /// 2) Opdaterer navn og antal sæder
        /// 3) Gemmer ændringer til JSON-fil
        /// </summary>
        /// <param name="auditoriumId">ID på auditoriet der skal opdateres</param>
        /// <param name="name">Nyt navn på auditoriet</param>
        /// <param name="seats">Nyt antal sæder</param>
        /// <exception cref="ArgumentException">Hvis auditoriet ikke findes</exception>
        public void UpdateAuditorium(int auditoriumId, string name, int seats)
        {
            var cinemas = GetAll().ToList();
            var auditorium = cinemas.SelectMany(c => c.Auditoriums).FirstOrDefault(a => a.Id == auditoriumId);
            if (auditorium == null)
                throw new ArgumentException($"Auditorium with ID {auditoriumId} not found");

            auditorium.Name = name;
            auditorium.Seats = seats;
            
            SaveAll(cinemas);
        }

        /// <summary>
        /// Sletter et auditorium fra en biograf.
        /// 1) Finder biografen der indeholder auditoriet
        /// 2) Fjerner auditoriet fra biografens liste
        /// 3) Gemmer ændringer til JSON-fil
        /// </summary>
        /// <param name="auditoriumId">ID på auditoriet der skal slettes</param>
        /// <returns>True hvis auditoriet blev slettet, false hvis det ikke fandtes</returns>
        public bool DeleteAuditorium(int auditoriumId)
        {
            var cinemas = GetAll().ToList();
            var cinema = cinemas.FirstOrDefault(c => c.Auditoriums.Any(a => a.Id == auditoriumId));
            if (cinema == null)
                return false;

            var auditorium = cinema.Auditoriums.FirstOrDefault(a => a.Id == auditoriumId);
            if (auditorium == null)
                return false;

            var removed = cinema.Auditoriums.Remove(auditorium);
            if (removed)
            {
                SaveAll(cinemas);
            }
            
            return removed;
        }

        /// <summary>
        /// Finder et auditorium baseret på ID.
        /// Søger gennem alle biografer for at finde det specifikke auditorium.
        /// </summary>
        /// <param name="auditoriumId">ID på auditoriet</param>
        /// <returns>Auditorium hvis fundet, ellers null</returns>
        public Auditorium? GetAuditoriumById(int auditoriumId)
        {
            return GetAll().SelectMany(c => c.Auditoriums).FirstOrDefault(a => a.Id == auditoriumId);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Seed-data (bruger Infrastructure.SeedData)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter seed-data fra Infrastructure.SeedData klassen.
        /// Konverterer ObservableCollection til List for JSON-serialisering.
        /// Bruges kun når JSON-filen ikke eksisterer og skal oprettes første gang.
        /// </summary>
        /// <returns>Liste over seed-biografer med deres auditorier</returns>
        private static List<Cinema> GetSeed()
        {
            // Brug SeedData fra Infrastructure og konverter til List
            var seedCinemas = SeedData.Cinemas();
            return seedCinemas.ToList();
        }
    }
}
