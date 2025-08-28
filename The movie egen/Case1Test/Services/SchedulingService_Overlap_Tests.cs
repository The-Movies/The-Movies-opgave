using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using The_movie_egen.Model;
using The_movie_egen.Model.Enums;
using The_movie_egen.Services;
using The_movie_egen.Model.Repositories;
using Case1Test.TestDoubles;

namespace Case1Test.Services
{
    [TestClass]
    public class SchedulingService_Overlap_Tests
    {
        private InMemoryMovieRepository _movieRepo;
        private InMemoryScreeningRepository _screeningRepo;
        private SchedulingService _service;
        private Movie _testMovie;

        [TestInitialize]
        public void Setup()
        {
            _movieRepo = new InMemoryMovieRepository();
            _screeningRepo = new InMemoryScreeningRepository();
            _service = new SchedulingService(_movieRepo, _screeningRepo);
            
            // Tilføj en test-film
            _testMovie = new Movie("Test Film", 120, Genre.Action);
            _movieRepo.Add(_testMovie);
        }

        [TestMethod]
        public void AddScreening_WithOverlappingTime_ShouldThrowException()
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
        public void AddScreening_WithOverlappingEndTime_ShouldThrowException()
        {
            // Arrange
            var cinemaId = 1;
            var auditoriumId = 1;
            var firstStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            var secondStart = new DateTime(2025, 1, 15, 20, 30, 0); // 20:30 (overlapper med første film der slutter 21:30)
            
            // Tilføj første forestilling (120 min + 15 + 15 = 150 min total, slutter 21:30)
            _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, firstStart);
            
            // Act & Assert
            // Prøv at tilføj en anden forestilling der starter før første film slutter
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, secondStart));
            
            Assert.AreEqual("Overlap i tidsplanen for denne sal.", exception.Message);
        }

        [TestMethod]
        public void AddScreening_WithOverlappingStartTime_ShouldThrowException()
        {
            // Arrange
            var cinemaId = 1;
            var auditoriumId = 1;
            var firstStart = new DateTime(2025, 1, 15, 20, 0, 0); // 20:00
            var secondStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00 (slutter 21:30, overlapper med første film der starter 20:00)
            
            // Tilføj første forestilling (starter 20:00)
            _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, firstStart);
            
            // Act & Assert
            // Prøv at tilføj en anden forestilling der slutter efter første film starter
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                _service.AddScreening(cinemaId, auditoriumId, _testMovie.Id, secondStart));
            
            Assert.AreEqual("Overlap i tidsplanen for denne sal.", exception.Message);
        }

        [TestMethod]
        public void AddScreening_WithNoOverlap_ShouldSucceed()
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
        }

        [TestMethod]
        public void AddScreening_DifferentAuditoriums_ShouldNotOverlap()
        {
            // Arrange
            var cinemaId = 1;
            var auditorium1 = 1;
            var auditorium2 = 2;
            var sameStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            
            // Act
            var firstScreening = _service.AddScreening(cinemaId, auditorium1, _testMovie.Id, sameStart);
            var secondScreening = _service.AddScreening(cinemaId, auditorium2, _testMovie.Id, sameStart);
            
            // Assert
            Assert.IsNotNull(firstScreening);
            Assert.IsNotNull(secondScreening);
            Assert.AreNotEqual(firstScreening.Id, secondScreening.Id);
        }

        [TestMethod]
        public void AddScreening_DifferentCinemas_ShouldNotOverlap()
        {
            // Arrange
            var cinema1 = 1;
            var cinema2 = 2;
            var auditoriumId = 1;
            var sameStart = new DateTime(2025, 1, 15, 19, 0, 0); // 19:00
            
            // Act
            var firstScreening = _service.AddScreening(cinema1, auditoriumId, _testMovie.Id, sameStart);
            var secondScreening = _service.AddScreening(cinema2, auditoriumId, _testMovie.Id, sameStart);
            
            // Assert
            Assert.IsNotNull(firstScreening);
            Assert.IsNotNull(secondScreening);
            Assert.AreNotEqual(firstScreening.Id, secondScreening.Id);
        }
    }
}
