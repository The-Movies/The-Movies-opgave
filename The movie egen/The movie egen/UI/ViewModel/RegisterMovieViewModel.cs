// System-navneområder (standard)
using System;                             // Exception, Enum, etc.
using System.Collections.ObjectModel;     // ObservableCollection<T> (til binding)
using System.ComponentModel;              // INotifyPropertyChanged (MVVM)
using System.Linq;                        // LINQ (Where/Select/Aggregate)
using System.Runtime.CompilerServices;    // [CallerMemberName] til OnPropertyChanged
using System.Windows.Input;               // ICommand / CommandManager

// Domænemodeller og infrastruktur
using The_movie_egen.Model;               // Movie
using The_movie_egen.Model.Enums;         // Genre, FilmStatus
using The_movie_egen.Model.Repositories;  // IMovieRepository
using The_movie_egen.Services;            // MovieRegistry (forretningsregler)

// UI-kommando
using The_movie_egen.UI.Commands;         // RelayCommand
using The_movie_egen.Model.Exstensions; 

namespace The_movie_egen.UI.ViewModel
{
    /// <summary>
    /// ViewModel til skærmbilledet "Registrér ny film".
    /// - Holder formularfelter (Title, DurationText, GenreOptions)
    /// - Udstiller en kommando (RegisterMovieCommand)
    /// - Synkroniserer listevisning (Movies) via data-binding
    /// </summary>
    public sealed class RegisterMovieViewModel : INotifyPropertyChanged
    {
        // Afhængigheder injiceres udefra (nemt at teste/mokke)
        private readonly MovieRegistry _registry;     // indeholder validering + regler
        private readonly IMovieRepository _repo;      // JSON/fil-repo (persistens)

