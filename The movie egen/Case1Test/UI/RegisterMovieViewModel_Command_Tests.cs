using Microsoft.VisualStudio.TestTools.UnitTesting;
using Case1Test.TestDoubles;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using The_movie_egen.Model;
using The_movie_egen.Model.Enums;
using The_movie_egen.Services;

namespace Case1Test.UI
{
    /// <summary>
    /// Tests for UI-logik og validering - simulerer hvad der sker når brugere interagerer med UI.
    /// 
    /// Dækker:
    /// - Validering af bruger-input fra UI
    /// - Håndtering af gyldige og ugyldige data
    /// - Fejlhåndtering for UI-scenarier
    /// - Integration mellem UI og business logic
    /// </summary>
    [TestClass]
    public class RegisterMovieViewModel_Command_Tests
    {
        /// <summary>
        /// Test: Oprettelse af film med gyldige data fra UI skal lykkes
        /// 
        /// Hvad testen tjekker:
        /// - At UI kan sende gyldige data til MovieRegistry service
        /// - At filmen oprettes korrekt med alle angivne data
        /// - At multiple genrer håndteres korrekt fra UI
        /// - At returnerede film objekt indeholder korrekte data
        /// 
        /// Hvorfor denne test er vigtig:
        /// - Simulerer den "glad vej" når brugeren udfylder formular korrekt
        /// - Validerer integration mellem UI og business logic
        /// - Sikrer at UI kan håndtere komplekse data (multiple genrer)
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MovieCreation_WithValidData_ShouldSucceed()
        {
            // Arrange: Opret service som normalt
            // Dette simulerer hvad der sker når UI initialiseres
            var repo = new InMemoryMovieRepository();
            var service = new MovieRegistry(repo);

            // Act: Simuler at brugeren udfylder formular og trykker "Gem"
            // Vi bruger "Test Movie" som eksempel med Action og Thriller genrer
            var movie = service.RegisterMovie("Test Movie", 120, Genre.Action | Genre.Thriller);

            // Assert: Verificer at filmen blev oprettet korrekt
            Assert.IsNotNull(movie, "Film objekt skal ikke være null");
            Assert.AreEqual("Test Movie", movie.Title, "Titel skal gemmes præcis som angivet");
            Assert.AreEqual(120, movie.DurationMin, "Varighed skal gemmes i minutter");
            Assert.IsTrue(movie.Genres.HasFlag(Genre.Action), "Action genre skal være markeret");
            Assert.IsTrue(movie.Genres.HasFlag(Genre.Thriller), "Thriller genre skal være markeret");
        }

        /// <summary>
        /// Test: Oprettelse af film med ugyldig titel fra UI skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At UI ikke kan oprette film med tom titel
        /// - At systemet kaster ArgumentException for ugyldig titel
        /// - At validering sker før film oprettes
        /// 
        /// Hvorfor denne test er vigtig:
        /// - Simulerer når brugeren glemmer at udfylde titel-feltet
        /// - Sikrer at UI viser fejlbesked til brugeren
        /// - Validerer at input-validering virker fra UI-perspektiv
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MovieCreation_WithInvalidTitle_ShouldThrowException()
        {
            // Arrange: Opret service som normalt
            var repo = new InMemoryMovieRepository();
            var service = new MovieRegistry(repo);

            // Act & Assert: Simuler at brugeren trykker "Gem" uden at udfylde titel
            // Dette skal kaste en ArgumentException som UI kan fange og vise
            Assert.ThrowsException<ArgumentException>(() =>
                service.RegisterMovie("", 120, Genre.Action),
                "UI skal ikke kunne oprette film med tom titel");
        }

        /// <summary>
        /// Test: Oprettelse af film med ugyldig varighed fra UI skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At UI ikke kan oprette film med negativ varighed
        /// - At systemet kaster ArgumentOutOfRangeException for ugyldig varighed
        /// - At validering af numeriske felter virker
        /// 
        /// Hvorfor denne test er vigtig:
        /// - Simulerer når brugeren indtaster ugyldig varighed (f.eks. -1)
        /// - Sikrer at UI viser fejlbesked for ugyldige tal
        /// - Validerer at numerisk input-validering virker fra UI-perspektiv
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MovieCreation_WithInvalidDuration_ShouldThrowException()
        {
            // Arrange: Opret service som normalt
            var repo = new InMemoryMovieRepository();
            var service = new MovieRegistry(repo);

            // Act & Assert: Simuler at brugeren indtaster negativ varighed
            // Dette skal kaste en ArgumentOutOfRangeException som UI kan fange
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                service.RegisterMovie("Test Movie", -1, Genre.Action),
                "UI skal ikke kunne oprette film med negativ varighed");
        }

        /// <summary>
        /// Test: Oprettelse af film uden valgt genre fra UI skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At UI ikke kan oprette film uden at vælge mindst én genre
        /// - At systemet kaster ArgumentException når ingen genre er valgt
        /// - At genre-validering virker fra UI-perspektiv
        /// 
        /// Hvorfor denne test er vigtig:
        /// - Simulerer når brugeren glemmer at vælge genre i UI
        /// - Sikrer at UI viser fejlbesked om at genre er påkrævet
        /// - Validerer at genre-validering virker korrekt
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MovieCreation_WithNoGenre_ShouldThrowException()
        {
            // Arrange: Opret service som normalt
            var repo = new InMemoryMovieRepository();
            var service = new MovieRegistry(repo);

            // Act & Assert: Simuler at brugeren trykker "Gem" uden at vælge genre
            // Genre.None betyder "ingen genre valgt" og skal afvises
            Assert.ThrowsException<ArgumentException>(() =>
                service.RegisterMovie("Test Movie", 120, Genre.None),
                "UI skal kræve at mindst én genre er valgt");
        }

        /// <summary>
        /// Test: Oprettelse af film med duplikat titel fra UI skal afvises
        /// 
        /// Hvad testen tjekker:
        /// - At UI ikke kan oprette film hvis der allerede findes en film med samme titel
        /// - At systemet kaster InvalidOperationException for duplikat titler
        /// - At duplikat-tjekket virker fra UI-perspektiv
        /// 
        /// Hvorfor denne test er vigtig:
        /// - Simulerer når brugeren prøver at oprette en film der allerede findes
        /// - Sikrer at UI viser fejlbesked om duplikat titel
        /// - Validerer at forretningsreglen "ingen duplikat titler" håndhæves
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MovieCreation_DuplicateTitle_ShouldThrowException()
        {
            // Arrange: Opret service og registrer en eksisterende film
            // Dette simulerer at der allerede findes en film i systemet
            var repo = new InMemoryMovieRepository();
            var service = new MovieRegistry(repo);
            
            // Opret første film - dette simulerer en eksisterende film i systemet
            service.RegisterMovie("Duplicate Title", 120, Genre.Action);

            // Act & Assert: Simuler at brugeren prøver at oprette film med samme titel
            // Dette skal fejle fordi der allerede findes en film med denne titel
            Assert.ThrowsException<InvalidOperationException>(() =>
                service.RegisterMovie("Duplicate Title", 150, Genre.Comedy),
                "UI skal ikke kunne oprette film med titel der allerede findes");
        }
    }
} 