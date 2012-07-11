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
using System.Linq;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Netsh;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface ISetProviderData
    {
        void Execute(ProviderData providerData);
    }

    public class SetProviderData : ISetProviderData
    {
        private readonly IExecutableProcessQueue _executableProcessQueue;
        private readonly ILogger _logger;
        private readonly INetshFirewallRuleNameAvailable _netshFirewallRuleNameAvailable;

        public SetProviderData(IExecutableProcessQueue executableProcessQueue, INetshFirewallRuleNameAvailable netshFirewallRuleNameAvailable, ILogger logger)
        {
            _executableProcessQueue = executableProcessQueue;
            _netshFirewallRuleNameAvailable = netshFirewallRuleNameAvailable;
            _logger = logger;
        }

        public void Execute(ProviderData providerData)
        {
            string logMessage = string.Format(" Provider Data Deserialzed : {0}", new Json<ProviderData>().Serialize(providerData));

            _logger.Log(logMessage);

            if (CheckRoleNameMatch(providerData))
            {
                if (providerData.white_List_Ips.Count > 0)
                {
                    var command = string.Empty;
                    if (_netshFirewallRuleNameAvailable.IsRuleAvailable(Constants.SoftwareFirewallRuleName))
                    {
                        command = string.Format(
                            "advfirewall firewall set rule name=\"{0}\" new enable=yes remoteip={1}",
                            Constants.SoftwareFirewallRuleName, string.Join(",", providerData.white_List_Ips.ToArray()));
                    }
                    else
                    {
                        command =
                            string.Format(
                                "advfirewall firewall add rule name=\"{0}\" enable=yes dir=in profile=public,private,domain localip=any remoteip={1} protocol=tcp localport=445 remoteport=any edge=no action=allow",
                                Constants.SoftwareFirewallRuleName,
                                string.Join(",", providerData.white_List_Ips.ToArray()));

                    }
                    _executableProcessQueue.Enqueue("netsh", command);
                    _executableProcessQueue.Go();
                }
                else
                {
                    _logger.Log("White List Ips not available. Firewall rules will not be added/updated.");
                }
            }
            else
            {
                _logger.Log(string.Format("Role Names did not match. Roles names from provider data {0}. Role names from configuration {1}",
                                            string.Join(",", providerData.roles.ToArray()), string.Join(",", GetFirewallRoles().ToArray())));
            }

        }

        private bool CheckRoleNameMatch(ProviderData providerData)
        {
            var result = false;

            var configFirewallRoles = GetFirewallRoles();

            foreach (var roleName in providerData.roles)
            {
                if (configFirewallRoles.Any(configRoleName => string.Equals(roleName, configRoleName, StringComparison.OrdinalIgnoreCase)))
                {
                    result = true;
                }
            }

            return result;
        }


        private IEnumerable<string> SplitAndGetConvertToList(string tablesString)
        {
            return tablesString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(item => (item));
        }

        public List<string> GetFirewallRoles()
        {
            var result = new List<string>();
            result.AddRange(SplitAndGetConvertToList(SvcConfiguration.FirewallRoleNames));
            return result;
        }
    }
}
