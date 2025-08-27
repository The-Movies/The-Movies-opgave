namespace The_movie_egen.Model.Cinemas
{
    /// <summary>
    /// Domænemodel til en sal i en biograf.
    /// - Holder grundlæggende sal-information (navn, antal pladser)
    /// - Tilknyttet til en specifik biograf via CinemaId
    /// - Bruges til både data-binding og JSON-serialisering
    /// </summary>
    public sealed class Auditorium
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Public properties (data-binding og serialisering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unikt ID for salen (auto-genereret af repository).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Sal-navn (fx "Sal 1", "Storsalen", "Studiosalen").
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Antal pladser i salen (skal være > 0 for validering).
        /// </summary>
        public int Seats { get; set; }

        /// <summary>
        /// ID på biografen som salen tilhører (foreign key).
        /// </summary>
        public int CinemaId { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Override metoder
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// String-repræsentation af salen (navn og antal pladser).
        /// </summary>
        /// <returns>Formateret streng med sal-information</returns>
        public override string ToString()
        {
            return $"{Name} ({Seats} pladser)";
        }
    }
}
