// System-navneområder (standard)
using System;                             // Exception, etc.
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows;                     // MessageBox
using System.Windows.Input;               // ICommand / CommandManager

// Domænemodeller og infrastruktur
using The_movie_egen.Model.Cinemas;       // Auditorium
using The_movie_egen.Services;            // AuditoriumService (forretningsregler)

// UI-komponenter
using The_movie_egen.UI.Commands;         // RelayCommand

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til dialog-vinduet "Tilføj/Rediger sal".
    /// - Håndterer både tilføjelse af nye sale og redigering af eksisterende
    /// - Validerer input (navn og antal pladser)
    /// - Udstiller kommandoer til at gemme eller annullere
    /// - Synkroniserer med AuditoriumService for forretningsregler
    /// </summary>
    public sealed class EditAuditoriumViewModel : INotifyPropertyChanged
    {
        // Afhængigheder injiceres udefra (nemt at teste/mokke)
        private readonly AuditoriumService _auditoriumService;     // indeholder validering + regler
        private readonly Action _onClose;                          // callback til at lukke dialog
        private readonly Auditorium? _existingAuditorium;          // null = ny sal, ikke-null = rediger

        /// <summary>
        /// Ctor: initialiserer formularfelter og opbygger kommandoer.
        /// </summary>
        public EditAuditoriumViewModel(AuditoriumService auditoriumService, int cinemaId, Action onClose, Auditorium? existingAuditorium = null)
        {
            _auditoriumService = auditoriumService;
            _onClose = onClose;
            _existingAuditorium = existingAuditorium;
            CinemaId = cinemaId;

            // Hvis vi redigerer en eksisterende sal, udfyld formularfelterne
            if (existingAuditorium != null)
            {
                Name = existingAuditorium.Name;
                Seats = existingAuditorium.Seats;
                IsEditMode = true;
            }

            // Opbyg kommandoer med CanExecute-logik
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(() => _onClose());
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings) - Read-only properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// ID på biografen som salen tilhører (read-only).
        /// </summary>
        public int CinemaId { get; }

        /// <summary>
        /// True hvis vi redigerer en eksisterende sal, false hvis vi tilføjer en ny.
        /// Bruges til at vise korrekt titel og knaptekst i UI.
        /// </summary>
        public bool IsEditMode { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Formularfelter (editables)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sal-navn fra formularen (kræves ikke-tom/ikke-blank for at kunne gemme).
        /// Når ændret, trigges OnPropertyChanged og knapper reevalueres.
        /// </summary>
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Re-evaluer knapper
            }
        }

        /// <summary>
        /// Antal pladser i salen (skal være > 0 for at kunne gemme).
        /// Når ændret, trigges OnPropertyChanged og knapper reevalueres.
        /// </summary>
        private int _seats = 1;
        public int Seats
        {
            get => _seats;
            set
            {
                _seats = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Re-evaluer knapper
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommandoer til UI-knapper
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kommando til at gemme salen (tilføj eller opdater).
        /// </summary>
        public RelayCommand SaveCommand { get; }

        /// <summary>
        /// Kommando til at annullere og lukke dialogen.
        /// </summary>
        public RelayCommand CancelCommand { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommando: CanExecute + Execute
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returnerer true hvis formularen er "gyldig nok" til at kunne gemme:
        ///  - Navn må ikke være tom/whitespace
        ///  - Antal pladser skal være > 0
        /// </summary>
        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && Seats > 0;
        }

        /// <summary>
        /// Gemmer salen (tilføjer ny eller opdaterer eksisterende).
        /// 1) Tjekker om vi er i edit-mode eller add-mode
        /// 2) Kalder service til at gemme/opdatere salen
        /// 3) Lukker dialogen via callback
        /// 4) Håndterer fejl og viser feedback
        /// </summary>
        private void Save()
        {
            try
            {
                if (IsEditMode && _existingAuditorium != null)
                {
                    // Opdater eksisterende sal
                    _auditoriumService.UpdateAuditorium(_existingAuditorium.Id, Name, Seats);
                }
                else
                {
                    // Tilføj ny sal
                    _auditoriumService.AddAuditorium(CinemaId, Name, Seats);
                }

                // Luk dialog via callback
                _onClose();
            }
            catch (Exception ex)
            {
                // Vis fejlbesked fra service (indeholder meningsfulde beskeder)
                MessageBox.Show(ex.Message, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  INotifyPropertyChanged-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for binding. [CallerMemberName] indsætter automatisk
        /// navnet på den property der kaldte metoden – så vi undgår "magiske strenge".
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
