using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Netsh;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs
{
    [TestFixture]
    public class NetshFirewallRuleNameAvailableSpec
    {
        private NetshFirewallRuleNameAvailable netshFirewallRuleNameAvailable;
        private IExecutableProcess executableProcess;
        private ILogger logger;
        private string input;

        [SetUp]
        public void Setup()
        {
            executableProcess = MockRepository.GenerateMock<IExecutableProcess>();
            logger = MockRepository.GenerateMock<ILogger>();
            logger.Stub(x => x.Log(Arg<string>.Is.Anything));
            netshFirewallRuleNameAvailable = new NetshFirewallRuleNameAvailable(executableProcess, logger);
        }

        [Test]
        public void should_return_true_for_firewall_rule_not_available()
        {
            string command = string.Format("advfirewall firewall show rule name={0}", "FakeFirewallRuleName");
            executableProcess.Stub(x => x.Run("netsh",command)).Return(new ExecutableResult 
                {Output = new List<string> {"These are not the droids you are looking for"}});
            var result = netshFirewallRuleNameAvailable.IsRuleAvailable("FakeFirewallRuleName");
            Assert.IsTrue(result);

        }
        
        [Test]
        public void should_return_true_for_firewall_rule_not_available_multiple_string_array()
        {
            string command = string.Format("advfirewall firewall show rule name={0}", "FakeFirewallRuleName");
            executableProcess.Stub(x => x.Run("netsh", command)).Return(new ExecutableResult { Output = new List<string> { "These are not the droids you are looking for","Again these are the droids you are looking for." } });
            var result = netshFirewallRuleNameAvailable.IsRuleAvailable("FakeFirewallRuleName");
            Assert.IsTrue(result);
        }

        [Test]
        public void should_return_false_for_firewall_rule_not_available()
        {
            string command = string.Format("advfirewall firewall show rule name={0}", "FakeFirewallRuleName");
            executableProcess.Stub(x => x.Run("netsh", command)).Return(new ExecutableResult { Output = new List<string> { "No rules match the specified criteria." } });
            var result = netshFirewallRuleNameAvailable.IsRuleAvailable("FakeFirewallRuleName");
            Assert.IsFalse(result);

        }

        [Test]
        public void should_return_false_for_firewall_rule_not_available_multiple_string_array()
        {
            string command = string.Format("advfirewall firewall show rule name={0}", "FakeFirewallRuleName");
            executableProcess.Stub(x => x.Run("netsh", command)).Return(new ExecutableResult { Output = new List<string> { "No rules match the specified criteria.", "May be these are the droids you are looking for, being evil." } });
            var result = netshFirewallRuleNameAvailable.IsRuleAvailable("FakeFirewallRuleName");
            Assert.IsFalse(result);
        }

    }
}