        /// <summary>
        /// Ctor: indlæs eksisterende film, opbyg genre-valg og klargør kommando.
        /// </summary>
        public RegisterMovieViewModel(MovieRegistry registry, IMovieRepository repo)
        {
            _registry = registry;
            _repo = repo;

            // Fyld listen i højre side med allerede gemte film
            Movies = new ObservableCollection<Movie>(_repo.GetAll());

            // Byg "chips"/checkbokse ud fra Flags-enum (spring None over)
            GenreOptions = new ObservableCollection<GenreOption>(
                Enum.GetValues(typeof(Genre)).Cast<Genre>()
                    .Where(g => g != Genre.None)
                    .Select(g => new GenreOption
                    {
                        Value = g,
                        Label = g.ToString(),
                        IsSelected = false
                    })
            );

            // ICommand der binder til "Tilføj"-knappen
            // - Execute  -> RegisterMovie
            // - CanExecute evalueres løbende (se CanRegisterMovie)
            RegisterMovieCommand = new RelayCommand(RegisterMovie, CanRegisterMovie);
            
            // Søge og rediger kommandoer
            SearchCommand = new RelayCommand(SearchMovies);
            EditMovieCommand = new RelayCommand<Movie>(EditMovie);
            DeleteMovieCommand = new RelayCommand<Movie>(DeleteMovie);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Publict API til View (bindings)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Liste over registrerede film (binder til DataGrid).
        /// </summary>
        public ObservableCollection<Movie> Movies { get; }

        /// <summary>
        /// Filtreret liste baseret på søgetekst.
        /// </summary>
        public IEnumerable<Movie> FilteredMovies => 
            string.IsNullOrWhiteSpace(SearchText) 
                ? Movies 
                : Movies.Where(m => 
                    m.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    m.Genres.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Valgt film i listen.
        /// </summary>
        private Movie? _selectedMovie;
        public Movie? SelectedMovie
        {
            get => _selectedMovie;
            set { _selectedMovie = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// UI-valgmuligheder for genrer (én pr. flag i enum).
        /// </summary>
        public ObservableCollection<GenreOption> GenreOptions { get; }

        // Formularfelter (Title + DurationText) – når de ændres,
        // trigges OnPropertyChanged (UI opdaterer), og knappen reevalueres.

        private string? _title;
        /// <summary>
        /// Titel fra formularen (kræves ikke-null/ikke-blank for at kunne gemme).
        /// </summary>
        public string? Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); RaiseCanExec(); }
        }

        private string? _durationText = "";
        /// <summary>
        /// Varighed i minutter som tekst (vi parser til int ved gem).
        /// </summary>
        public string? DurationText
        {
            get => _durationText;
            set { _durationText = value; OnPropertyChanged(); RaiseCanExec(); }
        }

        private string? _error;
        /// <summary>
        /// Fejlbesked til visning i UI (tom/null = ingen fejl).
        /// </summary>
        public string? ErrorMessage
        {
            get => _error;
            set { _error = value; OnPropertyChanged(); }
        }

        private string? _searchText = "";
        /// <summary>
        /// Søgetekst til at filtrere film.
        /// </summary>
        public string? SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredMovies)); }
        }

        /// <summary>
        /// Kommando bundet til "Tilføj"-knappen.
        /// </summary>
        public ICommand RegisterMovieCommand { get; }

        /// <summary>
        /// Kommando til at søge efter film.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Kommando til at redigere en film.
        /// </summary>
        public ICommand EditMovieCommand { get; }

        /// <summary>
        /// Kommando til at slette en film.
        /// </summary>
        public ICommand DeleteMovieCommand { get; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Kommando: CanExecute + Execute
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returnerer true hvis formularen er "gyldig nok" til at kunne gemme:
        ///  - Titel må ikke være tom/whitespace
        ///  - Varighed skal være heltal i intervallet 1..600
        ///  - Mindst én genre skal være valgt
        /// </summary>
        private bool CanRegisterMovie()
        {
            if (string.IsNullOrWhiteSpace(Title)) return false;

            if (!int.TryParse(DurationText, out var d) || d < 1 || d > 600)
                return false;

            return GenreOptions.Any(o => o.IsSelected);
        }

        /// <summary>
        /// Udfører registreringen:
        /// 1) Rydder tidligere fejl
        /// 2) Parser varighed og bygger Genre-flags ud fra valgte chips
        /// 3) Kalder domain-service (Registry) som validerer og gemmer via repo
        /// 4) Opdaterer UI-liste og rydder formularen
        /// 5) Viser evt. fejlbesked fra exception
        /// </summary>
        private void RegisterMovie()
        {
            try
            {
                // 1) Ryd tidligere fejl
                ErrorMessage = null;

                // 2) Valider input først
                if (string.IsNullOrWhiteSpace(Title))
                {
                    ErrorMessage = "Titel er påkrævet.";
                    return;
                }

                if (!int.TryParse(DurationText, out var duration) || duration < 1 || duration > 600)
                {
                    ErrorMessage = "Varighed skal være et tal mellem 1 og 600 minutter.";
                    return;
                }

                var selectedGenres = GenreOptions.Where(o => o.IsSelected).ToList();
                if (!selectedGenres.Any())
                {
                    ErrorMessage = "Vælg mindst én genre.";
                    return;
                }

                // 3) Byg genre flags
                var genres = selectedGenres
                    .Select(o => o.Value)
                    .Aggregate(Genre.None, (acc, g) => acc | g);

                // 4) Forretningsregler + persistens (kan kaste exception)
                var movie = _registry.RegisterMovie(Title.Trim(), duration, genres);

                // 5) UI-opdatering
                Movies.Add(movie);           // DataGrid opdaterer automatisk (ObservableCollection)
                Title = string.Empty; // Ryd formular
                DurationText = string.Empty;
                foreach (var g in GenreOptions) g.IsSelected = false;
            }
            catch (Exception ex)
            {
                // 6) Vis kort, brugbar fejl (Registry skriver meningsfulde beskeder)
                ErrorMessage = ex.Message;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  INotifyPropertyChanged-hjælpere
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Beder WPF om at re-evaluere alle knappers CanExecute (fx når Title/Duration ændres).
        /// </summary>
        private static void RaiseCanExec() => CommandManager.InvalidateRequerySuggested();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Rejser PropertyChanged for binding. [CallerMemberName] indsætter automatisk
        /// navnet på den property der kaldte metoden – så vi undgår "magiske strenge".
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ─────────────────────────────────────────────────────────────────────────
        //  Søge og rediger funktioner
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Søger efter film baseret på SearchText.
        /// </summary>
        private void SearchMovies()
        {
            // Søgningen sker automatisk via FilteredMovies property
            // Dette er bare en placeholder for fremtidige søgefunktioner
        }

        /// <summary>
        /// Åbner redigeringsvindue for en film.
        /// </summary>
        private void EditMovie(Movie? movie)
        {
            if (movie == null) return;

            try
            {
                // For nu, lad os bare vise filmens detaljer i en MessageBox
                // I fremtiden kan dette åbne et redigeringsvindue
                var message = $"Redigerer film: {movie.Title}\n\n" +
                             $"Titel: {movie.Title}\n" +
                             $"Varighed: {movie.DurationMin} minutter\n" +
                             $"Genrer: {movie.Genres}\n" +
                             $"Status: {movie.Status}\n\n" +
                             "Redigeringsfunktion kommer snart!";
                
                System.Windows.MessageBox.Show(message, "Rediger Film", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved redigering: {ex.Message}";
            }
        }

        /// <summary>
        /// Sletter en film efter bekræftelse.
        /// </summary>
        private void DeleteMovie(Movie? movie)
        {
            if (movie == null) return;

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"Er du sikker på, at du vil slette filmen '{movie.Title}'?",
                    "Bekræft sletning",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Fjern fra repository
                    _repo.Delete(movie.Id);
                    
                    // Fjern fra UI liste
                    Movies.Remove(movie);
                    
                    // Ryd valgt film hvis det var den der blev slettet
                    if (SelectedMovie == movie)
                    {
                        SelectedMovie = null;
                    }
                    
                    ErrorMessage = $"Film '{movie.Title}' blev slettet.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved sletning: {ex.Message}";
            }
        }
    }
}
