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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Communication;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent
{
    public class AgentUpdateMessageSender : IAgentUpdateMessageSender
    {
        private readonly ILogger _logger;
        public AgentUpdateMessageSender(ILogger logger)
        {
            _logger = logger;
        }

        public void Send(AgentUpdateInfo agentUpdateInfo)
        {
            IAgentUpdater agentUpdater;
            try
            {
                ConnectToRemotingHost(out agentUpdater);
                agentUpdater.DoUpdate(agentUpdateInfo);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("Error connecting to the Updater service: {0}", ex));
                throw new UnsuccessfulCommandExecutionException(
                    String.Format("UPDATE FAILED: {0}", ex.Message),
                    new ExecutableResult { ExitCode = "1" });
            }
        }

        private void ConnectToRemotingHost(out IAgentUpdater agentUpdater)
        {
            try
            {
                //Try connecting to the IPC server
                IpcClientChannel clientChannel = new IpcClientChannel();
                clientChannel.IsSecured = true;
                ChannelServices.RegisterChannel(clientChannel);

                agentUpdater = (IAgentUpdater)Activator.GetObject(typeof(IAgentUpdater), BuildIpcRemotingUri());
                agentUpdater.Equals(null);
            }
            catch (RemotingException rex)
            {
                _logger.Log(string.Format("Error connecting to the Updater service via IPC: {0}", rex));

                //IPC Server connection failed, try connecting to the TCP server if it exists, allow the exception to bubble up if it fails.
                agentUpdater = (IAgentUpdater)Activator.GetObject(typeof(IAgentUpdater), BuildTcpRemotingUri());
                agentUpdater.Equals(null);
            }
        }

        private string BuildIpcRemotingUri()
        {
            return String.Format("ipc://{0}/{1}", SvcConfiguration.IpcUriHost, SvcConfiguration.IpcUriName);
        }

        private string BuildTcpRemotingUri()
        {
            return string.Format("tcp://{0}:{1}/{2}", SvcConfiguration.RemotingUriHost, SvcConfiguration.RemotingPort, SvcConfiguration.RemotingUri);
        }
    }
}
