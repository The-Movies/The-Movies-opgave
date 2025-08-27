// System-navneområder (standard)
using System.Collections.Generic;         // List<T>

namespace The_movie_egen.Model.Cinemas
{
    /// <summary>
    /// Domænemodel til en biograf i biograf-systemet.
    /// - Holder grundlæggende biograf-information (navn, by)
    /// - Indeholder en samling af sale (Auditoriums)
    /// - Bruges til både data-binding og JSON-serialisering
    /// </summary>
    public sealed class Cinema
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Public properties (data-binding og serialisering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unikt ID for biografen (auto-genereret af repository).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Biograf-navn (kræves ikke-tom for validering).
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// By hvor biografen er placeret (kræves ikke-tom for validering).
        /// </summary>
        public string City { get; set; } = "";

        /// <summary>
        /// Samling af sale i biografen (navigation property).
        /// Initialiseres som tom liste ved oprettelse.
        /// </summary>
        public List<Auditorium> Auditoriums { get; set; } = new();

        // ─────────────────────────────────────────────────────────────────────────
        //  Override metoder
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// String-repræsentation af biografen (navn og by).
        /// </summary>
        /// <returns>Formateret streng med biograf-information</returns>
        public override string ToString()
        {
            return $"{Name} ({City})";
        }
    }
}

