// Copyright 2011 OpenStack LLC.
// All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License"); you may
//    not use this file except in compliance with the License. You may obtain
//    a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//    License for the specific language governing permissions and limitations
//    under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Commands
{
    public class EnsureMinAgentUpdater : IExecutableCommand
    {
        private readonly ISleeper _sleeper;
        private readonly IUnzipper _unzipper;
        private readonly IFileCopier _fileCopier;
        private readonly IFinalizer _finalizer;
        private readonly IServiceStopper _serviceStopper;
        private readonly IServiceStarter _serviceStarter;
        private readonly ILogger _logger;
        private readonly IBackupUpdater _backupUpdater;
        private readonly IExtractEmbededResource _extractEmbededResource;
        private readonly IVersionChecker _versionChecker;

        public EnsureMinAgentUpdater(ISleeper sleeper, IUnzipper unzipper, IFileCopier fileCopier, IFinalizer finalizer, IServiceStopper serviceStopper, IServiceStarter serviceStarter, ILogger logger, IBackupUpdater backupUpdater, IExtractEmbededResource extractEmbededResource, IVersionChecker versionChecker)
        {
            _sleeper = sleeper;
            _unzipper = unzipper;
            _fileCopier = fileCopier;
            _finalizer = finalizer;
            _serviceStopper = serviceStopper;
            _serviceStarter = serviceStarter;
            _logger = logger;
            _backupUpdater = backupUpdater;
            _extractEmbededResource = extractEmbededResource;
            _versionChecker = versionChecker;
        }

        public ExecutableResult Execute(string value)
        {
            var backupCreated = false;
            try
            {
                Statics.ShouldPollXenStore = false;
                string versionNumber = _versionChecker.Check(Version.AGENT_UPDATER_PATH);
                System.Version version;

                try
                {
                    version = new System.Version(versionNumber);
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("Version check failed, installing embedded updater.  Version Reported: {0}  Stack Trace: {1}", versionNumber, ex));
                    version = new System.Version("0.0.0.0");
                }

                if (version < UpdaterFiles.Updater.MinimumVersion)
                {
                    _logger.Log(String.Format("Updating the Agent Updater... \r\nWill resume in 10 seconds..."));
                    _sleeper.Sleep(10);
                    _extractEmbededResource.Extract(SvcConfiguration.AgentVersionUpdatesPath, Constants.UpdaterEmbeddedReleasePackagePath, Constants.UpdaterReleasePackageName);
                    _logger.Log("Waiting to unzip package.");
                    _unzipper.Unzip(Constants.UpdaterReleasePackage, Constants.UpdaterUnzipPath, "");
                    _serviceStopper.Stop("RackspaceCloudServersAgentUpdater");
                    _backupUpdater.Backup(Constants.UpdaterPath, Constants.UpdaterBackupPath);
                    backupCreated = true;
                    _fileCopier.CopyFiles(Constants.UpdaterUnzipPath, Constants.UpdaterPath, _logger);
                }
                else
                {
                    _logger.Log(string.Format("Agent Updater Version: {0}  detected, not updating updater.", version));    
                }

                return new ExecutableResult();
            }
            catch (Exception ex)
            {
                try
                {
                    if (backupCreated)
                    {
                        _serviceStopper.Stop("RackspaceCloudServersAgentUpdater");
                        _backupUpdater.Restore(Constants.UpdaterPath, Constants.UpdaterBackupPath);
                    }
                }
                catch (Exception exRestore)
                {
                    _logger.Log(String.Format("Exception was : {0}\nStackTrace Was: {1}", exRestore.Message, exRestore.StackTrace));
                }

                _logger.Log(String.Format("Exception was : {0}\nStackTrace Was: {1}", ex.Message, ex.StackTrace));
                return new ExecutableResult { Error = new List<string> { "Update failed" }, ExitCode = "1" };
            }
            finally
            {
                Statics.ShouldPollXenStore = true;
                _serviceStarter.Start("RackspaceCloudServersAgentUpdater");
                _finalizer.Finalize(new List<string> { Constants.UpdaterUnzipPath, Constants.UpdaterReleasePackage });
            }
        }

    }
}
