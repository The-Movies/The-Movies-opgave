// System-navneområder (standard)
using System;                             // Exception, etc.
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows.Input;               // ICommand

// Domænemodeller og infrastruktur
using The_movie_egen.Model.Repositories;  // ICinemaRepository
using The_movie_egen.Services;            // Services (forretningsregler)

// UI-komponenter
using The_movie_egen.UI.Commands;         // RelayCommand

// Type aliases for kortere kode
using CinemaModel = The_movie_egen.Model.Cinemas.Cinema;

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// Hoved-ViewModel til MainWindow (navigation og side-håndtering).
    /// - Holder referencer til alle underliggende ViewModels
    /// - Håndterer navigation mellem forskellige sider
    /// - Udstiller kommandoer til at skifte mellem sider
    /// - Holder styr på hvilken side der vises i øjeblikket
    /// </summary>
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Ctor: initialiserer alle underliggende ViewModels og navigation.
        /// </summary>
        public MainWindowViewModel(
            RegisterMovieViewModel registerVM,
            CinemasViewModel cinemasVM,
            PlanMonthViewModel planVM)
        {
            // Gem referencer til alle sider
            RegisterVM = registerVM;
            CinemasVM = cinemasVM;
            PlanVM = planVM;

            // Sæt startside til film-registrering
            CurrentView = RegisterVM;

            // Opbyg navigation-kommandoer
            ShowRegisterCommand = new RelayCommand(() => CurrentView = RegisterVM);
            ShowCinemasCommand = new RelayCommand(() => CurrentView = CinemasVM);
            ShowPlanCommand = new RelayCommand(() => CurrentView = PlanVM);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings) - Read-only properties
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// ViewModel til film-registreringssiden (read-only).
        /// </summary>
        public RegisterMovieViewModel RegisterVM { get; }

        /// <summary>
        /// ViewModel til biografer og sale-siden (read-only).
        /// </summary>
        public CinemasViewModel CinemasVM { get; }

        /// <summary>
        /// ViewModel til månedsplanlægningssiden (read-only).
        /// </summary>
        public PlanMonthViewModel PlanVM { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Navigation state
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Den nuværende side der vises i hovedvinduet.
        /// Når ændret, trigges OnPropertyChanged og UI opdateres.
        /// </summary>
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Navigation-kommandoer
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Kommando til at vise film-registreringssiden.
        /// </summary>
        public ICommand ShowRegisterCommand { get; }

        /// <summary>
        /// Kommando til at vise biografer og sale-siden.
        /// </summary>
        public ICommand ShowCinemasCommand { get; }

        /// <summary>
        /// Kommando til at vise månedsplanlægningssiden.
        /// </summary>
        public ICommand ShowPlanCommand { get; }

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
