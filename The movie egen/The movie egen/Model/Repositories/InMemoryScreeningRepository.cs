// System-navneområder (standard)
using System;                             // Exception, DateTime, etc.
using System.Collections.Generic;         // List<T>, IEnumerable<T>
using System.Linq;                        // LINQ (Where, OrderBy, FindIndex)

// Domænemodeller og repositories
using The_movie_egen.Model;               // Screening
using The_movie_egen.Model.Repositories;  // IScreeningRepository

namespace The_movie_egen.Model.Repositories
{
    /// <summary>
    /// Enkel in-memory implementation af IScreeningRepository.
    /// - Gemmer forestillinger i hukommelsen (ikke persistent)
    /// - Håndterer CRUD-operationer med automatisk ID-generering
    /// - Filtrering pr. biograf og måned
    /// - Sortering efter starttidspunkt
    /// - God til testing og når persistence ikke er nødvendig
    /// - Alle data går tabt når applikationen lukkes
    /// </summary>
    public sealed class InMemoryScreeningRepository : IScreeningRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter og konfiguration
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// In-memory liste over alle forestillinger.
        /// Alle data gemmes kun i hukommelsen og går tabt ved applikationsstop.
        /// </summary>
        private readonly List<Screening> _items = new();

        /// <summary>
        /// Næste ledige ID til nye forestillinger (auto-increment).
        /// Starter på 1 og øges automatisk for hver ny forestilling.
        /// </summary>
        private int _nextId = 1;

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (IScreeningRepository implementation)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle forestillinger for en specifik biograf og måned.
        /// 1) Filtrerer forestillinger baseret på cinemaId, year og month
        /// 2) Sorterer resultatet efter starttidspunkt (ældste først)
        /// 3) Returnerer tom liste hvis ingen forestillinger findes
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="year">År (fx 2025)</param>
        /// <param name="month">Måned (1-12)</param>
        /// <returns>Liste over forestillinger for den måned, sorteret efter starttidspunkt</returns>
        public IEnumerable<Screening> GetForCinemaMonth(int cinemaId, int year, int month)
            => _items
               .Where(s => s.CinemaId == cinemaId &&
                           s.StartUtc.Year == year &&
                           s.StartUtc.Month == month)
               .OrderBy(s => s.StartUtc);

        /// <summary>
        /// Tilføjer en ny forestilling til repository.
        /// 1) Tildeler automatisk et unikt ID
        /// 2) Tilføjer forestillingen til in-memory listen
        /// 3) Forestillingen er nu tilgængelig for søgninger
        /// </summary>
        /// <param name="s">Forestillingen der skal tilføjes</param>
        public void Add(Screening s)
        {
            s.Id = _nextId++;
            _items.Add(s);
        }

        /// <summary>
        /// Sletter en forestilling baseret på ID.
        /// 1) Finder forestillingen i listen baseret på ID
        /// 2) Fjerner den fra listen hvis den findes
        /// 3) Gør intet hvis forestillingen ikke findes
        /// </summary>
        /// <param name="id">ID på forestillingen der skal slettes</param>
        public void Delete(int id)
        {
            var i = _items.FindIndex(x => x.Id == id);
            if (i >= 0) _items.RemoveAt(i);
        }
    }
}
