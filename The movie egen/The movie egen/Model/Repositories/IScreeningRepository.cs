// System-navneområder (standard)
using System.Collections.Generic;         // IEnumerable<T>

// Domænemodeller
using The_movie_egen.Model;               // Screening

namespace The_movie_egen.Model.Repositories
{
    /// <summary>
    /// Repository interface til persistering af forestillings-data.
    /// - Definerer operationer for forestillings-håndtering
    /// - Implementeres af JsonScreeningRepository
    /// - Organiserer data pr. biograf og måned
    /// - Bruges til dependency injection og testability
    /// </summary>
    public interface IScreeningRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  CRUD operations
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle forestillinger for en specifik biograf og måned.
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="year">År (fx 2025)</param>
        /// <param name="month">Måned (1-12)</param>
        /// <returns>Samling af forestillinger for den måned</returns>
        IEnumerable<Screening> GetForCinemaMonth(int cinemaId, int year, int month);

        /// <summary>
        /// Tilføjer en ny forestilling til repository.
        /// </summary>
        /// <param name="s">Forestillingen der skal tilføjes</param>
        void Add(Screening s);

        /// <summary>
        /// Sletter en forestilling baseret på ID.
        /// </summary>
        /// <param name="screeningId">ID på forestillingen der skal slettes</param>
        void Delete(int screeningId);
    }
}
