using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest
{
    public class FilePathSettings
    {
        private const string applicationFolderName = "MakularTest";
        private const string settingsFolderName = "Settings";

        private const string csvDataFileName = "MakulaData.csv";
        private const string windowsSettingsFileName = "windowsSettings.xml";
        private const string appSettingsFileName = "appsettings.xml";

        private readonly string _applicationFolderPath;
        private readonly string _settingFolderPath;        

        private static FilePathSettings _instance;

        public string WindowsSettingsFilePath { get; }

        public string CSVDataFilePath { get;  }

        public string AppSettingsFilePath { get; }

        public static FilePathSettings Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new FilePathSettings();
                }

                return _instance; 
            }        
        }

        FilePathSettings()
        {
            _applicationFolderPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationFolderName);
            _settingFolderPath = Path.Combine(_applicationFolderPath, settingsFolderName);

            if (!Directory.Exists(_applicationFolderPath))
                Directory.CreateDirectory(_applicationFolderPath);

            if (!Directory.Exists(_settingFolderPath))
                Directory.CreateDirectory(_settingFolderPath);

            WindowsSettingsFilePath = Path.Combine(_settingFolderPath, windowsSettingsFileName);
            AppSettingsFilePath = Path.Combine(_settingFolderPath, appSettingsFileName);
            CSVDataFilePath = Path.Combine(_applicationFolderPath, csvDataFileName);
        }

        

    }
}
