using System;
using System.Windows;
using AbrViewer.Services;
using AbrViewer.ViewModels;
using GalaSoft.MvvmLight;
using Unity;

namespace AbrViewer
{
	public partial class MainWindow : Window, IRootWindow
	{
        public MainWindow() {
			InitializeComponent();
		}
        
		[Dependency]
		public MainViewModel ViewModel {
            set {
                DataContext = value;
                value.Close += ViewModel_Close;
            }
        }

        private void ViewModel_Close(object sender, EventArgs e)
        {
            (DataContext as ICleanup)?.Cleanup();
            Close();
        }
    }
}
