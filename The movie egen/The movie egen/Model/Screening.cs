// System-navneområder (standard)
using System;                             // DateTime, DateTimeKind

// Domænemodeller
using The_movie_egen.Model.Cinemas;       // Cinema, Auditorium

namespace The_movie_egen.Model
{
    /// <summary>
    /// Domænemodel til en film-forestilling i biograf-systemet.
    /// - Holder forestillings-information (tidspunkt, sal, film)
    /// - Håndterer UTC-tidshåndtering for konsistent dato/klokkeslæt
    /// - Beregner automatisk sluttid baseret på film-varighed + tillæg
    /// - Inkluderer navigation properties til relaterede entiteter
    /// </summary>
    public sealed class Screening
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Public properties (data-binding og serialisering)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unikt ID for forestillingen (auto-genereret af repository).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID på biografen hvor forestillingen vises (foreign key).
        /// </summary>
        public int CinemaId { get; set; }

        /// <summary>
        /// ID på salen hvor forestillingen vises (foreign key).
        /// </summary>
        public int AuditoriumId { get; set; }

        /// <summary>
        /// ID på filmen der vises (foreign key).
        /// </summary>
        public int MovieId { get; set; }

        /// <summary>
        /// Starttidspunkt for forestillingen (altid i UTC).
        /// Setter normaliserer automatisk til UTC hvis nødvendigt.
        /// </summary>
        private DateTime _startUtc;
        public DateTime StartUtc
        {
            get => _startUtc;
            set => _startUtc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        }

        /// <summary>
        /// Reklametid i minutter før filmen starter (standard: 15 min).
        /// Kan ændres pr. forestilling.
        /// </summary>
        public int AdsMinutes { get; init; } = 15;

        /// <summary>
        /// Oprydningstid i minutter efter filmen slutter (standard: 15 min).
        /// Kan ændres pr. forestilling.
        /// </summary>
        public int CleaningMinutes { get; init; } = 15;

        // ─────────────────────────────────────────────────────────────────────────
        //  Beregnede properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Samlet varighed i minutter (film + reklamer + oprydning).
        /// Beregnet dynamisk baseret på film-varighed og tillæg.
        /// </summary>
        public int TotalMinutes
            => (Movie?.DurationMin ?? 0) + AdsMinutes + CleaningMinutes;

        /// <summary>
        /// Sluttidspunkt i UTC (beregnet ud fra start + total-varighed).
        /// Altid konsistent med StartUtc og TotalMinutes.
        /// </summary>
        public DateTime EndUtc => StartUtc.AddMinutes(TotalMinutes);

        /// <summary>
        /// Starttidspunkt i lokal tid (konverteret fra UTC).
        /// Praktisk til UI-visning og bruger-interaktion.
        /// </summary>
        public DateTime StartLocal => StartUtc.ToLocalTime();

        /// <summary>
        /// Sluttidspunkt i lokal tid (konverteret fra UTC).
        /// Praktisk til UI-visning og bruger-interaktion.
        /// </summary>
        public DateTime EndLocal => EndUtc.ToLocalTime();

        // ─────────────────────────────────────────────────────────────────────────
        //  Navigation properties (relaterede entiteter)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Biografen hvor forestillingen vises (navigation property).
        /// Kan være null hvis ikke indlæst.
        /// </summary>
        public Cinema? Cinema { get; set; }

        /// <summary>
        /// Salen hvor forestillingen vises (navigation property).
        /// Kan være null hvis ikke indlæst.
        /// </summary>
        public Auditorium? Auditorium { get; set; }

        /// <summary>
        /// Filmen der vises (navigation property).
        /// Kan være null hvis ikke indlæst.
        /// </summary>
        public Movie? Movie { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Static hjælpefunktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Hjælper til at beregne sluttid for en forestilling.
        /// Normaliserer automatisk input til UTC og beregner sluttid.
        /// </summary>
        /// <param name="startUtc">Starttidspunkt (konverteres til UTC hvis nødvendigt)</param>
        /// <param name="movieDurationMinutes">Film-varighed i minutter</param>
        /// <param name="adsMinutes">Reklametid i minutter (default: 15)</param>
        /// <param name="cleaningMinutes">Oprydningstid i minutter (default: 15)</param>
        /// <returns>Sluttidspunkt i UTC</returns>
        public static DateTime CalcEnd(DateTime startUtc, int movieDurationMinutes, int adsMinutes = 15, int cleaningMinutes = 15)
            => (startUtc.Kind == DateTimeKind.Utc ? startUtc : startUtc.ToUniversalTime())
               .AddMinutes(movieDurationMinutes + adsMinutes + cleaningMinutes);

        // ─────────────────────────────────────────────────────────────────────────
        //  Override metoder
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// String-repræsentation af forestillingen.
        /// Indeholder starttid, filmtitel, varighed og sal.
        /// </summary>
        /// <returns>Formateret streng med forestillings-information</returns>
        public override string ToString()
            => $"{StartLocal:dd-MM HH:mm}  {Movie?.Title ?? "(ukendt film)"}  " +
               $"({Movie?.DurationMin ?? 0} + {AdsMinutes}+{CleaningMinutes} = {TotalMinutes} min)  " +
               $"Sal: {Auditorium?.Name}";
    }
}
