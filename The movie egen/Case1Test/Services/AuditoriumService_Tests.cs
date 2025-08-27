using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using The_movie_egen.Model.Cinemas;
using The_movie_egen.Model.Repositories;
using The_movie_egen.Services;

namespace Case1Test.Services;

[TestClass]
public class AuditoriumService_Tests
{
    private AuditoriumService _service = null!;
    private InMemoryCinemaRepository _cinemaRepo = null!;
    private InMemoryScreeningRepository _screeningRepo = null!;

    [TestInitialize]
    public void Setup()
    {
        _cinemaRepo = new InMemoryCinemaRepository();
        _screeningRepo = new InMemoryScreeningRepository();
        _service = new AuditoriumService(_cinemaRepo, _screeningRepo);

        // Setup test data
        var cinema = new Cinema { Id = 1, Name = "Test Cinema", City = "Test City" };
        _cinemaRepo.AddCinema(cinema);
    }

    [TestMethod]
    public void AddAuditorium_ValidData_ShouldAddAuditorium()
    {
        // Act
        var auditorium = _service.AddAuditorium(1, "Test Sal", 100);

        // Assert
        Assert.IsNotNull(auditorium);
        Assert.AreEqual("Test Sal", auditorium.Name);
        Assert.AreEqual(100, auditorium.Seats);
        Assert.AreEqual(1, auditorium.CinemaId);
    }

    [TestMethod]
    public void AddAuditorium_EmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => 
            _service.AddAuditorium(1, "", 100));
    }

    [TestMethod]
    public void AddAuditorium_ZeroSeats_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => 
            _service.AddAuditorium(1, "Test Sal", 0));
    }

    [TestMethod]
    public void UpdateAuditorium_ValidData_ShouldUpdateAuditorium()
    {
        // Arrange
        var auditorium = _service.AddAuditorium(1, "Test Sal", 100);

        // Act
        _service.UpdateAuditorium(auditorium.Id, "Updated Sal", 150);

        // Assert
        var updated = _cinemaRepo.GetAuditoriumById(auditorium.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Sal", updated.Name);
        Assert.AreEqual(150, updated.Seats);
    }

    [TestMethod]
    public void DeleteAuditorium_NoFutureScreenings_ShouldDeleteAuditorium()
    {
        // Arrange
        var auditorium = _service.AddAuditorium(1, "Test Sal", 100);

        // Act
        var result = _service.DeleteAuditorium(auditorium.Id);

        // Assert
        Assert.IsTrue(result);
        var deleted = _cinemaRepo.GetAuditoriumById(auditorium.Id);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public void CanDeleteAuditorium_NoFutureScreenings_ShouldReturnTrue()
    {
        // Arrange
        var auditorium = _service.AddAuditorium(1, "Test Sal", 100);

        // Act
        var canDelete = _service.CanDeleteAuditorium(auditorium.Id);

        // Assert
        Assert.IsTrue(canDelete);
    }
}

// Test double for ICinemaRepository
public class InMemoryCinemaRepository : ICinemaRepository
{
    private readonly List<Cinema> _cinemas = new();

    public void AddCinema(Cinema cinema)
    {
        _cinemas.Add(cinema);
    }

    public IEnumerable<Cinema> GetAll() => _cinemas;

    public Cinema? GetById(int id) => _cinemas.FirstOrDefault(c => c.Id == id);

    public void SaveAll(IEnumerable<Cinema> cinemas)
    {
        _cinemas.Clear();
        _cinemas.AddRange(cinemas);
    }

    public Auditorium AddAuditorium(int cinemaId, string name, int seats)
    {
        var cinema = GetById(cinemaId);
        if (cinema == null)
            throw new ArgumentException($"Cinema with ID {cinemaId} not found");

        // Find next available auditorium ID - handle empty list
        var existingAuditoriums = _cinemas.SelectMany(c => c.Auditoriums);
        var nextId = existingAuditoriums.Any() ? existingAuditoriums.Max(a => a.Id) + 1 : 1;
        
        var auditorium = new Auditorium
        {
            Id = nextId,
            CinemaId = cinemaId,
            Name = name,
            Seats = seats
        };

        cinema.Auditoriums.Add(auditorium);
        return auditorium;
    }

    public void UpdateAuditorium(int auditoriumId, string name, int seats)
    {
        var auditorium = _cinemas.SelectMany(c => c.Auditoriums).FirstOrDefault(a => a.Id == auditoriumId);
        if (auditorium == null)
            throw new ArgumentException($"Auditorium with ID {auditoriumId} not found");

        auditorium.Name = name;
        auditorium.Seats = seats;
    }

    public bool DeleteAuditorium(int auditoriumId)
    {
        var cinema = _cinemas.FirstOrDefault(c => c.Auditoriums.Any(a => a.Id == auditoriumId));
        if (cinema == null) return false;

        var auditorium = cinema.Auditoriums.FirstOrDefault(a => a.Id == auditoriumId);
        if (auditorium == null) return false;

        return cinema.Auditoriums.Remove(auditorium);
    }

    public Auditorium? GetAuditoriumById(int auditoriumId)
    {
        return _cinemas.SelectMany(c => c.Auditoriums).FirstOrDefault(a => a.Id == auditoriumId);
    }
}
