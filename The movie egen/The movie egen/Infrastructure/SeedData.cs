// System-navneområder (standard)
using System.Collections.Generic;         // List<T>
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)

// Domænemodeller
using The_movie_egen.Model.Cinemas;       // Cinema, Auditorium

namespace The_movie_egen.Infrastructure
{
    /// <summary>
    /// Seed-data til initialisering af applikationen.
    /// - Indeholder standard biografer og sale til test/demo
    /// - Bruges af JsonCinemaRepository når cinemas.json ikke findes
    /// - Returnerer ObservableCollection for nem binding til UI
    /// </summary>
    public static class SeedData
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returnerer standard seed-data med biografer og sale.
        /// Indeholder 4 biografer med forskellige antal sale og pladser.
        /// Bruges til at initialisere applikationen første gang den køres.
        /// </summary>
        /// <returns>ObservableCollection med seed-biografer og sale</returns>
        public static ObservableCollection<Cinema> Cinemas()
        {
            return new ObservableCollection<Cinema>
            {
                // Biograf 1: Hjerm
                new Cinema
                {
                    Id = 1, 
                    Name = "The Movies Hjerm", 
                    City = "Hjerm",
                    Auditoriums = new List<Auditorium>
                    {
                        new Auditorium { Id = 101, CinemaId = 1, Name = "Sal 1", Seats = 120 },
                        new Auditorium { Id = 102, CinemaId = 1, Name = "Sal 2", Seats = 80  },
                    }
                },
                
                // Biograf 2: Videbæk
                new Cinema
                {
                    Id = 2, 
                    Name = "The Movies Videbæk", 
                    City = "Videbæk",
                    Auditoriums = new List<Auditorium>
                    {
                        new Auditorium { Id = 201, CinemaId = 2, Name = "Sal A", Seats = 90  },
                        new Auditorium { Id = 202, CinemaId = 2, Name = "Sal B", Seats = 60  },
                    }
                },
                
                // Biograf 3: Thorsminde
                new Cinema
                {
                    Id = 3, 
                    Name = "The Movies Thorsminde", 
                    City = "Thorsminde",
                    Auditoriums = new List<Auditorium>
                    {
                        new Auditorium { Id = 301, CinemaId = 3, Name = "Sal 1", Seats = 70 },
                        new Auditorium { Id = 302, CinemaId = 3, Name = "Sal 2", Seats = 50 },
                    }
                },
                
                // Biograf 4: Ræhr
                new Cinema
                {
                    Id = 4, 
                    Name = "The Movies Ræhr", 
                    City = "Ræhr",
                    Auditoriums = new List<Auditorium>
                    {
                        new Auditorium { Id = 401, CinemaId = 4, Name = "Storsalen", Seats = 140 },
                        new Auditorium { Id = 402, CinemaId = 4, Name = "Studiosalen", Seats = 65  },
                    }
                }
            };
        }
    }
}
