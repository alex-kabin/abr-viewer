using System.Threading.Tasks;
using System.Windows;
using AbrViewer.Support;
using GalaSoft.MvvmLight.Threading;
using Unity;

namespace AbrViewer.Services
{
    public class DialogService : IDialogService
    {
        [Dependency]
        public Lazy<IRootWindow> RootWindow {
            get; set;
        }

        public DialogService() { }

        public Task<MessageBoxResult> ShowDialog(
            string title,
            string message,
            MessageBoxImage image = MessageBoxImage.Information,
            MessageBoxButton buttons = MessageBoxButton.OK
        ) {
            TaskCompletionSource<MessageBoxResult> tcs = new TaskCompletionSource<MessageBoxResult>();
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => {
                    tcs.SetResult(
                        MessageBox.Show(
                            RootWindow.As<Window>(), 
                            message, 
                            RootWindow != null ? $"{RootWindow.Value.Title} : {title}" : title, 
                            buttons, 
                            image
                        )
                    );
                }
            );
            return tcs.Task;
        }
    }
}
