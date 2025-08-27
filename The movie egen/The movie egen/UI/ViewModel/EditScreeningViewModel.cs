// System-navneområder (standard)
using System;                             // Exception, DateTime, TimeSpan, etc.
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows;                     // MessageBox, Application, Window
using System.Windows.Input;               // ICommand

// Domænemodeller og infrastruktur
using The_movie_egen.Services;            // SchedulingService (forretningsregler)

// UI-komponenter
using The_movie_egen.UI.Commands;         // RelayCommand
using The_movie_egen.UI.ViewModel;        // TimelineScreeningVM

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til dialog-vinduet "Rediger forestilling".
    /// - Viser detaljer om en eksisterende forestilling
    /// - Tillader redigering af dato og starttidspunkt
    /// - Udstiller kommandoer til at gemme ændringer eller slette forestillingen
    /// - Håndterer validering af input og fejlhåndtering
    /// </summary>
    public class EditScreeningViewModel : INotifyPropertyChanged
    {
        // Afhængigheder injiceres udefra (nemt at teste/mokke)
        private readonly SchedulingService _schedulingService;     // indeholder validering + regler
        private readonly TimelineScreeningVM _screening;          // forestillingen der redigeres
        private readonly Action _refreshCallback;                  // callback til at opdatere UI

        /// <summary>
        /// Ctor: initialiserer formularfelter og opbygger kommandoer.
        /// </summary>
        public EditScreeningViewModel(
            TimelineScreeningVM screening, 
            SchedulingService schedulingService,
            Action refreshCallback)
        {
            _screening = screening;
            _schedulingService = schedulingService;
            _refreshCallback = refreshCallback;

            // Initialiser formularfelter med eksisterende værdier
            EditDate = screening.Start.Date;
            EditStartTime = screening.Start.ToString("HH:mm");

            // Opbyg kommandoer
            SaveChangesCommand = new RelayCommand(SaveChanges);
            DeleteScreeningCommand = new RelayCommand(DeleteScreening);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings) - Read-only properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Filmtitel (read-only, vises som information).
        /// </summary>
        public string MovieTitle => _screening.MovieTitle;

        /// <summary>
        /// Sal-navn (read-only, vises som information).
        /// </summary>
        public string AuditoriumName => _screening.AuditoriumName;

        /// <summary>
        /// Film-genrer (read-only, vises som information).
        /// </summary>
        public string MovieGenre => _screening.Movie.Genres.ToString();

        /// <summary>
        /// Film-varighed i minutter (read-only, vises som information).
        /// </summary>
        public string MovieDuration => $"{_screening.Movie.DurationMin} min";

        /// <summary>
        /// Reklametid i minutter (read-only, vises som information).
        /// </summary>
        public int AdsMinutes => _screening.AdsMinutes;

        /// <summary>
        /// Oprydningstid i minutter (read-only, vises som information).
        /// </summary>
        public int CleaningMinutes => _screening.CleaningMinutes;

        // ─────────────────────────────────────────────────────────────────────────
        //  Formularfelter (editables)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Valgt dato for forestillingen (binder til DatePicker).
        /// </summary>
        private DateTime _editDate;
        public DateTime EditDate
        {
            get => _editDate;
            set { _editDate = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Starttidspunkt som tekst i HH:mm format (binder til TextBox).
        /// </summary>
        private string _editStartTime = string.Empty;
        public string EditStartTime
        {
            get => _editStartTime;
            set { _editStartTime = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommandoer til UI-knapper
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kommando til at gemme ændringer til forestillingen.
        /// </summary>
        public ICommand SaveChangesCommand { get; }

        /// <summary>
        /// Kommando til at slette forestillingen (efter bekræftelse).
        /// </summary>
        public ICommand DeleteScreeningCommand { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommando: Execute
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gemmer ændringer til forestillingen.
        /// 1) Validerer input (tidspunkt format)
        /// 2) Kombinerer dato og tid til DateTime
        /// 3) Sletter gammel forestilling
        /// 4) Opretter ny forestilling med opdaterede værdier
        /// 5) Kalder callback til at opdatere UI
        /// 6) Lukker dialog-vinduet
        /// 7) Håndterer fejl og viser feedback
        /// </summary>
        private void SaveChanges()
        {
            try
            {
                // 1) Valider tidspunkt format
                if (!TimeSpan.TryParse(EditStartTime, out var startTime))
                {
                    MessageBox.Show("Ugyldigt tidspunkt. Brug HH:MM format.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2) Kombiner dato og tid
                var newStart = EditDate.Date + startTime;

                // 3) Slet gammel forestilling
                _schedulingService.RemoveScreening(_screening.Id);

                // 4) Opret ny forestilling med opdaterede værdier
                _schedulingService.AddScreening(
                    _screening.CinemaId,
                    _screening.Auditorium.Id,
                    _screening.Movie.Id,
                    newStart,
                    AdsMinutes,
                    CleaningMinutes
                );

                // 5) Opdater UI og luk dialog
                _refreshCallback?.Invoke();
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fejl ved gemning: {ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sletter forestillingen efter bekræftelse.
        /// 1) Viser bekræftelsesdialog
        /// 2) Sletter forestillingen via service
        /// 3) Kalder callback til at opdatere UI
        /// 4) Lukker dialog-vinduet
        /// 5) Håndterer fejl og viser feedback
        /// </summary>
        private void DeleteScreening()
        {
            var result = MessageBox.Show(
                "Er du sikker på, at du vil slette denne forestilling?",
                "Bekræft sletning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Slet forestillingen
                    _schedulingService.RemoveScreening(_screening.Id);
                    
                    // Opdater UI og luk dialog
                    _refreshCallback?.Invoke();
                    CloseWindow();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fejl ved sletning: {ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Hjælpefunktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lukker dialog-vinduet ved at finde det aktuelle window.
        /// Søger gennem alle åbne vinduer og lukker det der har denne ViewModel som DataContext.
        /// </summary>
        private void CloseWindow()
        {
            if (Application.Current.MainWindow is Window mainWindow)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.Close();
                        break;
                    }
                }
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
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
