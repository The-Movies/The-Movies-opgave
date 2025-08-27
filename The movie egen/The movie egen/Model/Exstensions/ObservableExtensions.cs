// System-navneområder (standard)
using System;                             // Func<T, TResult>
using System.Collections.ObjectModel;     // ObservableCollection<T>
using System.Linq;                        // OrderBy, ToList

namespace The_movie_egen.Model.Exstensions
{
    /// <summary>
    /// Extension metoder til ObservableCollection for nem håndtering.
    /// - Giver hjælpefunktioner til sortering og manipulation
    /// - Optimeret for UI-binding med færre events
    /// - Kræver kald på UI-tråden (ObservableCollection er UI-bound)
    /// - Bruges i ViewModels til at håndtere observable samlinger
    /// </summary>
    public static class ObservableExtensions
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Sorting operations
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sorterer ObservableCollection in-place efter en nøgle.
        /// Optimeret for UI-binding ved at bruge Move() i stedet for Clear/Add.
        /// Dette minimerer antal CollectionChanged events og forbedrer UI-performance.
        /// </summary>
        /// <typeparam name="T">Type af elementer i samlingen</typeparam>
        /// <typeparam name="TKey">Type af sorteringsnøgle</typeparam>
        /// <param name="col">ObservableCollection at sortere</param>
        /// <param name="keySelector">Funktion der udtrækker sorteringsnøgle fra hvert element</param>
        /// <example>
        /// movies.SortBy(m => m.Title);           // sorter efter titel
        /// movies.SortBy(m => m.DurationMin);     // sorter efter varighed
        /// </example>
        public static void SortBy<T, TKey>(
            this ObservableCollection<T> col,
            Func<T, TKey> keySelector)
        {
            // Sorter elementerne og konverter til liste
            var ordered = col.OrderBy(keySelector).ToList();

            // Flyt elementer til korrekte positioner ved hjælp af Move()
            for (int newIndex = 0; newIndex < ordered.Count; newIndex++)
            {
                var item = ordered[newIndex];
                var oldIndex = col.IndexOf(item);
                
                // Flyt kun hvis elementet ikke allerede er på rette plads
                if (oldIndex >= 0 && oldIndex != newIndex)
                    col.Move(oldIndex, newIndex);
            }
        }

    }
}
