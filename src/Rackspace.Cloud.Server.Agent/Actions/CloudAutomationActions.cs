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
using Rackspace.Cloud.Server.Common.Restart;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface ICloudAutomationActions
    {
        void RunKMSActivateCloudAutomationScripts();
        void RunPostRebootCloudAutomationScripts();
    }

    public class CloudAutomationActions : ICloudAutomationActions
    {
        private readonly ILogger _logger;
        private readonly ICloudAutomationSubActions _cloudAutomationSubActions;

        public CloudAutomationActions(ILogger logger, ICloudAutomationSubActions cloudAutomationSubActions)
        {
            _logger = logger;
            _cloudAutomationSubActions = cloudAutomationSubActions;
        }

        #region ICloudAutomationActions Members

        public void RunPostRebootCloudAutomationScripts()
        {
            try
            {
                _logger.Log("Checking for KMSActivate cloud automation registry entry");
                if (_cloudAutomationSubActions.IsKMSActivateSignalPresent())
                {
                    _logger.Log("KMSActivate registry entry found, removing registry entry");
                    if (_cloudAutomationSubActions.RemoveKMSActivateSignal())
                    {
                        _logger.Log("Successfully removed KMSActivate registry entry, running cloud automation scripts");
                        _cloudAutomationSubActions.RunCloudAutomationScripts();
                    }
                    else
                    {
                        _logger.Log("Could not delete the KMSActivate registry entry for cloud automation, not running cloud automation.");
                    }
                }
                else
                {
                    _logger.Log("KMSActivate cloud automation registry entry not detected.");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("Error running cloud automation: {0}", ex));
            }
        }

        public void RunKMSActivateCloudAutomationScripts()
        {
            try
            {
                _logger.Log("Checking for SysPrep cloud automation registry entry");
                if (_cloudAutomationSubActions.IsSysPrepSignalPresent())
                {
                    _logger.Log("SysPrep registry entry found, removing registry entry");
                    if (_cloudAutomationSubActions.RemoveSysPrepSignal())
                    {
                        _logger.Log("Successfully removed SysPrep registry entry, checking to see if a reboot is pending");
                        if (RestartManager.RestartNeeded)
                        {
                            _logger.Log("Reboot is pending, writing KMSActivate registry entry for post reboot cloud automation script execution");
                            _cloudAutomationSubActions.WriteKMSActivateSignal();
                        }
                        else
                        {
                            _logger.Log("Reboot is not pending, running cloud automation scripts");
                            _cloudAutomationSubActions.RunCloudAutomationScripts();
                        }
                    }
                    else
                    {
                        _logger.Log("Could not remove the SysPrep registry key, not writing the KMSActivate registry key for cloud automation.");
                    }
                }
                else
                {
                    _logger.Log("SysPrep cloud automation registry entry not detected");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("An error occurred while writing the KMSActivate registry key for cloud automation: {0}", ex));
            }
        }

        #endregion
    }
}
