using System.Collections.Generic;
using System.IO.Abstractions;

namespace Badass.Model
{
    public class Settings
    {
        private readonly IFileSystem _fs;
        private string _configurationFile;
        private const string JsonExtension = ".codegen.json";

        public Settings(IFileSystem fs)
        {
            _fs = fs;
            ConfigurationFile = "codegen.json";
            ClientAppTypes = new List<ClientAppUIType>();
            GenerateSecurityPolicies = true;
            ExcludedSchemas = new List<string>();
            GenerateTestRepos = false;
        }

        public string ConfigurationFile
        {
            get { return _configurationFile; }
            set
            {
                _configurationFile = value;

                if (!_fs.File.Exists(_configurationFile))
                {
                    if (!_configurationFile.ToLowerInvariant().EndsWith(JsonExtension))
                    {
                        _configurationFile += JsonExtension;
                    }    
                }
            }
        }
        public string RootDirectory { get; set; }
        public int Verbosity { get; set; } 
        public bool ShowHelp { get; set; }
        public bool AddGeneratedOptionsToDatabase { get; set; }
        public string ApplicationName { get; set; }
        public string DataDirectory { get; set; }
        public WebUIType WebUIType { get; set; }
        public string TypeName { get; set; }
        public bool Debug { get; set; }
        public string ConnectionString { get; set; }
        public NewAppSettings NewAppSettings { get; set; }
        public bool DeleteGeneratedFiles { get; set; }  
        
        public string OpenApiUri { get; set; }

        public List<ClientAppUIType> ClientAppTypes { get; }
        
        public bool GenerateSecurityPolicies { get; set; }
        
        public List<string> ExcludedSchemas { get; }
        
        public bool GenerateTestRepos { get; set; }
            
        public string TestDataDirectory { get; set; }
        
        public int? TestDataSize { get; set; }
    }

    public class NewAppSettings 
    {
        public bool CreateNew { get; set; }
        public string BrandColour { get; set; }
        public string LogoFileName { get; set; }
        
        public string TemplateProjectDirectory { get; set; }
    }

    public enum WebUIType
    {
        MVC,
        React
    }

    public enum ClientAppUIType
    {
        Flutter
    }
}