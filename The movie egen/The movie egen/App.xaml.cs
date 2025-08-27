using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace The_movie_egen
{
    public partial class App : Application
    {
        public App()
        {
            // Log at app'en starter
            System.Diagnostics.Debug.WriteLine("App constructor started");
            
            // Globale "airbags" for at fange alle exceptions
            DispatcherUnhandledException += (s, ex) =>
            {
                System.Diagnostics.Debug.WriteLine($"DispatcherUnhandledException: {ex.Exception}");
                // Ikke MessageBox.Show her - kan forårsage uendelig loop
                ex.Handled = true;   // Så du kan se vinduet i stedet for at app'en lukker
            };
            
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                System.Diagnostics.Debug.WriteLine($"UnhandledException: {ex.ExceptionObject}");
                // Ikke MessageBox.Show her - kan forårsage uendelig loop
            };
            
            TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                System.Diagnostics.Debug.WriteLine($"UnobservedTaskException: {ex.Exception}");
                ex.SetObserved();
            };
            
            System.Diagnostics.Debug.WriteLine("App constructor completed");
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OnStartup started");
                
                var win = new MainWindow();
                System.Diagnostics.Debug.WriteLine("MainWindow created successfully");
                
                MainWindow = win;
                System.Diagnostics.Debug.WriteLine("MainWindow assigned to App.MainWindow");
                
                win.Show();
                System.Diagnostics.Debug.WriteLine("MainWindow.Show() called successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in OnStartup: {ex}");
                // Kun log til debug, ikke MessageBox
                Shutdown(-1);
            }
        }
    }
}
