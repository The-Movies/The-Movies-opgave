using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using The_movie_egen.Model;
using The_movie_egen.Model.Repositories;
using The_movie_egen.Services;
using The_movie_egen.UI.Commands;
using The_movie_egen.UI.View;

using CinemaModel = The_movie_egen.Model.Cinemas.Cinema;
using AuditoriumModel = The_movie_egen.Model.Cinemas.Auditorium;

namespace The_movie_egen.UI.ViewModel
{
    // Bruges til måned-dropdown (navn + værdi)
    public sealed class MonthOption
    {
        public string Name { get; }
        public int Value { get; }
        public MonthOption(string name, int value) { Name = name; Value = value; }
        public override string ToString() => Name;
    }

    // Togglebar filterknap for biograf
    public sealed class CinemaFilterVM : INotifyPropertyChanged
    {
        public int CinemaId { get; }
        public string Name { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected == value) return; _isSelected = value; OnPropertyChanged(); Toggled?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler? Toggled;
        public CinemaFilterVM(CinemaModel cinema, bool selected = false)
        {
            CinemaId = cinema.Id;
            Name = cinema.Name;
            _isSelected = selected;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // Dagsgruppe til højre panel (én pr. kalenderdag)
    public sealed class DayGroupVM
    {
        public DateTime Date { get; }
        public string DateText => Date.ToString("dd-MM-yyyy");
        public ObservableCollection<ScreeningVM> Items { get; } = new();

        public DayGroupVM(DateTime date, IEnumerable<ScreeningVM> items)
        {
            Date = date;
            foreach (var s in items.OrderBy(x => x.Start)) Items.Add(s);
        }
    }

    public sealed class PlanMonthViewModel : INotifyPropertyChanged
    {
        private const int CutoffDay = 23;
        private static DateTime FirstOfMonth(DateTime d) => new(d.Year, d.Month, 1);
        private static DateTime CalcDefaultPlanningMonth(DateTime today)
            => FirstOfMonth(today).AddMonths(today.Day > CutoffDay ? 2 : 1);

        private readonly SchedulingService _schedule;
        private readonly IMovieRepository _movieRepo;
        private bool _suspendSync;

        public PlanMonthViewModel(
            SchedulingService schedule,
            IMovieRepository movieRepo,
            ObservableCollection<CinemaModel> cinemas)
        {
            try
            {
                _schedule = schedule;
                _movieRepo = movieRepo;

                Cinemas = cinemas;
                System.Diagnostics.Debug.WriteLine($"PlanMonthViewModel: Loaded {Cinemas.Count} cinemas");
                foreach (var cinema in Cinemas)
                {
                    System.Diagnostics.Debug.WriteLine($"  Cinema: {cinema.Name} with {cinema.Auditoriums.Count} auditoriums");
                }
                
                Movies = new ObservableCollection<Movie>();
                LoadMovies(); // Load movies initially
                System.Diagnostics.Debug.WriteLine($"PlanMonthViewModel: Loaded {Movies.Count} movies");
                
                // Subscribe to movie repository changes
                _movieRepo.MoviesChanged += (_, __) => LoadMovies();
                
                Screenings = new ObservableCollection<ScreeningVM>();

                // Lav filter-VM'er for biografer (første valgt som default)
                CinemaFilters = new ObservableCollection<CinemaFilterVM>(
                    Cinemas.Select((c, i) =>
                    {
                        var vm = new CinemaFilterVM(c, i == 0);
                        vm.Toggled += (_, __) => RefreshMonth();
                        return vm;
                    }));

                var today = DateTime.Today;
                MinMonth = FirstOfMonth(today).AddMonths(-1);
                MaxMonth = FirstOfMonth(today).AddMonths(6);
                SelectedMonth = CalcDefaultPlanningMonth(today); // sætter også pickere via sync

                var culture = CultureInfo.CurrentCulture;
                Months = Enumerable.Range(1, 12)
                    .Select(m => new MonthOption(culture.DateTimeFormat.GetMonthName(m), m))
                    .ToList();
                Years = Enumerable.Range(MinMonth.Year, MaxMonth.Year - MinMonth.Year + 1).ToList();

                _suspendSync = true;
                _selectedMonthNumber = SelectedMonth.Month;
                _selectedYear = SelectedMonth.Year;
                _suspendSync = false;

                AddScreeningCommand = new RelayCommand(() => 
                {
                    System.Diagnostics.Debug.WriteLine("AddScreeningCommand executed");
                    AddScreening();
                }, CanAddScreening);
                
                ClearFormCommand = new RelayCommand(() => 
                {
                    System.Diagnostics.Debug.WriteLine("ClearFormCommand executed");
                    ClearForm();
                });
                
                RemoveSelectedCommand = new RelayCommand(() => 
                {
                    System.Diagnostics.Debug.WriteLine("RemoveSelectedCommand executed");
                    RemoveSelected();
                }, () => SelectedScreening != null);

                EditScreeningCommand = new RelayCommand<TimelineScreeningVM>(screening => 
                {
                    System.Diagnostics.Debug.WriteLine("EditScreeningCommand executed");
                    EditScreening(screening);
                });

                RefreshMonth();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fejl i PlanMonthViewModel constructor: {ex.Message}");
                throw;
            }
        }

        // -------- Bindings / lister --------
        public ObservableCollection<CinemaModel> Cinemas { get; }
        public ObservableCollection<CinemaFilterVM> CinemaFilters { get; }
        public ObservableCollection<Movie> Movies { get; }
        public ObservableCollection<ScreeningVM> Screenings { get; }
        public ObservableCollection<DayGroupVM> DayGroups { get; } = new();
        public ObservableCollection<TimelineDayVM> TimelineDays { get; } = new();

        public IReadOnlyList<MonthOption> Months { get; private set; } = Array.Empty<MonthOption>();
        public IReadOnlyList<int> Years { get; private set; } = Array.Empty<int>();

        private CinemaModel? _selectedCinema;
        public CinemaModel? SelectedCinema
        {
            get => _selectedCinema;
            set 
            { 
                System.Diagnostics.Debug.WriteLine($"SelectedCinema changed from {_selectedCinema?.Name} to {value?.Name}");
                _selectedCinema = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CanPickAuditorium)); 
                RaiseCanExec(); 
                RefreshMonth(); 
            }
        }

