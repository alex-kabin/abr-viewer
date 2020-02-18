using System.Windows;
using System.Windows.Input;

namespace AbrViewer
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Close();
        }
    }
}
