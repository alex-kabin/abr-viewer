using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace AbrViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UnityContainer _container;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            _container = new UnityContainer();
            try {
                _container.LoadConfiguration();
                MainWindow = _container.Resolve<MainWindow>();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            MainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            _container.Dispose();
            base.OnExit(e);
        }
    }
}