// Domænemodeller
using The_movie_egen.Model;               // Movie
using The_movie_egen.Model.Enums;         // Genre

namespace The_movie_egen.Model.Extensions
{
    /// <summary>
    /// Extension metoder til Movie for nem håndtering af Flags-enum genrer.
    /// - Giver intuitive metoder til at arbejde med multiple genrer
    /// - Understøtter bitwise operationer på Genre-flags
    /// - Forudsætter at Genre er defineret som [Flags]-enum
    /// - Bruges i UI og business logic for genre-håndtering
    /// 
    /// Brugseksempler:
    ///   movie.AddGenre(Genre.Action);
    ///   if (movie.HasGenre(Genre.SciFi)) { ... }
    ///   movie.RemoveGenre(Genre.Comedy);
    ///   movie.ToggleGenre(Genre.Fantasy);
    /// </summary>
    public static class MovieGenreExtensions
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Query operations (læsning af genre-status)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tjekker om et bestemt genre-flag er sat.
        /// Bruger bitwise AND operation for at teste flag-status.
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <param name="g">Genre-flag at teste</param>
        /// <returns>True hvis genren er tildelt, false ellers</returns>
        /// <example>
        /// if (movie.HasGenre(Genre.Action)) { /* film er action */ }
        /// </example>
        public static bool HasGenre(this Movie m, Genre g) => (m.Genres & g) == g;

        // ─────────────────────────────────────────────────────────────────────────
        //  Mutation operations (ændring af genre-status)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tilføjer et genre-flag til filmen.
        /// Bruger bitwise OR operation - idempotent (gentagne kald skader ikke).
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <param name="g">Genre-flag at tilføje</param>
        /// <example>
        /// movie.AddGenre(Genre.Comedy);  // tilføjer komedie
        /// movie.AddGenre(Genre.Action);  // tilføjer action (nu: Comedy | Action)
        /// </example>
        public static void AddGenre(this Movie m, Genre g) => m.Genres |= g;

        /// <summary>
        /// Fjerner et genre-flag fra filmen.
        /// Bruger bitwise AND med komplement - fjerner flag uanset oprindelig status.
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <param name="g">Genre-flag at fjerne</param>
        /// <example>
        /// movie.RemoveGenre(Genre.Horror);  // fjerner horror
        /// </example>
        public static void RemoveGenre(this Movie m, Genre g) => m.Genres &= ~g;

        /// <summary>
        /// Toggler et genre-flag (slår det til hvis slukket, slukker det hvis tændt).
        /// Bruger bitwise XOR operation.
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <param name="g">Genre-flag at toggle</param>
        /// <example>
        /// movie.ToggleGenre(Genre.SciFi);  // slår SciFi til hvis slukket, ellers fra
        /// </example>
        public static void ToggleGenre(this Movie m, Genre g) => m.Genres ^= g;

        // ─────────────────────────────────────────────────────────────────────────
        //  Utility operations
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tjekker om filmen har mindst én genre tildelt.
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <returns>True hvis filmen har mindst én genre, false hvis Genre.None</returns>
        public static bool HasAnyGenre(this Movie m) => m.Genres != Genre.None;

        /// <summary>
        /// Tæller antal genrer tildelt til filmen.
        /// </summary>
        /// <param name="m">Movie-objektet</param>
        /// <returns>Antal genrer (0 hvis ingen)</returns>
        public static int GenreCount(this Movie m)
        {
            var genres = m.Genres;
            int count = 0;
            while (genres != Genre.None)
            {
                if ((genres & Genre.Action) != 0) { count++; genres &= ~Genre.Action; }
                if ((genres & Genre.Comedy) != 0) { count++; genres &= ~Genre.Comedy; }
                if ((genres & Genre.Drama) != 0) { count++; genres &= ~Genre.Drama; }
                if ((genres & Genre.Horror) != 0) { count++; genres &= ~Genre.Horror; }
                if ((genres & Genre.Romance) != 0) { count++; genres &= ~Genre.Romance; }
                if ((genres & Genre.SciFi) != 0) { count++; genres &= ~Genre.SciFi; }
                if ((genres & Genre.Thriller) != 0) { count++; genres &= ~Genre.Thriller; }
                if ((genres & Genre.Documentary) != 0) { count++; genres &= ~Genre.Documentary; }
                if ((genres & Genre.Crime) != 0) { count++; genres &= ~Genre.Crime; }
                if ((genres & Genre.Animation) != 0) { count++; genres &= ~Genre.Animation; }
                if ((genres & Genre.Adventure) != 0) { count++; genres &= ~Genre.Adventure; }
                if ((genres & Genre.Fantasy) != 0) { count++; genres &= ~Genre.Fantasy; }
            }
            return count;
        }
    }
}