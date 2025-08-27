// System-navneområder (standard)
using System.Diagnostics;                 // Debug, Debugger

// Domænemodeller
using The_movie_egen.Model.Enums;         // Genre, FilmStatus

namespace The_movie_egen.Model
{
    /// <summary>
    /// Domænemodel til en film i biograf-systemet.
    /// - Holder grundlæggende film-information (titel, varighed, genrer)
    /// - Understøtter multiple genrer via Flags-enum
    /// - Håndterer film-status (aktiv/arkiveret)
    /// - Bruges til både data-binding og JSON-serialisering
    /// </summary>
    public sealed class Movie
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Public properties (data-binding og serialisering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unikt ID for filmen (auto-genereret af repository).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Filmtitel (kræves ikke-tom for validering).
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Film-varighed i minutter (skal være > 0 for validering).
        /// </summary>
        public int DurationMin { get; set; }

        /// <summary>
        /// Film-genrer via Flags-enum (kan kombinere flere).
        /// Eksempel: Action | Comedy | Drama
        /// Default: Genre.None (skal valideres i service)
        /// </summary>
        public Genre Genres { get; set; } = Genre.None;

        /// <summary>
        /// Film-status (aktiv eller arkiveret).
        /// Default: FilmStatus.Active (nye film er aktive)
        /// </summary>
        public FilmStatus Status { get; set; } = FilmStatus.Active;

        // ─────────────────────────────────────────────────────────────────────────
        //  Constructors
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tom constructor til WPF-binding og JSON-serialisering.
        /// Properties sættes via data-binding eller deserialisering.
        /// </summary>
        public Movie() { }

        /// <summary>
        /// Convenience-constructor til manuel oprettelse af film.
        /// Bemærk: Service bør normalt håndtere film-oprettelse med validering.
        /// </summary>
        /// <param name="title">Filmtitel</param>
        /// <param name="durationMin">Varighed i minutter</param>
        /// <param name="genres">Film-genrer (Flags-enum)</param>
        /// <param name="status">Film-status (default: Active)</param>
        public Movie(string title, int durationMin, Genre genres, FilmStatus status = FilmStatus.Active)
        {
            Title = title;
            DurationMin = durationMin;
            Genres = genres;   // kræv evt. != Genre.None i din service
            Status = status;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Override metoder
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// String-repræsentation af filmen (titel).
        /// Bruges til debugging og UI-visning.
        /// </summary>
        /// <returns>Filmtitel</returns>
        public override string ToString()
        {
            return Title;
        }
    }
}
