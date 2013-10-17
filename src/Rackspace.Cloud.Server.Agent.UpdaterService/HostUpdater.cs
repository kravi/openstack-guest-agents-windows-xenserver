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

using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Principal;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;
using StructureMap;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public class HostUpdater {
        private readonly ILogger _logger;

        public HostUpdater(ILogger logger) {
            _logger = logger;
        }

        public void OnStart() {
            _logger.Log("Updater Service starting ...");
            _logger.Log("Updater Version: " + Assembly.GetExecutingAssembly().GetName().Version);

            StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            IoC.Register();

            ConfigureRemotingHost();
        }

        private void ConfigureRemotingHost() {
            SetIpcChannel();
            SetRemotingType();
        }

        private void SetIpcChannel() {
            // Get SID code for the Built in Administrators group
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            // Get the NT account related to the SID
            NTAccount account = sid.Translate(typeof(NTAccount)) as NTAccount;

            IDictionary sProperties = new Hashtable();
            sProperties["portName"] = SvcConfiguration.IpcUriHost;
            sProperties["authorizedGroup"] = account.Value;

            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();

            IpcServerChannel channel = new IpcServerChannel(sProperties, serverProvider);
            ChannelServices.RegisterChannel(channel, true);

        }

        private void SetRemotingType() {
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(AgentUpdater), SvcConfiguration.IpcUriName, WellKnownObjectMode.SingleCall);
        }

        public void OnStop() {
            _logger.Log("Updater Service stopping ...");
        }
    }
}
