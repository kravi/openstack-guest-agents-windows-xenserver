using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.WMI;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Netsh
{
    public interface INetshFirewallRuleNameAvailable
    {
        bool IsRuleAvailable(string ruleName);
    }

    public class NetshFirewallRuleNameAvailable : INetshFirewallRuleNameAvailable
    {
        private readonly IExecutableProcess _executableProcess;

        public NetshFirewallRuleNameAvailable(IExecutableProcess executableProcess, ILogger logger)
        {
            _executableProcess = executableProcess;
        }

        public bool IsRuleAvailable(string ruleName)
        {
            string command = string.Format("advfirewall firewall show rule name={0}", ruleName);
            var executableResult = _executableProcess.Run("netsh", command);
            return !executableResult.Output.ToArray().Contains("No rules match the specified criteria."); 
        }
    }
}
