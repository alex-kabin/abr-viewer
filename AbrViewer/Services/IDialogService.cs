using System.Threading.Tasks;
using System.Windows;

namespace AbrViewer.Services
{
    public interface IDialogService
    {
        Task<MessageBoxResult> ShowDialog(
            string title,
            string message,
            MessageBoxImage image = MessageBoxImage.Information,
            MessageBoxButton buttons = MessageBoxButton.OK
        );
    }
}
