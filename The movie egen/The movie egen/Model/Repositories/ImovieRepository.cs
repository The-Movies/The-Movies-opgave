// System-navneområder (standard)
using System.Collections.Generic;         // IReadOnlyList<T>

// Domænemodeller
using The_movie_egen.Model;               // Movie

namespace The_movie_egen.Model.Repositories
{
    /// <summary>
    /// Repository interface til persistering af film-data.
    /// - Definerer CRUD-operationer for film
    /// - Implementeres af JsonMovieRepository
    /// - Bruges til dependency injection og testability
    /// - Separerer data-access fra business logic
    /// </summary>
    public interface IMovieRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  CRUD operations
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle film fra repository.
        /// </summary>
        /// <returns>Read-only liste over alle film</returns>
        IReadOnlyList<Movie> GetAll();

        /// <summary>
        /// Finder en film baseret på ID.
        /// </summary>
        /// <param name="id">Film-ID at søge efter</param>
        /// <returns>Film hvis fundet, ellers null</returns>
        Movie? GetById(int id);

        /// <summary>
        /// Tilføjer en ny film til repository.
        /// </summary>
        /// <param name="movie">Filmen der skal tilføjes</param>
        void Add(Movie movie);

        /// <summary>
        /// Opdaterer en eksisterende film.
        /// </summary>
        /// <param name="movie">Film med opdateret data</param>
        void Update(Movie movie);

        /// <summary>
        /// Sletter en film baseret på ID.
        /// </summary>
        /// <param name="id">ID på filmen der skal slettes</param>
        void Delete(int id);

        // ─────────────────────────────────────────────────────────────────────────
        //  Business logic operations
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tjekker om der findes en aktiv film med samme titel.
        /// Bruges til validering - forhindrer duplikater af aktive film.
        /// </summary>
        /// <param name="title">Titel at søge efter</param>
        /// <returns>True hvis der findes en aktiv film med denne titel</returns>
        bool ExistsActiveTitle(string title);
    }
}
