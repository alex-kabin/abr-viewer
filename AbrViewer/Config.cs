using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace AbrViewer
{
    [DataContract]
    public class Config : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember] public bool ExitOnEsc { get; set; } = true;

        [DataMember] public int ThumbnailSize { get; set; } = 120;

        [DataMember] public int WindowWidth { get; set; } = 800;

        [DataMember] public int WindowHeight { get; set; } = 600;

        [DataMember] public WindowState WindowState { get; set; } = WindowState.Normal;
    }
}
