using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using The_movie_egen.Model;
using The_movie_egen.Model.Enums;
using The_movie_egen.Services;
using The_movie_egen.Data;
using The_movie_egen.Data.Json;
using System.Linq;

namespace Case1Test.Services
{
    [TestClass]
    public class SchedulingService_JsonRepository_Tests
    {
        private JsonMovieRepository _movieRepo;
        private JsonScreeningRepository _screeningRepo;
        private SchedulingService _service;
        private Movie _testMovie;

        [TestInitialize]
        public void Setup()
        {
            // Brug unikke filnavne for hver test
            var testId = Guid.NewGuid().ToString("N")[..8];
            _movieRepo = new JsonMovieRepository($"test_movies_{testId}.json");
            _screeningRepo = new JsonScreeningRepository($"test_screenings_{testId}", _movieRepo);
            _service = new SchedulingService(_movieRepo, _screeningRepo);
            
            // Tilføj en test-film
            _testMovie = new Movie("Test Film", 120, Genre.Action);
            _movieRepo.Add(_testMovie);
        }

        [TestMethod]
        public void AddScreening_WithOverlappingTime_ShouldThrowException_JsonRepository()
        {
            // Arrange
            var cinemaId = 1;
            var auditoriumId = 1;
            var startTime = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            
            // Tilføj første forestilling
            _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, startTime);
            
            // Act & Assert
            // Prøv at tilføj en anden forestilling på samme tidspunkt
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, startTime));
            
            Assert.AreEqual("Overlap i tidsplanen for denne sal.", exception.Message);
        }

        [TestMethod]
        public void AddScreening_WithOverlappingEndTime_ShouldThrowException_JsonRepository()
        {
            // Arrange
            var cinemaId = 1;
            var auditoriumId = 1;
            var firstStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            var secondStart = new DateTime(2025, 1, 15, 20, 30, 0); // 20:30 (overlapper med første film der slutter 21:30)
            
            // Tilføj første forestilling (120 min + 15 + 15 = 150 min total, slutter 21:30)
            var firstScreening = _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, firstStart);
            Console.WriteLine($"Added first screening: {firstScreening.Id} at {firstScreening.StartLocal:HH:mm} - {firstScreening.EndLocal:HH:mm}");
            
            // Tjek om første forestilling blev gemt
            var existingBeforeSecond = _service.GetMonth(cinemaId, 2025, 1).ToList();
            Console.WriteLine($"Existing screenings before second: {existingBeforeSecond.Count}");
            foreach (var s in existingBeforeSecond)
            {
                Console.WriteLine($"  Screening {s.Id}: {s.StartLocal:HH:mm} - {s.EndLocal:HH:mm} (Auditorium: {s.AuditoriumId})");
            }
            
            // Act & Assert
            // Prøv at tilføj en anden forestilling der starter før første film slutter
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, secondStart));
            
            Assert.AreEqual("Overlap i tidsplanen for denne sal.", exception.Message);
        }

        [TestMethod]
        public void AddScreening_WithNoOverlap_ShouldSucceed_JsonRepository()
        {
            // Arrange
            var cinemaId = 1;
            var auditoriumId = 1;
            var firstStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            var secondStart = new DateTime(2025, 1, 15, 21, 45, 0); // 21:45 (efter første film slutter 21:30)
            
            // Act
            var firstScreening = _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, firstStart);
            var secondScreening = _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, secondStart);
            
            // Assert
            Assert.IsNotNull(firstScreening);
            Assert.IsNotNull(secondScreening);
            Assert.AreNotEqual(firstScreening.Id, secondScreening.Id);
            
            // Verificer at begge forestillinger er gemt
            var screenings = _service.GetMonth(cinemaId, 2025, 1).ToList();
            Assert.AreEqual(2, screenings.Count);
        }
    }
}
