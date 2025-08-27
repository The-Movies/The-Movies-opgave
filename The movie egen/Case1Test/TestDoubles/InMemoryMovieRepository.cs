using System;
using System.Collections.Generic;
using System.Linq;
using The_movie_egen.Model;
using The_movie_egen.Model.Enums;
using The_movie_egen.Model.Repositories;

namespace Case1Test.TestDoubles
{
    /// <summary>
    /// In-memory test-double for IMovieRepository.
    /// 
    /// Hvad denne klasse gør:
    /// - Simulerer en database uden at bruge filer eller eksterne systemer
    /// - Holder film i hukommelsen under test-kørslen
    /// - Implementerer alle metoder fra IMovieRepository interface
    /// 
    /// Hvorfor vi bruger denne:
    /// - Tests kører hurtigt (ingen fil-IO)
    /// - Tests er isolerede (ingen delt data mellem tests)
    /// - Tests er forudsigelige (ingen eksterne afhængigheder)
    /// </summary>
    internal sealed class InMemoryMovieRepository : IMovieRepository
    {
        // Privat liste til at holde film i hukommelsen
        private readonly List<Movie> _items = new();
        
        // Tæller til at generere unikke ID'er (starter på 1)
        private int _nextId = 1;

        /// <summary>
        /// Henter alle film sorteret efter titel.
        /// </summary>
        public IReadOnlyList<Movie> GetAll() => _items.OrderBy(x => x.Title).ToList();

        /// <summary>
        /// Finder en film ved ID.
        /// Returnerer null hvis filmen ikke findes.
        /// </summary>
        public Movie? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// Tilføjer en ny film til repository.
        /// Tildeler automatisk et unikt ID (simulerer database auto-increment).
        /// </summary>
        public void Add(Movie movie)
        {
            // Simuler database auto-increment - tildel næste ledige ID
            movie.Id = _nextId++;
            _items.Add(movie);
        }

        /// <summary>
        /// Opdaterer en eksisterende film.
        /// </summary>
        public void Update(Movie movie)
        {
            var i = _items.FindIndex(x => x.Id == movie.Id);
            if (i < 0) throw new KeyNotFoundException($"Movie {movie.Id} not found");
            _items[i] = movie;
        }

        /// <summary>
        /// Sletter en film ved ID.
        /// Gør ingenting hvis filmen ikke findes (idempotent).
        /// </summary>
        public void Delete(int id)
        {
            var idx = _items.FindIndex(x => x.Id == id);
            if (idx >= 0) _items.RemoveAt(idx);
        }

        /// <summary>
        /// Tjekker om der findes en aktiv film med den givne titel.
        /// Søgningen er case-insensitive (stor/lille bogstaver betyder ikke noget).
        /// </summary>
        public bool ExistsActiveTitle(string title) =>
            _items.Any(x => x.Status == FilmStatus.Active &&
                            string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Hjælpemetode til at forfylde repository i test Arrange-trin.
        /// 
        /// Eksempel brug:
        /// repo.Seed(new Movie("Die Hard", 131, Genre.Action));
        /// Dette er nyttigt når vi vil teste scenarier med eksisterende film.
        /// </summary>
        public void Seed(params Movie[] movies)
        {
            foreach (var m in movies) Add(m);
        }
    }
} 