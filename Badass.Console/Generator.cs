﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Badass.OpenApi;
using Badass.ProjectGeneration;
using Badass.Model;
using Badass.Templating;
using Badass.Templating.Classes;
using Badass.Templating.DatabaseFunctions;
using Badass.Templating.MvcViews;
using Badass.Templating.ReactClient;
using Badass.Templating.TestData;
using Serilog;

namespace Badass.Console
{
    public class Generator
    {
        private readonly IFileSystem _fs;
        private readonly Settings _settings;
        private readonly FileWriter _fileWriter;

        private const string DbScriptsRelativePath = ".\\Database\\Domain\\";

        public Generator(IFileSystem fileSystem, Settings settings, FileWriter fileWriter)
        {
            _fs = fileSystem;
            _settings = settings;
            _fileWriter = fileWriter;
        }

        private string DatabaseScriptsFolder => _fs.Path.Combine(CSharpDataAccessFolder, DbScriptsRelativePath);

        private string CSharpDataAccessFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(_settings.DataDirectory))
                {
                    return _fs.Path.Combine(_settings.RootDirectory, _settings.DataDirectory);
                }
                
                return _fs.Path.Combine(_settings.RootDirectory, "Data");
            }
        }
        
        private string CSharpDataAccessTestFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(_settings.TestDataDirectory))
                {
                    return _fs.Path.Combine(_settings.RootDirectory, _settings.TestDataDirectory);
                }
                
                return _fs.Path.Combine(_settings.RootDirectory, "Data\\Test");
            }
        }

        public void Generate(ITypeProvider typeProvider)
        {
            Log.Information($"Starting Code Generation");

            if (_settings.NewAppSettings?.CreateNew == true)
            {
                var newProjectGen = new NewProjectGenerator(_settings, _fs);
                var newProjectResult = newProjectGen.Generate();
                if (newProjectResult == false)
                {
                    Log.Error("Error generating new project - exiting");
                    return;
                }
            }

            if (_settings.AddGeneratedOptionsToDatabase)
            {
                var sb = new StringBuilder();
                sb.AppendLine("-- generated by a tool");
                typeProvider.DropGeneratedOperations(_settings, sb);
                typeProvider.DropGeneratedTypes(_settings, sb);
                var dropFile = new CodeFile() {Contents = sb.ToString(), Name = "drop_generated.sql"};
                _fileWriter.ApplyCodeFiles(new List<CodeFile>{dropFile}, DatabaseScriptsFolder);
                Log.Information("Finishing dropping generated operations");
            }

            var domain = typeProvider.GetDomain(_settings);
            Log.Information("Finished building domain");

            domain.DefaultNamespace = _settings.ApplicationName;

            SetupRootFolder();

            GenerateDbFunctions(domain, _settings.AddGeneratedOptionsToDatabase, typeProvider);
            Log.Information("Finished generating db functions");

            typeProvider.GetOperations(domain);
            Log.Information("Finished adding operations to domain");

            SanityCheckDomain(domain);

            GenerateClasses(domain);
            Log.Information("Finished generating classes");

            GenerateRepositories(domain);
            Log.Information("Finished generating repositories");

            if (_settings.GenerateTestRepos && !string.IsNullOrEmpty(_settings.TestDataDirectory))
            {
                // this is very much a work-in-progress
                GenerateTestRepositories(domain);
                Log.Information("Finished generating test repositories");
            }
            
            if (domain.ResultTypes.Any(rt => !rt.Ignore))
            {
                GenerateReturnTypes(domain);
                Log.Information("Finished generating return types");
            }
            
            if (_settings.WebUIType == WebUIType.MVC)
            {
                GenerateControllers(domain);
                GenerateViews(domain);
                GenerateViewModels(domain);
                Log.Information("Finished generating MVC UI");
            }

            if (_settings.WebUIType == WebUIType.React)
            {
                GenerateWebApi(domain);
                GenerateWebApiModels(domain);
                
                if (!string.IsNullOrEmpty(_settings.OpenApiUri))
                {
                    var openApiDocProvider = new OpenApiDocumentProvider(_fs, _settings);
                    var openApiDomainProvider = new OpenApiDomainProvider(openApiDocProvider);
                    openApiDomainProvider.AugmentDomainFromOpenApi(domain);
                }
                
                GenerateClientServiceProxy(domain);
                GenerateClientApiModels(domain);
                GenerateClientPages(domain);
                Log.Information("Finished generating react UI");
            }

            if (_settings.ClientAppTypes.Contains(ClientAppUIType.Flutter))
            {
                var flutterGen = new Flutter.Generator(_fs, _settings, _fileWriter);
                flutterGen.Generate(domain);
            }

            if (_settings.TestDataSize != null && _settings.TestDataSize > 0)
            {
                var testDataGen = new TestDataGenerator();
                var testData = testDataGen.Generate(domain);
                foreach (var testDataFile in testData)
                {
                    typeProvider.AddTestData(testDataFile.Contents);
                }
            }
            
            Log.Information("Finished Code Generation");
        }

        private void FilterDomainToSingleType(Domain domain)
        {
            var selectedType = domain.Types.FirstOrDefault(t => t.Name == _settings.TypeName);
            if (selectedType == null)
            {
                throw new InvalidOperationException("Unable to find the type you specified to operate on " +
                                                    _settings.TypeName);
            }

            domain.Types.Clear();
            domain.Types.Add(selectedType);
            var ops = domain.Operations.Where(o =>
                o.Attributes?.applicationtype == selectedType.Name || o.Returns.SimpleReturnType == selectedType);
            domain.Operations.Clear();
            domain.Operations.AddRange(ops);
        }

        private void SetupRootFolder()
        {
            if (!_fs.Directory.Exists(_settings.RootDirectory))
            {
                _fs.Directory.CreateDirectory(_settings.RootDirectory);
            }
        }

        private void GenerateClasses(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateDomain(domain);
            const string DomainObjectFolderName = "Domain";
            var dir = _fs.Path.Combine(CSharpDataAccessFolder, DomainObjectFolderName);
            _fileWriter.ApplyCodeFiles(files, dir);
        }

        private void GenerateReturnTypes(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateReturnTypes(domain);
            const string folderName = "Model";
            var dir = _fs.Path.Combine(CSharpDataAccessFolder, folderName);
            _fileWriter.ApplyCodeFiles(files, dir);
        }

        private void GenerateDbFunctions(Domain domain, bool addGeneratedOperationsToDatabase, ITypeProvider typeProvider)
        {
            var generator = new DbFunctionGenerator();
            var files = generator.Generate(domain, _settings);

            _fileWriter.ApplyCodeFiles(files, DatabaseScriptsFolder, file =>
            {
                if (addGeneratedOperationsToDatabase)
                {
                    typeProvider.AddGeneratedOperation(file.Contents);
                }
            });
        }

        private void GenerateRepositories(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateRepositories(domain);

            var infra = generator.GenerateRepositoryInfrastructure(domain);
            files.AddRange(infra);

            const string RepoFolderName = "Repository";
            var path = _fs.Path.Combine(CSharpDataAccessFolder, RepoFolderName);

            _fileWriter.ApplyCodeFiles(files, path);
        }
        
        private void GenerateTestRepositories(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateTestRepositories(domain);
            
            const string RepoFolderName = "Repository";
            var path = _fs.Path.Combine(CSharpDataAccessTestFolder, RepoFolderName);

            _fileWriter.ApplyCodeFiles(files, path);
        }

        private void GenerateControllers(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateControllers(domain);

            _fileWriter.ApplyCodeFiles(files, "Controllers");
        }

        private void GenerateWebApi(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateWebApiControllers(domain);
            _fileWriter.ApplyCodeFiles(files, "Controllers");
        }

        private void GenerateWebApiModels(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateWebApiModels(domain);
            _fileWriter.ApplyCodeFiles(files, "Models");
        }

        private void GenerateViews(Domain domain)
        {
            var generator = new ViewGenerator();
            var files = generator.Generate(domain);

            if (files.Any())
            {
                foreach (var codeFile in files)
                {
                    var folder = _fs.Path.Combine(_settings.RootDirectory, codeFile.RelativePath);
                    if (!_fs.Directory.Exists(folder))
                    {
                        _fs.Directory.CreateDirectory(folder);
                    }

                    _fs.File.WriteAllText(_fs.Path.Combine(folder, codeFile.Name), codeFile.Contents);
                }
            }
        }

        private void GenerateViewModels(Domain domain)
        {
            var generator = new ClassGenerator();
            var files = generator.GenerateEditModels(domain);

            _fileWriter.ApplyCodeFiles(files, "Models");
        }

        public const string ReactComponentDirectory = @"ClientApp\src\components\";

        private void GenerateClientServiceProxy(Domain domain)
        {
            var clientGenerator = new ReactClientGenerator();
            var files = clientGenerator.Generate(domain);
            _fileWriter.ApplyCodeFiles(files, ReactComponentDirectory);
        }

        private void GenerateClientApiModels(Domain domain)
        {
            var clientGenerator = new ReactClientGenerator();
            var files = clientGenerator.GenerateClientModels(domain);
            _fileWriter.ApplyCodeFiles(files, ReactComponentDirectory);
        }

        private void GenerateClientPages(Domain domain)
        {
            var generator = new ReactClientGenerator();
            var listPages = generator.GenerateComponents(domain);
            _fileWriter.ApplyCodeFiles(listPages, ReactComponentDirectory);
        }

        private void SanityCheckDomain(Domain domain)
        {
            if (domain.Types.Count == 0)
            {
                Log.Error("There are no types in domain");
            }

            if (domain.Operations.Count == 0)
            {
                Log.Error("There are no operations in domain");
            }

            foreach (var applicationType in domain.FilteredTypes)
            {
                if (applicationType.Fields.Count == 0)
                {
                    Log.Warning($"Type {applicationType.Name} has no fields.");
                }

                if (string.IsNullOrEmpty(applicationType.Namespace))
                {
                    Log.Warning($"Type {applicationType.Name} has no namespace.");
                }

                if (!applicationType.Ignore)
                {
                    var operations = domain.Operations.Where(o => o.Returns.SimpleReturnType == applicationType);
                    if (operations.Count() == 0)
                    {
                        Log.Warning($"Type {applicationType.Name} is not returned by any operations");
                    }
                }
            }

            foreach (var resultType in domain.ResultTypes)
            {
                if (resultType.Fields.Count == 0)
                {
                    Log.Warning($"Result Type {resultType.Name} has no fields");
                }

                var operations = domain.Operations.Where(o => o.Returns.SimpleReturnType == resultType);
                if (operations.Count() == 0)
                {
                    Log.Warning($"Result Type {resultType.Name} is not returned by any operations");
                }
            }

            foreach (var op in domain.Operations)
            {
                if (op.Returns == null)
                {
                    Log.Warning($"Operation {op.Name} does not return anything.");
                }
            }
        }
    }
}
