using Microsoft.VisualStudio.TestTools.UnitTesting;
using Case1Test.TestDoubles;
using System;
using The_movie_egen.Model;
using The_movie_egen.Model.Enums;
using The_movie_egen.Services;

namespace Case1Test.Services
{
    /// <summary>
    /// Tests for MovieRegistry.RegisterMovie() – den forretningskritiske registrering.
    ///
    /// Dækker:
    ///  - Success: film oprettes, har Active-status og får Id
    ///  - Valideringsfejl: tom titel, varighed udenfor interval, ingen genre valgt
    ///  - Regelfejl: duplikat aktiv titel må ikke tillades
    /// </summary>
    [TestClass]
    public class MovieRegistry_RegisterMovie_Tests
    {
        /// <summary>
        /// Test: Registrering af en gyldig film skal lykkes
        /// 
        /// Hvad testen tjekker:
        /// - At filmen får et unikt ID (større end 0)
        /// - At alle filmdata gemmes korrekt (titel, varighed, genrer)
        /// - At filmen får status "Active" som standard
        /// - At filmen faktisk bliver gemt i repository (persistens)
        /// </summary>
        [TestMethod]
        [TestCategory("Register")]
        public void RegisterMovie_WhenValid_AddsActiveMovie_WithId()
        {
            // Arrange: Opret tomt in-memory repository og MovieRegistry service
            // Dette simulerer en ren database uden eksisterende film
            var repo = new InMemoryMovieRepository();
            var svc = new MovieRegistry(repo);

            // Act: Registrér en gyldig film med alle nødvendige data
            // Vi bruger "Interstellar" som eksempel - en kendt film med multiple genrer
            var m = svc.RegisterMovie("Interstellar", 169, Genre.SciFi | Genre.Drama);

            // Assert: Verificer at registreringen lykkedes fuldstændigt
            Assert.IsTrue(m.Id > 0, "Forventer at repository tildeler et unikt ID > 0");
            Assert.AreEqual("Interstellar", m.Title, "Titlen skal gemmes præcis som angivet");
            Assert.AreEqual(169, m.DurationMin, "Varigheden skal gemmes i minutter");
            Assert.IsTrue(m.Genres.HasFlag(Genre.SciFi), "SciFi genre skal være markeret");
            Assert.IsTrue(m.Genres.HasFlag(Genre.Drama), "Drama genre skal være markeret");
            Assert.AreEqual(FilmStatus.Active, m.Status, "Nye film skal have Active status");
            Assert.AreEqual(1, repo.GetAll().Count, "Repository skal indeholde præcis 1 film efter registrering");
        }

        /// <summary>
        /// Test: Registrering med tom eller blank titel skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At systemet afviser film med tom titel ("")
        /// - At systemet afviser film med kun mellemrum ("   ")
        /// - At der kastes en ArgumentException med forklarende besked
        /// </summary>
        [TestMethod]
        [TestCategory("Register")]
        public void RegisterMovie_WhenTitleIsBlank_ThrowsArgumentException()
        {
            // Arrange: Opret service
            var repo = new InMemoryMovieRepository();
            var svc = new MovieRegistry(repo);

            // Act + Assert: Forsøg at registrere film med blank titel
            // Dette skal kaste en ArgumentException fordi titel er påkrævet
            Assert.ThrowsException<ArgumentException>(() =>
                svc.RegisterMovie("   ", 120, Genre.Action),
                "Systemet skal afvise film med blank titel og kaste ArgumentException");
        }

        /// <summary>
        /// Test: Registrering med ugyldig varighed skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At systemet afviser film med varighed 0 eller negativ
        /// - At systemet afviser film med urealistisk lang varighed (over 600 min)
        /// - At der kastes ArgumentOutOfRangeException for ugyldige varigheder
        /// </summary>
        [TestMethod]
        [TestCategory("Register")]
        public void RegisterMovie_WhenDurationOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange: Opret service
            var repo = new InMemoryMovieRepository();
            var svc = new MovieRegistry(repo);

            // Act + Assert: Test nedre grænse - varighed 0 er ugyldig
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                svc.RegisterMovie("Ok", 0, Genre.Action),
                "Varighed 0 er udenfor gyldigt interval (1-600 minutter)");

            // Act + Assert: Test øvre grænse - varighed 1000 er urealistisk
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                svc.RegisterMovie("Ok", 1000, Genre.Action),
                "Varighed 1000 minutter er urealistisk og skal afvises");
        }

        /// <summary>
        /// Test: Registrering uden valgt genre skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At systemet afviser film hvor ingen genre er valgt (Genre.None)
        /// - At der kastes ArgumentException når genre mangler
        /// </summary>
        [TestMethod]
        [TestCategory("Register")]
        public void RegisterMovie_WhenNoGenreSelected_ThrowsArgumentException()
        {
            // Arrange: Opret service som normalt
            var repo = new InMemoryMovieRepository();
            var svc = new MovieRegistry(repo);

            // Act + Assert: Forsøg at registrere film uden genre
            // Genre.None betyder "ingen genre valgt" og skal afvises
            Assert.ThrowsException<ArgumentException>(() =>
                svc.RegisterMovie("Ok", 120, Genre.None),
                "Systemet skal kræve at mindst én genre er valgt");
        }

        /// <summary>
        /// Test: Registrering af film med duplikat titel skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At systemet afviser nye film hvis der allerede findes en aktiv film med samme titel
        /// - At duplikat-tjekket er case-insensitive (stor/lille bogstaver)
        /// - At der kastes InvalidOperationException for duplikat titler
        /// </summary>
        [TestMethod]
        [TestCategory("Register")]
        public void RegisterMovie_WhenDuplicateActiveTitle_ThrowsInvalidOperationException()
        {
            // Arrange: Opret repository med en eksisterende aktiv film
            var repo = new InMemoryMovieRepository();
            repo.Seed(new Movie("Die Hard", 131, Genre.Action));
            var svc = new MovieRegistry(repo);

            // Act + Assert: Forsøg at registrere endnu en film med titlen "Die Hard"
            Assert.ThrowsException<InvalidOperationException>(() =>
                svc.RegisterMovie("Die Hard", 100, Genre.Action),
                "Systemet skal forhindre duplikat aktive film med samme titel");
        }
    }
} 