// System-navneområder (standard)
using System;                             // DateTime, Math, etc.
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows.Media;               // Color, SolidColorBrush

// Domænemodeller
using The_movie_egen.Model;               // Movie
using The_movie_egen.Model.Cinemas;       // Auditorium

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til en forestilling i tidslinje-visningen.
    /// - Holder data om en specifik forestilling (film, sal, tidspunkt)
    /// - Beregner layout-positioner og dimensioner til Canvas-visning
    /// - Tilbyder farve-tildeling baseret på film-ID
    /// - Udstiller tooltip-tekst til visning i UI
    /// </summary>
    public class TimelineScreeningVM : INotifyPropertyChanged
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Konstanter og statiske felter
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Skala for tidslinje: 40 pixels per time.
        /// Bruges til at beregne position og bredde af forestillings-blokke.
        /// </summary>
        public const double PixelsPerHour = 40.0;

        /// <summary>
        /// Total bredde af tidslinje: 24 timer * 40 pixels/time = 960 pixels.
        /// </summary>
        public const double TotalWidth = 24 * PixelsPerHour;

        /// <summary>
        /// Farver til forskellige film (cykler gennem disse baseret på film-ID).
        /// Hver film får en konsistent farve baseret på dens ID.
        /// </summary>
        private static readonly Color[] _movieColors = new[]
        {
            Color.FromRgb(52, 152, 219),  // Blue
            Color.FromRgb(231, 76, 60),   // Red
            Color.FromRgb(46, 204, 113),  // Green
            Color.FromRgb(155, 89, 182),  // Purple
            Color.FromRgb(241, 196, 15),  // Yellow
            Color.FromRgb(230, 126, 34),  // Orange
            Color.FromRgb(26, 188, 156),  // Teal
            Color.FromRgb(142, 68, 173),  // Dark Purple
        };

        /// <summary>
        /// Ctor: initialiserer forestilling med film, sal og starttidspunkt.
        /// Tildeler automatisk en farve baseret på film-ID.
        /// </summary>
        public TimelineScreeningVM(DateTime start, Movie movie, Auditorium auditorium)
        {
            Start = start;
            Movie = movie;
            Auditorium = auditorium;

            // Tildel farve baseret på film-ID (konsistent for samme film)
            var colorIndex = Math.Abs(movie.Id) % _movieColors.Length;
            MovieColor = new SolidColorBrush(_movieColors[colorIndex]);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings) - Read-only properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unikt ID for forestillingen (fra database).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID på biografen som forestillingen tilhører.
        /// </summary>
        public int CinemaId { get; set; }

        /// <summary>
        /// Starttidspunkt for forestillingen (read-only).
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Filmen der vises (read-only).
        /// </summary>
        public Movie Movie { get; }

        /// <summary>
        /// Salen hvor forestillingen vises (read-only).
        /// </summary>
        public Auditorium Auditorium { get; }

        /// <summary>
        /// Navn på biografen (read-only, beregnet fra CinemaId).
        /// </summary>
        public string CinemaName { get; set; } = string.Empty;

        /// <summary>
        /// Reklametid i minutter før filmen starter.
        /// </summary>
        public int AdsMinutes { get; set; }

        /// <summary>
        /// Oprydningstid i minutter efter filmen slutter.
        /// </summary>
        public int CleaningMinutes { get; set; }

        /// <summary>
        /// Filmtitel (read-only, beregnet fra Movie).
        /// </summary>
        public string MovieTitle => Movie.Title;

        /// <summary>
        /// Sal-navn (read-only, beregnet fra Auditorium).
        /// </summary>
        public string AuditoriumName => Auditorium.Name;

        /// <summary>
        /// Total varighed inkl. reklamer og oprydning i minutter.
        /// </summary>
        public int DurationWithExtras => Movie.DurationMin + AdsMinutes + CleaningMinutes;

        /// <summary>
        /// Sluttidspunkt for forestillingen (start + total varighed).
        /// </summary>
        public DateTime End => Start.AddMinutes(DurationWithExtras);

        /// <summary>
        /// Farve til forestillings-blokken (read-only, tildelt baseret på film-ID).
        /// </summary>
        public SolidColorBrush MovieColor { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Canvas-layout properties (beregnede positioner og dimensioner)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Horisontal position på Canvas i pixels.
        /// Beregnet baseret på starttidspunkt og PixelsPerHour skala.
        /// </summary>
        public double TimelinePosition
        {
            get
            {
                var totalHours = Start.Hour + (Start.Minute / 60.0);
                return totalHours * PixelsPerHour;
            }
        }

        /// <summary>
        /// Bredde af forestillings-blokken på Canvas i pixels.
        /// Beregnet baseret på total varighed og PixelsPerHour skala.
        /// Minimum 6 pixels for synlighed.
        /// </summary>
        public double TimelineWidth
        {
            get
            {
                var hours = DurationWithExtras / 60.0;
                return Math.Max(6.0, hours * PixelsPerHour);
            }
        }

        /// <summary>
        /// Vertikal position på Canvas i pixels (radindex).
        /// Sættes af TimelineDayVM for at undgå overlap.
        /// </summary>
        private double _timelineTop = 5;
        public double TimelineTop => _timelineTop;

        /// <summary>
        /// Sætter vertikal position og trigger UI-opdatering.
        /// Kaldes af TimelineDayVM når layout beregnes.
        /// </summary>
        public void SetTimelineTop(double top)
        {
            _timelineTop = top;
            OnPropertyChanged(nameof(TimelineTop));
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  UI-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Tooltip-tekst til visning når musen holdes over forestillings-blokken.
        /// Indeholder alle relevante detaljer om forestillingen.
        /// </summary>
        public string TooltipText =>
            $"Film: {MovieTitle}\n" +
            $"Start: {Start:HH:mm}\n" +
            $"Slut: {End:HH:mm}\n" +
            $"Varighed: {DurationWithExtras} min\n" +
            $"Sal: {AuditoriumName}\n" +
            $"Klik for at redigere";

        // ─────────────────────────────────────────────────────────────────────────
        //  INotifyPropertyChanged-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for binding. [CallerMemberName] indsætter automatisk
        /// navnet på den property der kaldte metoden – så vi undgår "magiske strenge".
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
