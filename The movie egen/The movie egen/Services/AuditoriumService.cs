// System-navneområder (standard)
using System;                             // Exception, ArgumentException, DateTime
using System.Collections.Generic;         // IEnumerable<T>
using System.Linq;                        // LINQ (SelectMany, FirstOrDefault, Where, Any)

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Screening
using The_movie_egen.Model.Cinemas;       // Auditorium
using The_movie_egen.Model.Repositories;  // ICinemaRepository, IScreeningRepository

namespace The_movie_egen.Services
{
    /// <summary>
    /// Service til håndtering af biograf-sale og deres forretningsregler.
    /// - Validerer sal-data før persistering
    /// - Håndterer CRUD-operationer for sale
    /// - Forhindrer sletning af sale med fremtidige forestillinger
    /// - Koordinerer mellem CinemaRepository og ScreeningRepository
    /// - Implementerer forretningslogik og validering
    /// </summary>
    public sealed class AuditoriumService
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter (dependencies)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Repository til biograf-data (injiceret via constructor).
        /// </summary>
        private readonly ICinemaRepository _cinemaRepository;

        /// <summary>
        /// Repository til forestillings-data (injiceret via constructor).
        /// </summary>
        private readonly IScreeningRepository _screeningRepository;

        /// <summary>
        /// Ctor: initialiserer service med nødvendige repositories.
        /// </summary>
        /// <param name="cinemaRepository">Repository til biograf-data</param>
        /// <param name="screeningRepository">Repository til forestillings-data</param>
        public AuditoriumService(ICinemaRepository cinemaRepository, IScreeningRepository screeningRepository)
        {
            _cinemaRepository = cinemaRepository;
            _screeningRepository = screeningRepository;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (CRUD operations)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tilføjer en ny sal til en biograf.
        /// 1) Validerer input-data (navn og pladser)
        /// 2) Kalder repository til at oprette salen
        /// 3) Returnerer den oprettede sal med tildelt ID
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="name">Sal-navn</param>
        /// <param name="seats">Antal pladser</param>
        /// <returns>Den oprettede sal</returns>
        /// <exception cref="ArgumentException">Hvis validering fejler</exception>
        public Auditorium AddAuditorium(int cinemaId, string name, int seats)
        {
            ValidateAuditoriumData(name, seats);
            
            return _cinemaRepository.AddAuditorium(cinemaId, name, seats);
        }

        /// <summary>
        /// Opdaterer en eksisterende sal.
        /// 1) Validerer input-data (navn og pladser)
        /// 2) Kalder repository til at opdatere salen
        /// </summary>
        /// <param name="auditoriumId">ID på salen</param>
        /// <param name="name">Nyt sal-navn</param>
        /// <param name="seats">Nyt antal pladser</param>
        /// <exception cref="ArgumentException">Hvis validering fejler</exception>
        public void UpdateAuditorium(int auditoriumId, string name, int seats)
        {
            ValidateAuditoriumData(name, seats);
            
            _cinemaRepository.UpdateAuditorium(auditoriumId, name, seats);
        }

        /// <summary>
        /// Sletter en sal hvis den ikke har fremtidige forestillinger.
        /// 1) Tjekker om salen har planlagte forestillinger
        /// 2) Kaster exception hvis sletning ikke er tilladt
        /// 3) Kalder repository til at slette salen
        /// </summary>
        /// <param name="auditoriumId">ID på salen der skal slettes</param>
        /// <returns>True hvis salen blev slettet</returns>
        /// <exception cref="InvalidOperationException">Hvis salen har fremtidige forestillinger</exception>
        public bool DeleteAuditorium(int auditoriumId)
        {
            // Tjek om salen har fremtidige forestillinger
            if (HasFutureScreenings(auditoriumId))
            {
                throw new InvalidOperationException("Salen kan ikke slettes, da den har planlagte forestillinger i fremtiden.");
            }

            return _cinemaRepository.DeleteAuditorium(auditoriumId);
        }

        /// <summary>
        /// Tjekker om en sal kan slettes (ingen fremtidige forestillinger).
        /// Bruges til at aktivere/deaktivere slet-knapper i UI.
        /// </summary>
        /// <param name="auditoriumId">ID på salen</param>
        /// <returns>True hvis salen kan slettes</returns>
        public bool CanDeleteAuditorium(int auditoriumId)
        {
            return !HasFutureScreenings(auditoriumId);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Private hjælpefunktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tjekker om en sal har fremtidige forestillinger.
        /// Søger gennem de næste 12 måneder for at finde planlagte forestillinger.
        /// </summary>
        /// <param name="auditoriumId">ID på salen</param>
        /// <returns>True hvis salen har fremtidige forestillinger</returns>
        private bool HasFutureScreenings(int auditoriumId)
        {
            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentMonth = now.Month;

            // Tjek nuværende måned og de næste 12 måneder
            for (int i = 0; i < 12; i++)
            {
                var year = currentYear + ((currentMonth + i - 1) / 12);
                var month = ((currentMonth + i - 1) % 12) + 1;

                // Hent alle biografer for at finde hvilken biograf salen tilhører
                var cinemas = _cinemaRepository.GetAll();
                var auditorium = cinemas.SelectMany(c => c.Auditoriums).FirstOrDefault(a => a.Id == auditoriumId);
                if (auditorium == null) continue;

                // Hent forestillinger for denne måned og tjek for fremtidige
                var screenings = _screeningRepository.GetForCinemaMonth(auditorium.CinemaId, year, month)
                    .Where(s => s.AuditoriumId == auditoriumId && s.StartUtc > now);

                if (screenings.Any())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Validerer sal-data før persistering.
        /// Kaster ArgumentException hvis data er ugyldigt.
        /// </summary>
        /// <param name="name">Sal-navn at validere</param>
        /// <param name="seats">Antal pladser at validere</param>
        /// <exception cref="ArgumentException">Hvis validering fejler</exception>
        private static void ValidateAuditoriumData(string name, int seats)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Salnavn er påkrævet.");

            if (seats <= 0)
                throw new ArgumentException("Kapacitet skal være større end 0.");
        }
    }
}
