using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Badass.Model;
using Badass.Templating;
using Serilog;

namespace Badass.ProjectGeneration
{
    public class NewProjectGenerator : ProjectGeneratorBase
    {
        private const string FixMeAppName = "FixMeAppName";
        private const string FixMeDefaultNamespace = "FixMeDefaultNamespace";
        private const string FixMeAppTitle = "FixMeAppTitle";

        public NewProjectGenerator(Settings settings, IFileSystem fileSystem) : base(settings, fileSystem)
        {
        }

        public bool Generate()
        {
            try
            {
                if (!_fileSystem.Directory.Exists(_settings.RootDirectory))
                {
                    Log.Warning("Root directory {DirectoryName} does not exist. It has been created.", _settings.RootDirectory);
                    _fileSystem.Directory.CreateDirectory(_settings.RootDirectory);
                }

                if (_fileSystem.Directory.GetFiles(_settings.RootDirectory).Length > 0)
                {
                    Log.Warning("Some files already exist in the root directory. Skipping new web project generation");
                    return true;
                }

                CopyBaseSolution();
                RenameFiles();
                FindAndReplaceInFiles();
                UpdateLaunchSettings();
                if (!string.IsNullOrEmpty(_settings.NewAppSettings.BrandColour))
                {
                    UpdateBrandColour();
                }

                if (!string.IsNullOrEmpty(_settings.NewAppSettings.LogoFileName))
                {
                    CopyLogoFile();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error creating new solution");
                return false;
            }
            Log.Information("Finished creating new Solution");
            return true;
        }

        private void FindAndReplaceInFiles()
        {
            Log.Information("Replacing File Contents");
            RecurseDirectory(_settings.RootDirectory, file =>
            {
                var contents = _fileSystem.File.ReadAllText(file.FullName);
                var touched = false;
                if (contents.Contains(FixMeAppName))
                {
                    contents = contents.Replace(FixMeAppName, _settings.ApplicationName);
                    touched = true;
                }

                if (contents.Contains(FixMeDefaultNamespace))
                {
                    contents = contents.Replace(FixMeDefaultNamespace, _settings.ApplicationName);
                    touched = true;
                }

                if (contents.Contains(FixMeAppTitle))
                {
                    contents = contents.Replace(FixMeAppTitle, Util.HumanizeName(_settings.ApplicationName));
                    touched = true;
                }

                if (touched)
                {
                    _fileSystem.File.WriteAllText(file.FullName, contents);
                }

            }, null);
        }

        private void UpdateLaunchSettings()
        {
            // TODO - replace some port numbers
        }

        private void RenameFiles()
        {
            Log.Information("Renaming Files");
            RecurseDirectory(_settings.RootDirectory, file =>
            {
                if (file.Name.Contains(FixMeAppName))
                {
                    var newFileName = _fileSystem.Path.Combine(file.DirectoryName, file.Name.Replace(FixMeAppName, _settings.ApplicationName));
                    file.MoveTo(newFileName);
                }
            }, null);
        }

        private void CopyBaseSolution()
        {
            if (string.IsNullOrEmpty(_settings.NewAppSettings.TemplateProjectDirectory))
            {
                Log.Fatal("Unable to copy base project. Template Project Directory is not specified. This can be specified on the command-line via the -tmplt command-line switch, or via configuration with the template-dir config value.");
                throw new InvalidOperationException("Template project directory not specified");
            }
            
            Log.Information("Copying Base Solution");
            var source = _settings.NewAppSettings.TemplateProjectDirectory; 
            CopyDirectory(source, _settings.RootDirectory);
        }

        private void UpdateBrandColour()
        {
            const string brandColourCss = "black; /* brand colour */"; // <= we find and replace on this           

            RecurseDirectory(_settings.RootDirectory, file =>
            {
                var contents = _fileSystem.File.ReadAllText(file.FullName);
                if (contents.Contains(brandColourCss))
                {
                    contents = contents.Replace(brandColourCss, _settings.NewAppSettings.BrandColour + "; /*brand colour*/");
                    _fileSystem.File.WriteAllText(file.FullName, contents);
                }
            }, null);
        }

        private void CopyLogoFile()
        {
            if (!_fileSystem.File.Exists(_settings.NewAppSettings.LogoFileName))
            {
                Log.Error($"Logo file {_settings.NewAppSettings.LogoFileName} does not exist.");
                return;
            }

            //find current logo.svg files
            var existingLogoFiles = _fileSystem.Directory.GetFiles(_settings.RootDirectory, "logo.svg", SearchOption.AllDirectories);
            if (existingLogoFiles.Any())
            {
                foreach (var logoFile in existingLogoFiles)
                {
                    _fileSystem.File.Copy(_settings.NewAppSettings.LogoFileName, logoFile, true);
                }
            }
        }
    }
}
