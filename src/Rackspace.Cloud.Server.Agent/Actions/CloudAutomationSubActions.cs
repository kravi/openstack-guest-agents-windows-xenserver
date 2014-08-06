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
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface ICloudAutomationSubActions
    {
        //SysPrep Signal commands
        bool IsSysPrepSignalPresent();
        bool RemoveSysPrepSignal();

        //KMSActivate Signal Commands
        bool IsKMSActivateSignalPresent();
        bool WriteKMSActivateSignal();
        bool RemoveKMSActivateSignal();

        //Run cloud script(s) command
        void RunCloudAutomationScripts();
    }

    public class CloudAutomationSubActions : ICloudAutomationSubActions
    {
        private readonly ILogger _logger;

        public CloudAutomationSubActions(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsSysPrepSignalPresent()
        {
            using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey))
            {
                if (rk != null)
                {
                    var signal = rk.GetValue(Constants.CloudAutomationSysPrepRegKey);

                    if (signal != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool RemoveSysPrepSignal()
        {
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey, true))
                {
                    if (rk != null)
                    {
                        rk.DeleteValue(Constants.CloudAutomationSysPrepRegKey);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
                return false;
            }
        }

        public bool IsKMSActivateSignalPresent()
        {
            using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey))
            {
                if (rk != null)
                {
                    var signal = rk.GetValue(Constants.CloudAutomationKMSActivateRegKey);

                    if (signal != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool WriteKMSActivateSignal()
        {
            try
            {
                using (var rsrk = Registry.LocalMachine.CreateSubKey(Constants.RackspaceRegKey))
                {
                    if (rsrk != null)
                    {
                        rsrk.SetValue(Constants.CloudAutomationKMSActivateRegKey, "True");
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
                return false;
            }
        }

        public bool RemoveKMSActivateSignal()
        {
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey, true))
                {
                    if (rk != null)
                    {
                        rk.DeleteValue(Constants.CloudAutomationKMSActivateRegKey);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
                return false;
            }
        }

        public void RunCloudAutomationScripts()
        {
            StartProcess(Constants.CloudAutomationBatPath);
            StartProcess(Constants.CloudAutomationCmdPath);
        }

        private void StartProcess(string path)
        {
            try
            {
                _logger.Log(string.Format("Checking to see if the '{0}' file exists", path));
                if (File.Exists(path))
                {
                    _logger.Log(string.Format("Executing the file '{0}' for cloud automation", path));
                    Process.Start(path);
                    _logger.Log(string.Format("File '{0}' was started for cloud automation", path));
                }
                else
                {
                    _logger.Log(string.Format("File '{0}' not found", path));
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
            }
        }
    }
}
