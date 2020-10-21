using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AbrViewer.Services;
using AbrViewer.Support;
using AK.Abr;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace AbrViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const string FILE_FILTER = "*.abr";

        private class AbrReadOperation : IDisposable
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
                _viewModel.StatusText = "Loading brushes...";
                _reader.ReadAsync(_cancellation.Token)
                       .ContinueWith(Finished, TaskScheduler.FromCurrentSynchronizationContext());
            }

            private void Finished(Task task) {
                _reader.BrushImageReady -= HandleBrushImageReady;
                _cancellation.Dispose();

                if (task.IsFaulted) {
                    _viewModel.StatusText = "Error";
                    _viewModel._dialogService.ShowDialog(
                        "Error",
                        task.Exception?.InnerException?.ToString() ?? "Internal error",
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

            public void Dispose() {
                try {
                    _cancellation.Cancel();
                }
                catch (ObjectDisposedException) {
                    // ignore
                }
            }
        }

        private readonly IAbrReaderFactory _readerFactory;
        private readonly IConfigService _configService;
        private readonly IDialogService _dialogService;

        private AbrReadOperation _currentOperation;
        private ObservableCollection<FileInfo> _files = new ObservableCollection<FileInfo>();
        private ObservableCollection<BitmapSource> _images = new ObservableCollection<BitmapSource>();

        private FileSystemWatcher _folderWatcher = null;

        public MainViewModel(IAbrReaderFactory readerFactory, IConfigService configService, IDialogService dialogService) {
            _readerFactory = readerFactory;
            _configService = configService;
            _dialogService = dialogService;

            BrushFiles = new ListCollectionView(_files);
            BrushFiles.SortDescriptions.Add(new SortDescription(nameof(FileInfo.Name), ListSortDirection.Ascending));
            BrushFiles.CurrentChanged += OnBrushFileSelected;

            BrushImages = new ListCollectionView(_images);

            OpenFolderCommand = new RelayCommand(OpenFolder);
            ExitCommand = new RelayCommand(Exit);
            SaveImageCommand = new RelayCommand<BitmapSource>(SaveImage);
            AboutCommand = new RelayCommand(About);
            PreviewCommand = new RelayCommand<BitmapSource>(Preview);
            DeleteFileCommand = new RelayCommand<FileInfo>(DeleteFile);
            EscapeCommand = new RelayCommand(Escape);

            ProcessCommandLineArgs();
        }

        private async void DeleteFile(FileInfo file) {
            if (file == null || !file.Exists) {
                return;
            }

            if (await _dialogService.ShowDialog(
                "Attention",
                $"Do you really want to delete file '{file.Name}'?",
                MessageBoxImage.Question,
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try {
                    file.Delete();
                }
                catch (Exception ex) {
                    await _dialogService.ShowDialog(
                        "Error",
                        $"Error deleting file '{file.Name}':\n{ex}",
                        MessageBoxImage.Error
                    );
                }
            }
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
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog() {
                Title = "Brush image export",
                Filter = "PNG|*.png",
                DefaultExt = "png"
            };

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
            Close?.Invoke(this, EventArgs.Empty);
        }

        private void Escape() {
            if (Config.ExitOnEsc) {
                Exit();
            }
        }

        private void OnBrushFileSelected(object sender, EventArgs e) {
            var selectedFile = BrushFiles.CurrentItem as FileInfo;
            if (selectedFile == null)
                return;

            var source = new FileAbrSource(selectedFile.FullName);

            _currentOperation?.Dispose();
            _currentOperation = new AbrReadOperation(this, source);
        }

        private void OpenFolder() {
            var dialog = new System.Windows.Forms.FolderBrowserDialog() {
                ShowNewFolderButton = false,
                Description = "Choose folder with brushes",
                SelectedPath = CurrentFolder
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _images.Clear();
                ListFiles(dialog.SelectedPath);
            }
        }
        
        private void WatchFiles(string folderPath) {
            void OnChanged(object obj, FileSystemEventArgs e) {
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Deleted:
                        _files.RemoveIf(f => e.Name.Equals(f.Name));
                        break;
                    case WatcherChangeTypes.Created:
                        _files.Add(new FileInfo(e.FullPath));
                        break;
                    case WatcherChangeTypes.Renamed:
                        _files.RemoveIf(f => ((RenamedEventArgs)e).OldName.Equals(f.Name));
                        var newFile = new FileInfo(e.FullPath);
                        _files.Add(newFile);
                        BrushFiles.MoveCurrentTo(newFile);
                        break;
                }
            }

            if (_folderWatcher == null) {
                _folderWatcher = new FileSystemWatcher(folderPath) {
                    Filter = FILE_FILTER,
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName,
                    SynchronizingObject = new SynchronizeInvokeAdapter()
                };
                _folderWatcher.Changed += OnChanged;
                _folderWatcher.Renamed += OnChanged;
                _folderWatcher.Deleted += OnChanged;
                _folderWatcher.Created += OnChanged;
                _folderWatcher.EnableRaisingEvents = true;
            }
            else {
                _folderWatcher.Path = folderPath;
            }
        }
        
        private void ListFiles(string folderPath) {
            WatchFiles(folderPath);

            _files.Clear();
            var directory = new DirectoryInfo(folderPath);
            foreach (var brushFile in directory.EnumerateFiles(FILE_FILTER)) {
                _files.Add(brushFile);
            }

            StatusText = $"Total files: {_files.Count}";
            CurrentFolder = directory.FullName;
        }

        public override void Cleanup() {
            base.Cleanup();
            if (_folderWatcher != null) {
                _folderWatcher.EnableRaisingEvents = false;
                _folderWatcher.Dispose();
                _folderWatcher = null;
            }
        }

        public ListCollectionView BrushFiles { get; }
        public ListCollectionView BrushImages { get; }

        public string StatusText { get; set; }
        public string CurrentFolder { get; set; }

        public Config Config => _configService.CurrentConfig;
        public string Title => ApplicationInfo.ProductName;

        public ICommand OpenFolderCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand EscapeCommand { get; }

        public event EventHandler Close;
    }
}