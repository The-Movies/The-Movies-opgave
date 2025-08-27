using System;
using System.Collections.ObjectModel;
using System.Windows;
using The_movie_egen.Data;
using The_movie_egen.Data.Json;
using The_movie_egen.Model.Cinemas;
using The_movie_egen.Model.Repositories;       // IMovieRepository, IScreeningRepository
using The_movie_egen.Services;
using The_movie_egen.UI.ViewModel;

namespace The_movie_egen
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Opret delte repository-instanser
                IMovieRepository movieRepo = new JsonMovieRepository("movies.json");
                IScreeningRepository screeningRepo = new The_movie_egen.Data.Json.JsonScreeningRepository();
                var cinemaRepo = new JsonCinemaRepository("cinemas.json");
                
                // Sikr at der er seed data
                cinemaRepo.EnsureSeeded();
                
                // Opret delt SchedulingService
                var schedule = new SchedulingService(movieRepo, screeningRepo);

                // Opret AuditoriumService
                var auditoriumService = new AuditoriumService(cinemaRepo, screeningRepo);

                // Root-VM med navigation - brug delte repositories
                DataContext = new ShellViewModel(new MovieRegistry(movieRepo), movieRepo, cinemaRepo, schedule, auditoriumService);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in MainWindow constructor: {ex}");
                Application.Current.Shutdown(-1);
            }
        }
    }
}
