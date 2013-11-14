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
using System.IO;
using System.Text;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IBackupUpdater
    {
        void Backup(string sourcePath, string backupPath);
        void Restore(string targetPath, string backupPath);
    }

    public class BackupUpdater : IBackupUpdater
    {
        private readonly ILogger _logger;
        private readonly IFileCopier _fileCopier;

        public BackupUpdater(ILogger logger, IFileCopier fileCopier)
        {
            _logger = logger;
            _fileCopier = fileCopier;
        }

        public void Backup(string sourcePath, string backupPath)
        {
            if (Directory.Exists(backupPath))
            {
                Directory.Delete(backupPath, true);
            }
            Directory.CreateDirectory(backupPath);

            _fileCopier.CopyFiles(sourcePath, backupPath, _logger);
        }

        public void Restore(string targetPath, string backupPath)
        {
            _fileCopier.CopyFiles(backupPath, targetPath, _logger);
        }
    }
}
