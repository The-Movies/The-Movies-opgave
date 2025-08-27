// System-navneområder (standard)
using System;                             // Exception, DateTime, etc.
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Linq;                        // LINQ (Where/Select/ToList)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows;                     // MessageBox, Application
using System.Windows.Input;               // ICommand / CommandManager

// Domænemodeller og infrastruktur
using The_movie_egen.Model.Cinemas;       // Cinema, Auditorium
using The_movie_egen.Model.Repositories;  // ICinemaRepository
using The_movie_egen.Services;            // AuditoriumService (forretningsregler)

// UI-komponenter
using The_movie_egen.UI.Commands;         // RelayCommand
using The_movie_egen.UI.View;             // EditAuditoriumWindow
using The_movie_egen.UI.ViewModel;        // EditAuditoriumViewModel

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til skærmbilledet "Biografer og sale".
    /// - Viser liste over biografer og deres sale
    /// - Udstiller kommandoer til at tilføje, redigere og slette sale
    /// - Håndterer valg af biograf og sal via data-binding
    /// - Synkroniserer med AuditoriumService for forretningsregler
    /// </summary>
    public sealed class CinemasViewModel : INotifyPropertyChanged
    {
        // Afhængigheder injiceres udefra (nemt at teste/mokke)
        private readonly AuditoriumService _auditoriumService;     // indeholder validering + regler
        private readonly ICinemaRepository _cinemaRepository;      // JSON/fil-repo (persistens)

        /// <summary>
        /// Ctor: initialiserer biograf-liste og opbygger kommandoer.
        /// </summary>
        public CinemasViewModel(ObservableCollection<Cinema> cinemas, AuditoriumService auditoriumService, ICinemaRepository cinemaRepository)
        {
            // Valider input og sæt dependencies
            Cinemas = cinemas ?? new ObservableCollection<Cinema>();
            _auditoriumService = auditoriumService;
            _cinemaRepository = cinemaRepository;

            // Opbyg kommandoer med CanExecute-logik
            AddAuditoriumCommand = new RelayCommand(AddAuditorium, CanAddAuditorium);
            EditAuditoriumCommand = new RelayCommand(EditAuditorium, CanEditAuditorium);
            DeleteAuditoriumCommand = new RelayCommand(DeleteAuditorium, CanDeleteAuditorium);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Liste over alle biografer (deler samme samling som resten af appen).
        /// Ændringer kan genbruges i andre dele af systemet.
        /// </summary>
        public ObservableCollection<Cinema> Cinemas { get; }

        /// <summary>
        /// Valgt biograf i listen (binder til ListBox/ComboBox).
        /// Når ændret, trigges OnPropertyChanged og knapper reevalueres.
        /// </summary>
        private Cinema? _selectedCinema;
        public Cinema? SelectedCinema
        {
            get => _selectedCinema;
            set
            {
                _selectedCinema = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Re-evaluer knapper
            }
        }

        /// <summary>
        /// Valgt sal i den valgte biograf (binder til ListBox/DataGrid).
        /// Når ændret, trigges OnPropertyChanged og knapper reevalueres.
        /// </summary>
        private Auditorium? _selectedAuditorium;
        public Auditorium? SelectedAuditorium
        {
            get => _selectedAuditorium;
            set
            {
                _selectedAuditorium = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Re-evaluer knapper
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommandoer til UI-knapper
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kommando til at tilføje en ny sal til den valgte biograf.
        /// </summary>
        public RelayCommand AddAuditoriumCommand { get; }

        /// <summary>
        /// Kommando til at redigere den valgte sal.
        /// </summary>
        public RelayCommand EditAuditoriumCommand { get; }

        /// <summary>
        /// Kommando til at slette den valgte sal (efter bekræftelse).
        /// </summary>
        public RelayCommand DeleteAuditoriumCommand { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommando: CanExecute + Execute
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returnerer true hvis en biograf er valgt og der kan tilføjes en sal.
        /// </summary>
        private bool CanAddAuditorium()
        {
            return SelectedCinema != null;
        }

        /// <summary>
        /// Åbner dialog til at tilføje en ny sal til den valgte biograf.
        /// 1) Validerer at en biograf er valgt
        /// 2) Opretter EditAuditoriumViewModel med callback
        /// 3) Åbner EditAuditoriumWindow som dialog
        /// 4) Opdaterer biograf-liste efter tilføjelse
        /// </summary>
        private void AddAuditorium()
        {
            if (SelectedCinema == null)
            {
                MessageBox.Show("Vælg venligst en biograf først.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Opret ViewModel med callback til at opdatere listen
            var viewModel = new EditAuditoriumViewModel(_auditoriumService, SelectedCinema.Id, () => 
            {
                RefreshCinemas(); // Opdater listen efter tilføjelse
            });

            // Åbn dialog og vent på resultat
            var window = new EditAuditoriumWindow(viewModel);
            window.ShowDialog();
        }

        /// <summary>
        /// Returnerer true hvis en sal er valgt og kan redigeres.
        /// </summary>
        private bool CanEditAuditorium()
        {
            return SelectedAuditorium != null;
        }

        /// <summary>
        /// Åbner dialog til at redigere den valgte sal.
        /// 1) Validerer at en sal er valgt
        /// 2) Opretter EditAuditoriumViewModel med eksisterende sal
        /// 3) Åbner EditAuditoriumWindow som dialog
        /// 4) Opdaterer biograf-liste efter redigering
        /// </summary>
        private void EditAuditorium()
        {
            if (SelectedAuditorium == null)
            {
                MessageBox.Show("Vælg venligst en sal først.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Opret ViewModel med eksisterende sal og callback
            var viewModel = new EditAuditoriumViewModel(_auditoriumService, SelectedAuditorium.CinemaId, () => 
            {
                RefreshCinemas(); // Opdater listen efter redigering
            }, SelectedAuditorium);

            // Åbn dialog og vent på resultat
            var window = new EditAuditoriumWindow(viewModel);
            window.ShowDialog();
        }

        /// <summary>
        /// Returnerer true hvis en sal er valgt og kan slettes.
        /// Tjekker også om salen faktisk kan slettes via service.
        /// </summary>
        private bool CanDeleteAuditorium()
        {
            return SelectedAuditorium != null && _auditoriumService.CanDeleteAuditorium(SelectedAuditorium.Id);
        }

        /// <summary>
        /// Sletter den valgte sal efter bekræftelse.
        /// 1) Validerer at en sal er valgt
        /// 2) Viser bekræftelsesdialog
        /// 3) Kalder service til at slette salen
        /// 4) Opdaterer UI og rydder valg
        /// 5) Håndterer fejl og viser feedback
        /// </summary>
        private void DeleteAuditorium()
        {
            if (SelectedAuditorium == null)
            {
                MessageBox.Show("Vælg venligst en sal først.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Vis bekræftelsesdialog
                var result = MessageBox.Show(
                    $"Er du sikker på, at du vil slette salen '{SelectedAuditorium.Name}'?",
                    "Bekræft sletning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Slet via service (kan kaste exception)
                    _auditoriumService.DeleteAuditorium(SelectedAuditorium.Id);
                    
                    // Opdater UI
                    RefreshCinemas();
                    SelectedAuditorium = null;
                    
                    // Vis success feedback
                    MessageBox.Show("Salen blev slettet.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Service-specifikke fejl (fx "sal har forestillinger")
                MessageBox.Show(ex.Message, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Generelle fejl
                MessageBox.Show($"Der opstod en fejl: {ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Hjælpefunktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Opdaterer biograf-listen fra repository.
        /// Bruges efter tilføjelse, redigering eller sletning af sale.
        /// </summary>
        private void RefreshCinemas()
        {
            // Hent opdaterede data fra repository
            var updatedCinemas = _cinemaRepository.GetAll().ToList();
            
            // Opdater ObservableCollection (UI opdaterer automatisk)
            Cinemas.Clear();
            foreach (var cinema in updatedCinemas)
            {
                Cinemas.Add(cinema);
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
