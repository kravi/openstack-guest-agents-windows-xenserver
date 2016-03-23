using System;
using System.Collections.Generic;
using System.Text;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IXenToolsUpdateActions
    {
        void ProcessXenToolsPostUpgradeActions();
    }

    public class XenToolsUpdateActions : IXenToolsUpdateActions
    {
        private readonly ILogger _logger;
        private readonly IXenToolsUpdateSubActions _xenToolsUpdateSubActions;
        private readonly ICommandFactory _factory;

        public XenToolsUpdateActions(ILogger logger, IXenToolsUpdateSubActions xenToolsUpdateSubActions, ICommandFactory commandFactory)
        {
            _logger = logger;
            _xenToolsUpdateSubActions = xenToolsUpdateSubActions;
            _factory = commandFactory;
        }


        public void ProcessXenToolsPostUpgradeActions()
        {
            if (_xenToolsUpdateSubActions.IsXenToolsUpdateSignalPresent())
            {
                _logger.Log("Xen Tools update signal was detected");
                if (XenStoreWmi.IsWmiEnabled())
                {
                    _logger.Log("Xen Store WMI Interface successfully detected, running resetnetwork");
                    if (RunResetNetworkCommand())
                    {
                        _logger.Log("Reset Netowrk successfully executed");
                        if (_xenToolsUpdateSubActions.RemoveXenToolsUpdateSignal())
                        {
                            _logger.Log("Xen Tools update signal successfully removed");
                        }
                        else
                        {
                            _logger.Log("Error removing Xen Tools update signal, please manually remove before reboot..");
                            _logger.Log(Constants.RackspaceRegKey);
                            _logger.Log(Constants.XenToolsUpdateSignalKey);
                        }
                    }
                    else
                    {
                        _logger.Log("Error running reset-network for post Xen Tools Upgrade process, will retry at next service startup");
                    }
                }
                else
                {
                    _logger.Log("WMI is not active yet, not processing Xen Tools Post Upgrade Command");
                }
            }
            else
            {
                _logger.Log("Xen Tools update signal is not present");
            }
        }

        public bool RunResetNetworkCommand()
        {
            var success = false;

            try
            {
                var executableCommand = _factory.CreateCommand("resetnetwork");
                var executableResult = executableCommand.Execute("nohostname");
                _logger.Log(executableResult.Output.Value());
                success = true;
            }
            catch (InvalidCommandException exception)
            {
                _logger.Log(exception.Message);
            }
            catch (UnsuccessfulCommandExecutionException exception)
            {
                var result = (ExecutableResult)exception.Data["result"];
                var output = "";
                var error = "";
                if (result.Output != null && !string.IsNullOrEmpty(result.Output.Value()))
                    output = ", Output:" + result.Output.Value();
                if (result.Error != null && !string.IsNullOrEmpty(result.Error.Value()))
                    error = ", Error:" + result.Error.Value();

                _logger.Log(string.Format("{0}{1}{2}", exception.Message, output, error));

            }

            return success;
        }

    }
}
