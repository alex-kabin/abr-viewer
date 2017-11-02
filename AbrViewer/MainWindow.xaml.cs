using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AK.Abr;
using AbrViewer.Properties;
using Microsoft.Practices.Unity;

namespace AbrViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			if (Settings.Default.ExitOnEsc)
			{
				this.KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
			}
		}

		void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
				Close();
		}

		[Dependency]
		public MainViewModel ViewModel
		{
			set { DataContext = value; }
		}
	}
}
