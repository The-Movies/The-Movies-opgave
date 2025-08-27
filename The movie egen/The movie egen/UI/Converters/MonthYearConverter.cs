// System-navneområder (standard)
using System;                             // Exception, NotImplementedException, Type
using System.Globalization;               // CultureInfo

// UI-komponenter
using System.Windows.Data;                // IValueConverter

namespace The_movie_egen.UI.Converters
{
    /// <summary>
    /// WPF value converter der konverterer DateTime til måned/år format.
    /// - Konverterer DateTime til "Måned ÅÅÅÅ" format (fx "September 2025")
    /// - Bruger CurrentCulture for lokalisering af månedsnavne
    /// - Bruges til at vise datoer i brugervenligt format
    /// - Kun one-way binding (ConvertBack kaster NotImplementedException)
    /// 
    /// Brugseksempel i XAML:
    /// Text="{Binding SelectedDate, Converter={StaticResource MonthYearConverter}}"
    /// </summary>
    public class MonthYearConverter : IValueConverter
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  IValueConverter implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Konverterer DateTime til måned/år format.
        /// Formaterer datoen som "Måned ÅÅÅÅ" (fx "September 2025").
        /// </summary>
        /// <param name="value">DateTime at konvertere</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres - bruger CurrentCulture)</param>
        /// <returns>Formateret måned/år string</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Tjek om value er DateTime
            if (value is DateTime dateTime)
            {
                // Formater som "Måned ÅÅÅÅ" med CurrentCulture
                return dateTime.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
            }
            
            // Fallback: returner value som string eller tom string
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Konverterer måned/år string tilbage til DateTime (ikke implementeret).
        /// Kaster NotImplementedException da converter kun understøtter one-way binding.
        /// </summary>
        /// <param name="value">Måned/år string at konvertere (ignoreres)</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Ikke implementeret</returns>
        /// <exception cref="NotImplementedException">Altid kastet</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("MonthYearConverter understøtter kun one-way binding.");
        }
    }
}
