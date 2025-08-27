// System-navneområder (standard)
using System;                             // Exception, NotImplementedException, Type
using System.Globalization;               // CultureInfo

// UI-komponenter
using System.Windows.Data;                // IValueConverter

namespace The_movie_egen.UI.Converters
{
    /// <summary>
    /// WPF value converter der konverterer bool til tekst baseret på parameter.
    /// - Konverterer true/false til forskellige tekster
    /// - Parameter format: "trueText|falseText" (pipe-separeret)
    /// - Bruges til at vise forskellige tekster baseret på bool-status
    /// - Kun one-way binding (ConvertBack kaster NotImplementedException)
    /// 
    /// Brugseksempel i XAML:
    /// Text="{Binding IsActive, Converter={StaticResource BoolToTitleConverter}, ConverterParameter='Aktiv|Inaktiv'}"
    /// </summary>
    public class BoolToTitleConverter : IValueConverter
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  IValueConverter implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Konverterer bool-værdi til tekst baseret på parameter.
        /// Parameter skal være i format "trueText|falseText".
        /// </summary>
        /// <param name="value">Bool-værdi at konvertere</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Tekst-parameter i format "trueText|falseText"</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Tekst baseret på bool-værdi og parameter</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Tjek om input er bool og parameter er string
            if (value is bool boolValue && parameter is string options)
            {
                // Split parameter på pipe-tegn
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    // Returner første del for true, anden del for false
                    return boolValue ? parts[0] : parts[1];
                }
            }
            
            // Fallback: returner value som string eller tom string
            return value?.ToString() ?? "";
        }

        /// <summary>
        /// Konverterer tekst tilbage til bool (ikke implementeret).
        /// Kaster NotImplementedException da converter kun understøtter one-way binding.
        /// </summary>
        /// <param name="value">Tekst at konvertere (ignoreres)</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Ikke implementeret</returns>
        /// <exception cref="NotImplementedException">Altid kastet</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BoolToTitleConverter understøtter kun one-way binding.");
        }
    }
}
