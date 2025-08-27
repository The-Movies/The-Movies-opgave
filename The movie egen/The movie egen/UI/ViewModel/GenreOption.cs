// System-navneområder (standard)
using System.ComponentModel;               // INotifyPropertyChanged (MVVM-binding)
using System.Runtime.CompilerServices;     // [CallerMemberName] til OnPropertyChanged

// Domænemodeller
using The_movie_egen.Model.Enums;          // Genre (Flags-enum)

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til en genre-valgmulighed i film-registreringsformularen.
    /// - Repræsenterer én genre som en "chip"/toggle-knap i UI
    /// - Holder genre-værdi, visningstekst og valg-status
    /// - Implementerer INotifyPropertyChanged for data-binding
    /// - Bemærk: Dette er en UI-klasse, ikke domæne-entity
    /// </summary>
    public sealed class GenreOption : INotifyPropertyChanged
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings) - Read-only properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Enum-værdien for genren (fx Genre.Action).
        /// Bruges når ViewModel'en samler alle valgte genrer til et Flags-sæt.
        /// Init-only: Du vælger genrer som klistermærker — først når du trykker 'Tilføj', sætter vi dem fast.
        /// </summary>
        public Genre Value { get; init; }

        /// <summary>
        /// Teksten der vises på chippen (typisk Value.ToString()).
        /// Init-only: Du kan ikke ændre teksten senere, kun vælge genrer.
        /// </summary>
        public string Label { get; init; } = "";

        // ─────────────────────────────────────────────────────────────────────────
        //  Formularfelter (editables)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Om brugeren har valgt genren i UI'et.
        /// Når den ændres, rejser vi PropertyChanged så bindings opdateres.
        /// Undgår unødige UI-opdateringer hvis værdien ikke reelt ændrer sig.
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // Undgå unødige UI-opdateringer hvis værdien ikke reelt ændrer sig
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  INotifyPropertyChanged-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Standard MVVM-event. WPF lytter på den her for at refreshe bindinger.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for binding. [CallerMemberName] indsætter automatisk
        /// navnet på den property der kaldte metoden – så vi undgår "magiske strenge".
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
