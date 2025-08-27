// System-navneområder (standard)
using System;                             // Exception, ArgumentException, ArgumentOutOfRangeException

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Movie
using The_movie_egen.Model.Enums;         // Genre
using The_movie_egen.Model.Repositories;  // IMovieRepository

namespace The_movie_egen.Services
{
    /// <summary>
    /// Applikationsservice til film-registrering og domæneregler.
    /// - Validerer film-data før oprettelse
    /// - Håndhæver forretningsregler (unikke titler, genre-krav)
    /// - Holder UI og model rene ved at centralisere validering
    /// - Implementerer forretningslogik for film-håndtering
    /// </summary>
    public sealed class MovieRegistry
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter (dependencies)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Repository til film-data (injiceret via constructor).
        /// </summary>
        private readonly IMovieRepository _repo;

        /// <summary>
        /// Ctor: initialiserer registry med film-repository.
        /// </summary>
        /// <param name="repo">Repository til film-data</param>
        public MovieRegistry(IMovieRepository repo) => _repo = repo;

        // ─────────────────────────────────────────────────────────────────────────
        //  Public API (film-registrering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Registrerer en ny film med omfattende validering.
        /// 1) Validerer titel (ikke-tom, max 200 tegn)
        /// 2) Validerer varighed (1-600 minutter)
        /// 3) Validerer genrer (mindst én genre)
        /// 4) Tjekker for duplikater (unik titel)
        /// 5) Opretter og gemmer filmen
        /// </summary>
        /// <param name="title">Filmtitel (trimmes automatisk)</param>
        /// <param name="durationMin">Film-varighed i minutter</param>
        /// <param name="genres">Film-genrer (Flags-enum)</param>
        /// <returns>Den oprettede film med tildelt ID</returns>
        /// <exception cref="ArgumentException">Hvis titel eller genrer er ugyldige</exception>
        /// <exception cref="ArgumentOutOfRangeException">Hvis varighed er uden for tilladt interval</exception>
        /// <exception cref="InvalidOperationException">Hvis der allerede findes en film med samme titel</exception>
        public Movie RegisterMovie(string title, int durationMin, Genre genres)
        {
            // ─────────────────────────────────────────────────────────────────────
            //  Validering af input-data
            // ─────────────────────────────────────────────────────────────────────

            // Valider titel (ikke-tom, max 200 tegn)
            if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
                throw new ArgumentException("Titel er påkrævet (max 200 tegn).", nameof(title));

            // Valider varighed (1-600 minutter)
            if (durationMin < 1 || durationMin > 600)
                throw new ArgumentOutOfRangeException(nameof(durationMin), "Varighed skal være 1–600 min.");

            // Valider genrer (mindst én genre)
            if (genres == Genre.None)
                throw new ArgumentException("Vælg mindst én genre.", nameof(genres));

            // ─────────────────────────────────────────────────────────────────────
            //  Forretningsregler
            // ─────────────────────────────────────────────────────────────────────

            // Regel: ingen aktiv film med samme titel (case-insensitive)
            if (_repo.ExistsActiveTitle(title))
                throw new InvalidOperationException("Der findes allerede en aktiv film med denne titel.");

            // ─────────────────────────────────────────────────────────────────────
            //  Oprettelse og persistering
            // ─────────────────────────────────────────────────────────────────────

            var movie = new Movie(title.Trim(), durationMin, genres);
            _repo.Add(movie);
            return movie;
        }
    }
}
