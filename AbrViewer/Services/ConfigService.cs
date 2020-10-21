using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace AbrViewer.Services
{
    public class ConfigService : IDisposable, IConfigService
    {
        private readonly string _configFilePath;
        private readonly DataContractJsonSerializer _serializer;

        private volatile bool dirty = false;
        public Config CurrentConfig { get; private set; }

        public ConfigService(string configFileName) {
            _configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), configFileName);
            _serializer = new DataContractJsonSerializer(typeof(Config));
            LoadConfig();
        }

        private void LoadConfig() {
            try {
                using (FileStream configFileStream = new FileStream(_configFilePath, FileMode.Open)) {
                    CurrentConfig = (Config) (_serializer.ReadObject(configFileStream));
                }
            }
            catch (Exception ex) {
                CurrentConfig = new Config();
            }
            CurrentConfig.PropertyChanged += CurrentConfig_PropertyChanged;
        }

        private void CurrentConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            dirty = true;
        }

        private void SaveConfig() {
            using (FileStream configFileStream = new FileStream(_configFilePath, FileMode.Create)) {
                _serializer.WriteObject(configFileStream, CurrentConfig);
            }
        }
        
        public bool Save() {
            if (!dirty)
                return false;

            try {
                SaveConfig();
                dirty = false;
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public void Dispose() {
            CurrentConfig.PropertyChanged -= CurrentConfig_PropertyChanged;
            Save();
        }
    }
}