        public bool CanPickAuditorium => SelectedCinema != null && SelectedCinema.Auditoriums.Any();

        private AuditoriumModel? _selectedAuditorium;
        public AuditoriumModel? SelectedAuditorium
        {
            get => _selectedAuditorium;
            set { _selectedAuditorium = value; OnPropertyChanged(); RaiseCanExec(); }
        }

        public DateTime MinMonth { get; }
        public DateTime MaxMonth { get; }

        private DateTime _selectedMonth;
        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                var v = FirstOfMonth(value);
                if (_selectedMonth == v) return;
                _selectedMonth = v;
                OnPropertyChanged();

                if (!_suspendSync)
                {
                    _suspendSync = true;
                    SelectedYear = _selectedMonth.Year;
                    SelectedMonthNumber = _selectedMonth.Month;
                    _suspendSync = false;
                }

                RefreshMonth();
            }
        }

        // Måned/år pickers
        private int _selectedMonthNumber;
        public int SelectedMonthNumber
        {
            get => _selectedMonthNumber;
            set
            {
                if (_selectedMonthNumber == value) return;
                _selectedMonthNumber = value; OnPropertyChanged();
                if (_suspendSync) return;
                _suspendSync = true;
                SelectedMonth = new DateTime(SelectedYear, _selectedMonthNumber, 1);
                _suspendSync = false;
            }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear == value) return;
                _selectedYear = value; OnPropertyChanged();
                if (_suspendSync) return;
                _suspendSync = true;
                SelectedMonth = new DateTime(_selectedYear, SelectedMonthNumber, 1);
                _suspendSync = false;
            }
        }

        // Formularfelter
        private DateTime? _newDate;
        public DateTime? NewDate { get => _newDate; set { _newDate = value; OnPropertyChanged(); RaiseCanExec(); } }

        private string _newStartText = "";
        public string NewStartText { get => _newStartText; set { _newStartText = value; OnPropertyChanged(); RaiseCanExec(); } }

        private Movie? _selectedMovie;
        public Movie? SelectedMovie { get => _selectedMovie; set { _selectedMovie = value; OnPropertyChanged(); RaiseCanExec(); } }

        private ScreeningVM? _selectedScreening;
        public ScreeningVM? SelectedScreening { get => _selectedScreening; set { _selectedScreening = value; OnPropertyChanged(); RaiseCanExec(); } }

        private int _adsMinutes = 15;
        public int AdsMinutes { get => _adsMinutes; set { _adsMinutes = Math.Max(0, value); OnPropertyChanged(); } }

        private int _cleaningMinutes = 15;
        public int CleaningMinutes { get => _cleaningMinutes; set { _cleaningMinutes = Math.Max(0, value); OnPropertyChanged(); } }

        private string? _error;
        public string? ErrorMessage { get => _error; set { _error = value; OnPropertyChanged(); } }

        // -------- Commands --------
        public ICommand AddScreeningCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand EditScreeningCommand { get; }

        private bool CanAddScreening()
        {
            if (SelectedCinema == null) return false;
            if (SelectedAuditorium == null) return false;
            if (SelectedMovie == null) return false;
            if (!NewDate.HasValue) return false;
            return TimeSpan.TryParse(NewStartText, out _);
        }

        // Loader visninger for den valgte måned (for de valgte biografer)
        public void RefreshMonth()
        {
            try
            {
                Screenings.Clear();
                TimelineDays.Clear();

                // Prioritet: valgte filtre → ellers SelectedCinema → ingen
                var selectedIds = CinemaFilters.Where(f => f.IsSelected).Select(f => f.CinemaId).Distinct().ToList();
                if (selectedIds.Count == 0 && SelectedCinema != null)
                    selectedIds.Add(SelectedCinema.Id);

                // Opret timeline data
                var timelineScreenings = new List<TimelineScreeningVM>();

                foreach (var id in selectedIds)
                {
                    var items = _schedule.GetMonth(id, SelectedMonth.Year, SelectedMonth.Month);
                    var cinema = Cinemas.FirstOrDefault(c => c.Id == id);

                    foreach (var s in items)
                    {
                        s.Movie ??= _movieRepo.GetById(s.MovieId);
                        var aud = cinema?.Auditoriums.FirstOrDefault(a => a.Id == s.AuditoriumId)
                                  ?? new AuditoriumModel { Id = s.AuditoriumId, Name = $"Sal {s.AuditoriumId}" };

                        var screeningVM = new ScreeningVM(s.StartLocal, s.Movie!, aud)
                        {
                            Id = s.Id,
                            CinemaId = s.CinemaId,
                            AdsMinutes = s.AdsMinutes,
                            CleaningMinutes = s.CleaningMinutes,
                            CinemaName = cinema?.Name ?? string.Empty
                        };

                        Screenings.Add(screeningVM);

                        // Opret TimelineScreeningVM for timeline-visning
                        var timelineScreening = new TimelineScreeningVM(s.StartLocal, s.Movie!, aud)
                        {
                            Id = s.Id,
                            CinemaId = s.CinemaId,
                            AdsMinutes = s.AdsMinutes,
                            CleaningMinutes = s.CleaningMinutes,
                            CinemaName = cinema?.Name ?? string.Empty
                        };
                        timelineScreenings.Add(timelineScreening);
                    }
                }

                // Organiser timeline data efter dato og auditorium
                BuildTimelineDays(timelineScreenings);

                SortScreeningsByTime();
                RebuildCalendar();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fejl i RefreshMonth: {ex.Message}");
                // Lad ikke programmet crashe, bare vis tom liste
                Screenings.Clear();
                TimelineDays.Clear();
            }
        }

        // Loader film fra repository og opdaterer Movies collection
        public void LoadMovies()
        {
            try
            {
                var currentSelectedMovieId = SelectedMovie?.Id;
                
                Movies.Clear();
                var allMovies = _movieRepo.GetAll();
                foreach (var movie in allMovies)
                {
                    Movies.Add(movie);
                }
                
                // Gendan valgt film hvis den stadig findes
                if (currentSelectedMovieId.HasValue)
                {
                    SelectedMovie = Movies.FirstOrDefault(m => m.Id == currentSelectedMovieId.Value);
                }
                
                System.Diagnostics.Debug.WriteLine($"LoadMovies: Loaded {Movies.Count} movies");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fejl i LoadMovies: {ex.Message}");
                // Lad ikke programmet crashe, bare vis tom liste
                Movies.Clear();
            }
        }

        private void BuildTimelineDays(List<TimelineScreeningVM> screenings)
        {
            TimelineDays.Clear();

            if (!screenings.Any()) return;

            // Gruppér efter dato
            var daysInMonth = DateTime.DaysInMonth(SelectedMonth.Year, SelectedMonth.Month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(SelectedMonth.Year, SelectedMonth.Month, day);
                var dayScreenings = screenings.Where(s => s.Start.Date == date).ToList();

                if (dayScreenings.Any())
                {
                    var timelineDay = new TimelineDayVM(date);
                    
                    // Tilføj alle screenings til dagen
                    foreach (var screening in dayScreenings)
                    {
                        timelineDay.AddScreening(screening);
                    }

                    TimelineDays.Add(timelineDay);
                }
            }
        }

        // Byg venstre-dagsgrupper til højre panel
        public void RebuildCalendar()
        {
            try
            {
                DayGroups.Clear();

                var first = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
                var days = DateTime.DaysInMonth(SelectedMonth.Year, SelectedMonth.Month);

                for (int i = 0; i < days; i++)
                {
                    var d = first.AddDays(i).Date;
                    var items = Screenings.Where(s => s.Start.Date == d).ToList();
                    DayGroups.Add(new DayGroupVM(d, items));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fejl i RebuildCalendar: {ex.Message}");
                // Lad ikke programmet crashe, bare vis tom liste
                DayGroups.Clear();
            }
        }

        private void AddScreening()
        {
            try
            {
                ErrorMessage = null;

                if (!TimeSpan.TryParse(NewStartText, out var startTime))
                    throw new ArgumentException("Ugyldigt tidspunkt. Brug HH:MM, f.eks. 19:30.");

                if (!NewDate.HasValue) throw new InvalidOperationException("Vælg en dato.");

                var start = NewDate.Value.Date + startTime;

                if (start.Year != SelectedMonth.Year || start.Month != SelectedMonth.Month)
                    throw new InvalidOperationException("Dato ligger ikke i den valgte måned.");

                // Brug SchedulingService til at tilføje forestillingen til repository
                var screening = _schedule.AddScreening(
                    SelectedCinema!.Id,
                    SelectedAuditorium!.Id,
                    SelectedMovie!.Id,
                    start,
                    AdsMinutes,
                    CleaningMinutes
                );

                System.Diagnostics.Debug.WriteLine($"Added screening: {screening.Id} for {SelectedMovie.Title} at {start}");

                // Opdater UI ved at genindlæse data
                RefreshMonth();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error adding screening: {ex.Message}");
            }
        }

        private void RemoveSelected()
        {
            if (SelectedScreening is null) return;
            
            try
            {
                // Brug SchedulingService til at slette forestillingen fra repository
                _schedule.RemoveScreening(SelectedScreening.Id);
                
                System.Diagnostics.Debug.WriteLine($"Removed screening: {SelectedScreening.Id}");
                
                // Opdater UI ved at genindlæse data
                RefreshMonth();
                SelectedScreening = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing screening: {ex.Message}");
                ErrorMessage = $"Fejl ved sletning: {ex.Message}";
            }
        }

        private void EditScreening(TimelineScreeningVM screening)
        {
            try
            {
                var editViewModel = new EditScreeningViewModel(screening, _schedule, RefreshMonth);
                var editWindow = new EditScreeningWindow
                {
                    DataContext = editViewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening edit window: {ex.Message}");
                ErrorMessage = $"Fejl ved åbning af redigeringsvindue: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            NewDate = null;
            NewStartText = string.Empty;
            SelectedMovie = null;
            RaiseCanExec();
        }

        private void SortScreeningsByTime()
        {
            if (Screenings.Count < 2) return;
            var sorted = Screenings.OrderBy(s => s.Start).ToList();
            Screenings.Clear();
            foreach (var s in sorted) Screenings.Add(s);
        }

        private static void RaiseCanExec() => CommandManager.InvalidateRequerySuggested();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // Én visning (bruges i både grid og daggrupper)
    public sealed class ScreeningVM : INotifyPropertyChanged
    {
        public ScreeningVM(DateTime start, Movie movie, AuditoriumModel auditorium)
        {
            Start = start; Movie = movie; Auditorium = auditorium;
        }

        public int Id { get; set; }
        public int CinemaId { get; set; }

        private DateTime _start;
        public DateTime Start { get => _start; set { _start = value; OnPropertyChanged(); OnPropertyChanged(nameof(End)); } }

        public Movie Movie { get; }
        public AuditoriumModel Auditorium { get; }
        public string CinemaName { get; set; } = string.Empty;

        private int _adsMinutes;
        public int AdsMinutes { get => _adsMinutes; set { _adsMinutes = Math.Max(0, value); OnPropertyChanged(); OnPropertyChanged(nameof(End)); } }

        private int _cleaningMinutes;
        public int CleaningMinutes { get => _cleaningMinutes; set { _cleaningMinutes = Math.Max(0, value); OnPropertyChanged(); OnPropertyChanged(nameof(End)); } }

        public string MovieTitle => Movie.Title;
        public string AuditoriumName => Auditorium.Name;

        public int DurationWithExtras => Movie.DurationMin + AdsMinutes + CleaningMinutes;
        public DateTime End => Start.AddMinutes(DurationWithExtras);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
