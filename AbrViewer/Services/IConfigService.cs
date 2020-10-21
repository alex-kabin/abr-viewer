namespace AbrViewer.Services
{
    public interface IConfigService
    {
        Config CurrentConfig { get; }
        bool Save();
        void Dispose();
    }
}
