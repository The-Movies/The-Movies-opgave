// System-navneområder (standard)
using System;                             // Exception, NotSupportedException, Type, Math
using System.Globalization;               // CultureInfo

// UI-komponenter
using System.Windows.Data;                // IMultiValueConverter

namespace The_movie_egen.UI.Converters
{
    /// <summary>
    /// WPF multi-value converter til timeline-visning.
    /// - Konverterer minutter og total bredde til pixel-bredde
    /// - Bruges til at beregne hvor bred en forestilling skal være i timeline
    /// - Baserer beregningen på 24-timers døgn (1440 minutter)
    /// - Kun one-way binding (ConvertBack kaster NotSupportedException)
    /// 
    /// Brugseksempel i XAML:
    /// Width="{Binding Path=DurationWithExtras, Converter={StaticResource MinutesToPixelsConverter}, 
    ///                 ConverterParameter=TotalWidth}"
    /// </summary>
    public sealed class MinutesToPixelsConverter : IMultiValueConverter
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Constants
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Antal minutter i et døgn (24 timer * 60 minutter).
        /// Bruges som basis for pixel-beregning.
        /// </summary>
        private const double MinutesPerDay = 24 * 60;

        // ─────────────────────────────────────────────────────────────────────────
        //  IMultiValueConverter implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Konverterer minutter og total bredde til pixel-bredde.
        /// Beregner proportionel bredde baseret på minutter i forhold til et døgn.
        /// </summary>
        /// <param name="values">Array med values: [0] = minutter (int), [1] = total bredde (double)</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Pixel-bredde som double</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Tjek om vi har mindst 2 values
            if (values.Length < 2) return 0d;
            
            // Tjek om første value er int (minutter)
            if (values[0] is not int minutes) return 0d;
            
            // Tjek om andet value er double (total bredde)
            if (values[1] is not double totalWidth) return 0d;

            // Beregn proportionel bredde: (minutter / minutter per døgn) * total bredde
            return (minutes / MinutesPerDay) * Math.Max(0, totalWidth);
        }

        /// <summary>
        /// Konverterer pixel-bredde tilbage til minutter (ikke implementeret).
        /// Kaster NotSupportedException da converter kun understøtter one-way binding.
        /// </summary>
        /// <param name="value">Pixel-bredde at konvertere (ignoreres)</param>
        /// <param name="targetTypes">Target types (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Ikke implementeret</returns>
        /// <exception cref="NotSupportedException">Altid kastet</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException("MinutesToPixelsConverter understøtter kun one-way binding.");
    }
}
