// System-navneområder (standard)
using System.Collections.Generic;         // IEnumerable<T>

// Domænemodeller
using The_movie_egen.Model.Cinemas;       // Cinema, Auditorium

namespace The_movie_egen.Model.Repositories
{
    /// <summary>
    /// Repository interface til persistering af biograf-data.
    /// - Definerer CRUD-operationer for biografer og sale
    /// - Implementeres af JsonCinemaRepository
    /// - Bruges til dependency injection og testability
    /// - Separerer data-access fra business logic
    /// </summary>
    public interface ICinemaRepository
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Cinema operations (CRUD)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Henter alle biografer fra repository.
        /// </summary>
        IEnumerable<Cinema> GetAll();

        /// <summary>
        /// Finder en biograf baseret på ID.
        /// </summary>
        /// <param name="id">Biograf-ID at søge efter</param>
        /// <returns>Biograf hvis fundet, ellers null</returns>
        Cinema? GetById(int id);

        /// <summary>
        /// Gemmer alle biografer til repository.
        /// Overskriver eksisterende data.
        /// </summary>
        /// <param name="cinemas">Samling af biografer at gemme</param>
        void SaveAll(IEnumerable<Cinema> cinemas);

        // ─────────────────────────────────────────────────────────────────────────
        //  Auditorium operations (CRUD)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tilføjer en ny sal til en biograf.
        /// </summary>
        /// <param name="cinemaId">ID på biografen</param>
        /// <param name="name">Sal-navn</param>
        /// <param name="seats">Antal pladser</param>
        /// <returns>Den oprettede sal med tildelt ID</returns>
        /// <exception cref="ArgumentException">Hvis biograf ikke findes</exception>
        Auditorium AddAuditorium(int cinemaId, string name, int seats);

        /// <summary>
        /// Opdaterer en eksisterende sal.
        /// </summary>
        /// <param name="auditoriumId">ID på salen</param>
        /// <param name="name">Nyt sal-navn</param>
        /// <param name="seats">Nyt antal pladser</param>
        /// <exception cref="ArgumentException">Hvis sal ikke findes</exception>
        void UpdateAuditorium(int auditoriumId, string name, int seats);

        /// <summary>
        /// Sletter en sal baseret på ID.
        /// </summary>
        /// <param name="auditoriumId">ID på salen der skal slettes</param>
        /// <returns>True hvis salen blev slettet, false hvis den ikke fandtes</returns>
        bool DeleteAuditorium(int auditoriumId);

        /// <summary>
        /// Finder en sal baseret på ID.
        /// </summary>
        /// <param name="auditoriumId">Sal-ID at søge efter</param>
        /// <returns>Sal hvis fundet, ellers null</returns>
        Auditorium? GetAuditoriumById(int auditoriumId);
    }
}
