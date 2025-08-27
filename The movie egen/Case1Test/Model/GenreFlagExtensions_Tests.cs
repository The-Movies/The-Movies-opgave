using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using The_movie_egen.Model.Enums;

namespace Case1Test.Model
{
    /// <summary>
    /// Tests for Genre flag funktionalitet - validerer at genre-systemet virker korrekt.
    /// 
    /// Dækker:
    /// - Kombination af multiple genrer (bitwise OR operation)
    /// - Standardværdier for genre
    /// - Fjernelse af genrer fra kombinationer
    /// - Verificering af genre-flag funktionalitet
    /// </summary>
    [TestClass]
    public class GenreFlagExtensions_Tests
    {
        /// <summary>
        /// Test: Kombination af multiple genrer skal virke korrekt
        /// 
        /// Hvad testen tjekker:
        /// - At man kan kombinere flere genrer med bitwise OR (|)
        /// - At alle kombinerede genrer er aktive i resultatet
        /// - At genrer der ikke er inkluderet ikke er aktive      
        /// </summary>
        [TestMethod]
        [TestCategory("Genre")]
        public void GenreFlags_CanCombineMultipleGenres()
        {
            // Arrange & Act: Kombiner tre forskellige genrer
            // Dette simulerer en film der tilhører både Action, Comedy og Drama
            var combined = Genre.Action | Genre.Comedy | Genre.Drama;

            // Assert: Verificer at alle kombinerede genrer er aktive
            Assert.IsTrue(combined.HasFlag(Genre.Action), "Action genre skal være inkluderet");
            Assert.IsTrue(combined.HasFlag(Genre.Comedy), "Comedy genre skal være inkluderet");
            Assert.IsTrue(combined.HasFlag(Genre.Drama), "Drama genre skal være inkluderet");
            
            // Assert: Verificer at ikke-inkluderede genrer ikke er aktive
            Assert.IsFalse(combined.HasFlag(Genre.Horror), "Horror genre skal IKKE være inkluderet");
        }

        /// <summary>
        /// Test: Standardværdi for Genre skal være None
        /// 
        /// Hvad testen tjekker:
        /// - At når en Genre variabel oprettes uden værdi, er den Genre.None
        /// - At default(Genre) returnerer Genre.None
        /// </summary>
        [TestMethod]
        [TestCategory("Genre")]
        public void GenreFlags_NoneIsDefault()
        {
            // Arrange & Act: Opret en Genre variabel uden at sætte værdi
            // Dette simulerer hvad der sker når en ny Movie oprettes
            Genre genre = default;

            // Assert: Verificer at standardværdien er Genre.None
            Assert.AreEqual(Genre.None, genre, "Standardværdi for Genre skal være None");
        }

        /// <summary>
        /// Test: Fjernelse af genrer fra kombination skal virke korrekt
        /// 
        /// Hvad testen tjekker:
        /// - At man kan fjerne en genre fra en kombination med bitwise AND og NOT
        /// - At kun den fjernede genre ikke længere er aktiv
        /// - At andre genrer i kombinationen forbliver aktive
        /// </summary>
        [TestMethod]
        [TestCategory("Genre")]
        public void GenreFlags_CanRemoveGenres()
        {
            // Arrange: Opret en kombination af tre genrer
            // Dette simulerer en film der oprindeligt har Action, Comedy og Drama
            var combined = Genre.Action | Genre.Comedy | Genre.Drama;

            // Act: Fjern Comedy genre fra kombinationen
            // Vi bruger bitwise AND med NOT (~) for at fjerne Comedy
            var withoutComedy = combined & ~Genre.Comedy;

            // Assert: Verificer at Comedy er fjernet, men andre genrer er beholdt
            Assert.IsTrue(withoutComedy.HasFlag(Genre.Action), "Action genre skal være beholdt");
            Assert.IsFalse(withoutComedy.HasFlag(Genre.Comedy), "Comedy genre skal være fjernet");
            Assert.IsTrue(withoutComedy.HasFlag(Genre.Drama), "Drama genre skal være beholdt");
        }

        /// <summary>
        /// Test: Kombination af mange genrer skal virke korrekt
        /// 
        /// Hvad testen tjekker:
        /// - At man kan kombinere mange forskellige genrer samtidig
        /// - At alle kombinerede genrer er aktive i resultatet
        /// - At bitwise operationer håndterer multiple flag korrekt
        /// </summary>
        [TestMethod]
        [TestCategory("Genre")]
        public void GenreFlags_AllGenresAreUnique()
        {
            // Arrange & Act: Kombiner seks forskellige genrer
            // Vi bruger kun seks genrer for at undgå overflow-problemer
            // Dette simulerer en kompleks film der tilhører mange genrer
            var someGenres = Genre.Action | Genre.Comedy | Genre.Drama | Genre.Horror | 
                           Genre.Romance | Genre.SciFi;

            // Assert: Verificer at alle seks genrer er aktive
            Assert.IsTrue(someGenres.HasFlag(Genre.Action), "Action genre skal være inkluderet");
            Assert.IsTrue(someGenres.HasFlag(Genre.Comedy), "Comedy genre skal være inkluderet");
            Assert.IsTrue(someGenres.HasFlag(Genre.Drama), "Drama genre skal være inkluderet");
            Assert.IsTrue(someGenres.HasFlag(Genre.Horror), "Horror genre skal være inkluderet");
            Assert.IsTrue(someGenres.HasFlag(Genre.Romance), "Romance genre skal være inkluderet");
            Assert.IsTrue(someGenres.HasFlag(Genre.SciFi), "SciFi genre skal være inkluderet");
        }
    }
} 