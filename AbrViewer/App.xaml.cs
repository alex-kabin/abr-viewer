using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using AbrViewer.Services;
using AbrViewer.Support;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.Unity.Configuration;
using Unity;
using Unity.Lifetime;

namespace AbrViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IUnityContainer _container;

        private void InitializeContainer() {
            _container = new UnityContainer()
                         .EnableDiagnostic()
                         .EnableLazy()
                         .LoadConfiguration()
                         .RegisterSingleton(typeof(IDialogService), typeof(DialogService))
                         .RegisterSingleton(typeof(IRootWindow), typeof(MainWindow));
            _container.BuildUp(this);
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            DispatcherHelper.Initialize();
            try {
                InitializeContainer();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
            MainWindow?.Show();
        }

        [Dependency]
        public IRootWindow RootWindow {
            set => MainWindow = (Window)value;
        }

        protected override void OnExit(ExitEventArgs e) {
            _container.Dispose();
            base.OnExit(e);
        }
    }
}