using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using The_movie_egen.Model.Cinemas;
using The_movie_egen.UI.ViewModel;

namespace The_movie_egen.UI.View
{
    /// <summary>
    /// Interaction logic for CinemasView.xaml
    /// </summary>
    public partial class CinemasView : UserControl
    {
        public CinemasView()
        {
            InitializeComponent();
        }

        private void CinemasView_Loaded(object sender, RoutedEventArgs e)
        {
            // View loaded successfully
        }

        private void Auditorium_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Auditorium auditorium)
            {
                if (DataContext is CinemasViewModel viewModel)
                {
                    // Find the cinema that contains this auditorium
                    var cinema = viewModel.Cinemas.FirstOrDefault(c => c.Auditoriums.Contains(auditorium));
                    if (cinema != null)
                    {
                        viewModel.SelectedCinema = cinema;
                        viewModel.SelectedAuditorium = auditorium;
                    }
                }
            }
        }
    }
}
