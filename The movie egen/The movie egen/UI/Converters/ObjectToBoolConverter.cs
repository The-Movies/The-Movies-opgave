// System-navneområder (standard)
using System;                             // Exception, NotImplementedException, Type
using System.Globalization;               // CultureInfo

// UI-komponenter
using System.Windows.Data;                // IValueConverter

namespace The_movie_egen.UI.Converters
{
    /// <summary>
    /// WPF value converter der konverterer ethvert objekt til bool-værdi.
    /// - Returnerer true hvis objektet ikke er null
    /// - Returnerer false hvis objektet er null
    /// - Bruges til at aktivere/deaktivere UI-elementer baseret på objekt-eksistens
    /// - Kun one-way binding (ConvertBack kaster NotImplementedException)
    /// - Singleton-instans tilgængelig via Instance property
    /// 
    /// Brugseksempel i XAML:
    /// IsEnabled="{Binding SelectedMovie, Converter={x:Static Converters:ObjectToBoolConverter.Instance}}"
    /// </summary>
    public class ObjectToBoolConverter : IValueConverter
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Static instance (singleton)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Singleton-instans af converteren til brug i XAML.
        /// Bruges i stedet for at oprette ny instans i Resources.
        /// </summary>
        public static readonly ObjectToBoolConverter Instance = new ObjectToBoolConverter();

        // ─────────────────────────────────────────────────────────────────────────
        //  IValueConverter implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Konverterer ethvert objekt til bool-værdi.
        /// Returnerer true hvis objektet ikke er null, false hvis det er null.
        /// </summary>
        /// <param name="value">Objekt at konvertere</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>True hvis objektet ikke er null, false ellers</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        /// <summary>
        /// Konverterer bool tilbage til objekt (ikke implementeret).
        /// Kaster NotImplementedException da converter kun understøtter one-way binding.
        /// </summary>
        /// <param name="value">Bool-værdi at konvertere (ignoreres)</param>
        /// <param name="targetType">Target type (ignoreres)</param>
        /// <param name="parameter">Parameter (ignoreres)</param>
        /// <param name="culture">Culture info (ignoreres)</param>
        /// <returns>Ikke implementeret</returns>
        /// <exception cref="NotImplementedException">Altid kastet</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ObjectToBoolConverter understøtter kun one-way binding.");
        }
    }
}
