using System.Windows;
using The_movie_egen.UI.ViewModel;

namespace The_movie_egen.UI.View;

public partial class EditAuditoriumWindow : Window
{
    public EditAuditoriumWindow(EditAuditoriumViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
