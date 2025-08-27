// System-navneområder (standard)
using System;                             // Exception, etc.
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows;                     // MessageBox, Application

// Domænemodeller og infrastruktur
using The_movie_egen.Model.Cinemas;       // Cinema
using The_movie_egen.Model.Repositories;  // IMovieRepository, ICinemaRepository
using The_movie_egen.Services;            // MovieRegistry, SchedulingService, AuditoriumService

// UI-komponenter
using The_movie_egen.UI.Commands;         // RelayCommand

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// Shell-ViewModel til hovedapplikationen (dependency injection og navigation).
    /// - Opretter og holder alle underliggende ViewModels
    /// - Håndterer dependency injection for alle services og repositories
    /// - Udstiller navigation-kommandoer til at skifte mellem sider
    /// - Holder styr på hvilken side der vises i øjeblikket
    /// </summary>
    public sealed class ShellViewModel : INotifyPropertyChanged
    {
        // Afhængigheder injiceres udefra (nemt at teste/mokke)
        private readonly RegisterMovieViewModel _registerVm;     // film-registrering
        private readonly CinemasViewModel _cinemasVm;           // biografer og sale
        private readonly PlanMonthViewModel _programVm;         // månedsplanlægning

        /// <summary>
        /// Ctor: opretter alle ViewModels med dependencies og navigation.
        /// </summary>
        public ShellViewModel(MovieRegistry registry, IMovieRepository movieRepo, ICinemaRepository cinemaRepo, SchedulingService schedulingService, AuditoriumService auditoriumService)
        {
            // Opret ViewModels med dependencies
            _registerVm = new RegisterMovieViewModel(registry, movieRepo);
            _cinemasVm = new CinemasViewModel(new ObservableCollection<Cinema>(cinemaRepo.GetAll()), auditoriumService, cinemaRepo);
            _programVm = new PlanMonthViewModel(schedulingService, movieRepo, new ObservableCollection<Cinema>(cinemaRepo.GetAll()));

            // Opbyg navigation-kommandoer
            NavigateRegisterCommand = new RelayCommand(() => CurrentViewModel = _registerVm);
            NavigateCinemasCommand = new RelayCommand(() => CurrentViewModel = _cinemasVm);
            NavigateProgramCommand = new RelayCommand(() => CurrentViewModel = _programVm);

            // Sæt startside til film-registrering
            CurrentViewModel = _registerVm;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Navigation state
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Den nuværende side der vises i hovedvinduet.
        /// Når ændret, trigges OnPropertyChanged og UI opdateres.
        /// </summary>
        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set { _currentViewModel = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Navigation-kommandoer
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kommando til at navigere til film-registreringssiden.
        /// </summary>
        public RelayCommand NavigateRegisterCommand { get; }

        /// <summary>
        /// Kommando til at navigere til biografer og sale-siden.
        /// </summary>
        public RelayCommand NavigateCinemasCommand { get; }

        /// <summary>
        /// Kommando til at navigere til månedsplanlægningssiden.
        /// </summary>
        public RelayCommand NavigateProgramCommand { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  INotifyPropertyChanged-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for binding. [CallerMemberName] indsætter automatisk
        /// navnet på den property der kaldte metoden – så vi undgår "magiske strenge".
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
