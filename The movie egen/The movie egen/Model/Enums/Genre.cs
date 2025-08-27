// System-navneområder (standard)
using System;                             // Flags attribute

namespace The_movie_egen.Model.Enums
{
    /// <summary>
    /// Flags-enum til film-genrer.
    /// - Understøtter multiple genrer per film (Action | Comedy | Drama)
    /// - Bruges til kategorisering og søgning af film
    /// - Serialiseres som tekst i JSON (fx "Action, Comedy")
    /// - Valideres i service-lag (ingen film må have Genre.None)
    /// </summary>
    [Flags]
    public enum Genre
    {

        None = 0,               // Ingen genre tildelt (bruges til validering)
        Action = 1 << 0,       
        Comedy = 1 << 1,       
        Drama = 1 << 2,
        Horror = 1 << 3,
        Romance = 1 << 4,
        SciFi = 1 << 5,
        Thriller = 1 << 6,
        Documentary = 1 << 7,
        Crime = 1 << 8,
        Animation = 1 << 9,
        Adventure = 1 << 10,
        Fantasy = 1 << 11
    }
}
