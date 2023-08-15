// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.ToolPackage;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ToolPackage;
using Microsoft.Extensions.EnvironmentAbstractions;
using NuGet.Versioning;

namespace Microsoft.DotNet.Tools.Tool.Install
{
    internal class ToolInstallLocalInstaller
    {
        private readonly ParseResult _parseResult;
        public string TargetFrameworkToInstall { get; private set; }

        private readonly IToolPackageStore _toolPackageStore;
        private readonly IToolPackageDownloader _toolPackageDownloader;
        private readonly PackageId _packageId;
        private readonly string _packageVersion;
        private readonly string _configFilePath;
        private readonly string[] _sources;
        private readonly string _verbosity;

        public ToolInstallLocalInstaller(
            ParseResult parseResult,
            IToolPackageDownloader toolPackageDownloader = null)
        {
            _parseResult = parseResult;
            _packageId = new PackageId(parseResult.GetValue(ToolInstallCommandParser.PackageIdArgument));
            _packageVersion = parseResult.GetValue(ToolInstallCommandParser.VersionOption);
            _configFilePath = parseResult.GetValue(ToolInstallCommandParser.ConfigOption);
            _sources = parseResult.GetValue(ToolInstallCommandParser.AddSourceOption);
            _verbosity = Enum.GetName(parseResult.GetValue(ToolInstallCommandParser.VerbosityOption));

            (IToolPackageStore store,
                IToolPackageStoreQuery,
                IToolPackageDownloader installer) toolPackageStoresAndDownloader
                    = ToolPackageFactory.CreateToolPackageStoresAndDownloader(
                        additionalRestoreArguments: parseResult.OptionValuesToBeForwarded(ToolInstallCommandParser.GetCommand()));
            _toolPackageStore = toolPackageStoresAndDownloader.store;
            _toolPackageDownloader = toolPackageDownloader?? toolPackageStoresAndDownloader.installer;
            
            
            TargetFrameworkToInstall = BundledTargetFramework.GetTargetFrameworkMoniker();
        }

        public IToolPackage Install(FilePath manifestFile)
        {
            if (!string.IsNullOrEmpty(_configFilePath) && !File.Exists(_configFilePath))
            {
                throw new GracefulException(
                    string.Format(
                        LocalizableStrings.NuGetConfigurationFileDoesNotExist,
                        Path.GetFullPath(_configFilePath)));
            }

            VersionRange versionRange = _parseResult.GetVersionRange();

            FilePath? configFile = null;
            if (!string.IsNullOrEmpty(_configFilePath))
            {
                configFile = new FilePath(_configFilePath);
            }

            try
            {
                IToolPackage toolDownloadedPackage = _toolPackageDownloader.InstallPackage(
                        new PackageLocation(
                            nugetConfig: configFile,
                            additionalFeeds: _sources,
                            rootConfigDirectory: manifestFile.GetDirectoryPath().GetParentPath()),
                        _packageId,
                        versionRange,
                        TargetFrameworkToInstall,
                        verbosity: _verbosity);

                return toolDownloadedPackage;
            }
            catch (Exception ex) when (InstallToolCommandLowLevelErrorConverter.ShouldConvertToUserFacingError(ex))
            {
                throw new GracefulException(
                    messages: InstallToolCommandLowLevelErrorConverter.GetUserFacingMessages(ex, _packageId),
                    verboseMessages: new[] {ex.ToString()},
                    isUserError: false);
            }
        }
    }
}
