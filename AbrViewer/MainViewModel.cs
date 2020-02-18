using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AK.Abr;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MessageBox = System.Windows.MessageBox;

namespace AbrViewer
{
    public class MainViewModel : ViewModelBase
    {
        private class AbrReadOperation
        {
            private readonly IAbrReader _reader;
            private readonly MainViewModel _viewModel;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

            public AbrReadOperation(MainViewModel viewModel, IAbrSource source) {
                _viewModel = viewModel;

                _reader = _viewModel._readerFactory.GetReader(source);
                _reader.BrushImageReady += HandleBrushImageReady;

                Start();
            }

            private void Start() {
                _viewModel._images.Clear();
                _reader.ReadAsync(_cancellation.Token)
                       .ContinueWith(Finished, TaskScheduler.FromCurrentSynchronizationContext());
                _viewModel.StatusText = "Loading brushes...";
            }

            private void Finished(Task task) {
                _reader.BrushImageReady -= HandleBrushImageReady;
                if (task.IsFaulted) {
                    _viewModel.StatusText = "Error";
                    MessageBox.Show(App.Current.MainWindow, 
                                    task.Exception?.InnerException?.ToString() ?? "Internal error", 
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error
                    );
                }
                else if (task.IsCompleted) {
                    _viewModel.StatusText = "OK";
                }
            }

            private void HandleBrushImageReady(object sender, BrushImageReadyEventArgs e) {
                if (_cancellation.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.BeginInvoke(
                    new Action(() => _viewModel._images.Add(e.Bitmap))
                );
            }

            public void Cancel() {
                _cancellation.Cancel();
            }
        }

        private readonly IAbrReaderFactory _readerFactory;

        private AbrReadOperation _currentOperation;
        private ObservableCollection<FileInfo> _files = new ObservableCollection<FileInfo>();
        private ObservableCollection<BitmapSource> _images = new ObservableCollection<BitmapSource>();

        public MainViewModel(IAbrReaderFactory readerFactory) {
            _readerFactory = readerFactory;

            BrushFiles = new ListCollectionView(_files);
            BrushFiles.CurrentChanged += OnBrushFileSelected;

            BrushImages = new ListCollectionView(_images);

            OpenFolderCommand = new RelayCommand(OpenFolder);
            ExitCommand = new RelayCommand(Exit);
            SaveImageCommand = new RelayCommand<BitmapSource>(SaveImage);
            AboutCommand = new RelayCommand(About);
            PreviewCommand = new RelayCommand<BitmapSource>(Preview);

            ProcessCommandLineArgs();
        }

        private void Preview(BitmapSource obj) {
            var previewWindow = new BrushPreviewWindow() {
                DataContext = obj,
                Owner = App.Current.MainWindow
            };
            previewWindow.ShowDialog();
        }

        private void About() {
            var aboutWindow = new AboutWindow() {
                Owner = App.Current.MainWindow,
                DataContext = new { ConfigInfo = "Factory=" + _readerFactory.ToString() }
            };
            aboutWindow.ShowDialog();
        }

        private void SaveImage(BitmapSource bmp) {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
                    { Title = "Brush image export", Filter = "PNG|*.png", DefaultExt = "png" };
            if (sfd.ShowDialog() == true) {
                var imageFilePath = sfd.FileName;
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using (var fs = File.OpenWrite(imageFilePath))
                    encoder.Save(fs);
            }
        }

        private void ProcessCommandLineArgs() {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                var dirOrFile = args[1];
                if (Directory.Exists(dirOrFile)) {
                    ListFiles(dirOrFile);
                }
                else if (File.Exists(dirOrFile)) {
                    var dir = Path.GetDirectoryName(dirOrFile);
                    ListFiles(dir);
                    var fileName = Path.GetFileName(dirOrFile);
                    BrushFiles.MoveCurrentTo(_files.FirstOrDefault(f => f.Name == fileName));
                }
            }
        }

        private void Exit() {
            System.Windows.Application.Current.Shutdown();
        }

        private void OnBrushFileSelected(object sender, EventArgs e) {
            var selectedFile = BrushFiles.CurrentItem as FileInfo;
            if (selectedFile == null)
                return;

            var source = new FileAbrSource(selectedFile.FullName);

            _currentOperation?.Cancel();

            _currentOperation = new AbrReadOperation(this, source);
        }

        private void OpenFolder() {
            var dialog = new FolderBrowserDialog() {
                ShowNewFolderButton = false,
                Description = "Choose folder with brushes",
                SelectedPath = CurrentFolder
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _images.Clear();
                ListFiles(dialog.SelectedPath);
            }
        }

        private void ListFiles(string folderPath) {
            _files.Clear();

            var directory = new DirectoryInfo(folderPath);
            foreach (var brushFile in directory.EnumerateFiles("*.abr")) {
                _files.Add(brushFile);
            }

            StatusText = $"Total files: {_files.Count}";
            CurrentFolder = directory.FullName;
        }

        public ListCollectionView BrushFiles { get; }
        public ListCollectionView BrushImages { get; }

        private string _statusText;

        public string StatusText {
            get => _statusText;
            set {
                _statusText = value;
                RaisePropertyChanged(nameof(StatusText));
            }
        }

        private string _currentFolder;

        public string CurrentFolder {
            get => _currentFolder;
            set {
                _currentFolder = value;
                RaisePropertyChanged(nameof(CurrentFolder));
            }
        }

        public string Title => ApplicationInfo.ProductName;

        public ICommand OpenFolderCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand AboutCommand { get; }
    }
}