// System-navneområder (standard)
using System;                             // Exception, InvalidOperationException, DateTime, DateTimeKind
using System.Collections.Generic;         // IEnumerable<T>
using System.Linq;                        // LINQ (Where, OrderBy)

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Screening
using The_movie_egen.Model.Repositories;  // IMovieRepository, IScreeningRepository

namespace The_movie_egen.Services
{
    /// <summary>
    /// Service til håndtering af forestillings-planlægning og tidsplaner.
    /// - Opretter og fjerner forestillinger med validering
    /// - Forhindrer overlap mellem forestillinger i samme sal
    /// - Håndterer UTC/Local tid-konvertering
    /// - Validerer film-eksistens og tidsplan-konflikter
    /// - Koordinerer mellem MovieRepository og ScreeningRepository
    /// </summary>
    public sealed class SchedulingService
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter (dependencies)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Repository til film-data (injiceret via constructor).
        /// </summary>
        private readonly IMovieRepository _movies;

        /// <summary>
        /// Repository til forestillings-data (injiceret via constructor).
        /// </summary>
        private readonly IScreeningRepository _screenings;

        /// <summary>
        /// Ctor: initialiserer service med nødvendige repositories.
        /// </summary>
        /// <param name="movies">Repository til film-data</param>
        /// <param name="screenings">Repository til forestillings-data</param>
        public SchedulingService(IMovieRepository movies, IScreeningRepository screenings)
        {
            _movies = movies;
            _screenings = screenings;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (forestillings-håndtering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Opretter en ny forestilling i en given biograf/sal.
        /// 1) Validerer at filmen eksisterer
        /// 2) Konverterer lokal tid til UTC
        /// 3) Tjekker for overlap med eksisterende forestillinger
        /// 4) Opretter og gemmer forestillingen
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="auditoriumId">ID på salen</param>
        /// <param name="movieId">ID på filmen</param>
        /// <param name="startLocal">Starttid i lokal tid (DateTimeKind.Local eller Unspecified)</param>
        /// <param name="adsMinutes">Minutter til reklamer (default: 15)</param>
        /// <param name="cleaningMinutes">Minutter til rengøring (default: 15)</param>
        /// <returns>Den oprettede forestilling</returns>
        /// <exception cref="InvalidOperationException">Hvis film ikke findes eller der er overlap</exception>
        public Screening AddScreening(
            int cinemaId,
            int auditoriumId,
            int movieId,
            DateTime startLocal,
            int adsMinutes = 15,
            int cleaningMinutes = 15)
        {
            // Valider at filmen eksisterer
            var movie = _movies.GetById(movieId)
                        ?? throw new InvalidOperationException("Film findes ikke.");

            // Sikr at input tolkes som lokal tid, også hvis Kind==Unspecified
            if (startLocal.Kind == DateTimeKind.Unspecified)
                startLocal = DateTime.SpecifyKind(startLocal, DateTimeKind.Local);

            // Konverter til UTC før lagring
            var startUtc = startLocal.ToUniversalTime();
            var candidateEndUtc = Screening.CalcEnd(startUtc, movie.DurationMin, adsMinutes, cleaningMinutes);

            // Hent eksisterende forestillinger i samme måned og sal
            var existing = _screenings
                .GetForCinemaMonth(cinemaId, startLocal.Year, startLocal.Month)
                .Where(s => s.AuditoriumId == auditoriumId);

            // Tjek for overlap: A.start < B.end && B.start < A.end
            foreach (var s in existing)
            {
                if (startUtc < s.EndUtc && s.StartUtc < candidateEndUtc)
                    throw new InvalidOperationException("Overlap i tidsplanen for denne sal.");
            }

            // Opret forestilling
            var screening = new Screening
            {
                CinemaId = cinemaId,
                AuditoriumId = auditoriumId,
                MovieId = movieId,
                StartUtc = startUtc,

                // EndUtc sættes IKKE — beregnes automatisk i modellen
                AdsMinutes = adsMinutes,
                CleaningMinutes = cleaningMinutes,

                // Valgfrit: sæt navigation for nem visning efterfølgende
                Movie = movie
            };

            _screenings.Add(screening);
            return screening;
        }

        /// <summary>
        /// Fjerner en forestilling baseret på ID.
        /// </summary>
        /// <param name="screeningId">ID på forestillingen der skal fjernes</param>
        public void RemoveScreening(int screeningId) => _screenings.Delete(screeningId);

        /// <summary>
        /// Henter alle forestillinger for en biograf i en given måned.
        /// Forestillinger returneres sorteret efter starttidspunkt.
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="year">År (fx 2025)</param>
        /// <param name="month">Måned (1-12)</param>
        /// <returns>Sorteret liste over forestillinger for måneden</returns>
        public IEnumerable<Screening> GetMonth(int cinemaId, int year, int month)
            => _screenings.GetForCinemaMonth(cinemaId, year, month)
                          .OrderBy(s => s.StartUtc);
    }
}
