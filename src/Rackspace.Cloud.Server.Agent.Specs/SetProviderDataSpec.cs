using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Netsh;
using Rackspace.Cloud.Server.Common.Logging;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs
{

    public class SetProviderDataSpecBase
    {
        protected ProviderData ProviderData;
        protected SetProviderData SetProviderData;
        protected IExecutableProcessQueue ExecutableProcessQueue;
        protected ILogger Logger;
        protected INetshFirewallRuleNameAvailable NetshFirewallRuleNameAvailable;

        internal void Setup()
        {
            NetshFirewallRuleNameAvailable = MockRepository.GenerateMock<INetshFirewallRuleNameAvailable>();
            Logger = MockRepository.GenerateMock<ILogger>();

            ExecutableProcessQueue = MockRepository.GenerateMock<IExecutableProcessQueue>();
            ExecutableProcessQueue.Stub(x => x.Enqueue(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(
                ExecutableProcessQueue);

            SetProviderData = new SetProviderData(ExecutableProcessQueue, NetshFirewallRuleNameAvailable, Logger);

            ProviderData = GetProviderDataWithFakeRoles();
        }

        private ProviderData GetProviderDataWithFakeRoles()
        {
            return new ProviderData()
                       {
                           region = "DFW",
                           ip_whitelist = new List<string>() { "0", "1" },
                           roles = new List<string>() { "fake role1", "fake role2" },
                           white_List_Ips =
                               new List<string>() { "10.177.212.96", "10.181.136.241", "10.176.89.224", "10.177.212.79" }

                       };
        }

    }

    [TestFixture]
    public class SetProviderDataSpec : SetProviderDataSpecBase
    {

        [Test]
        public void should_not_have_called_executable_process()
        {

            Setup();
            SetProviderData.Execute(ProviderData);
            ExecutableProcessQueue.AssertWasNotCalled(
                queue => queue.Enqueue(Arg<string>.Is.Equal("netsh"), Arg<string>.Is.Anything));
            Logger.AssertWasCalled(l => l.Log("Role Names did not match. Roles names from provider data fake role1,fake role2. Role names from configuration rax_managed,rack_connect"));
        }

        [Test]
        public void should_have_called_executable_process_when_firewall_rule_is_available()
        {
            Setup();
            NetshFirewallRuleNameAvailable.Stub(x => x.IsRuleAvailable(Constants.SoftwareFirewallRuleName)).Return(true);

            ProviderData.roles = new List<string>() { "rax_managed", "rack_connect" };
            SetProviderData.Execute(ProviderData);
            ExecutableProcessQueue.AssertWasCalled(
                queue => queue.Enqueue("netsh", "advfirewall firewall set rule name=\"RS_FIREWALL_RULE\" new enable=yes remoteip=10.177.212.96,10.181.136.241,10.176.89.224,10.177.212.79"));
        }


        [Test]
        public void should_have_called_executable_process_when_firewall_rule_is_not_available()
        {
            Setup();
            NetshFirewallRuleNameAvailable.Stub(x => x.IsRuleAvailable(Constants.SoftwareFirewallRuleName)).Return(false);
            ProviderData.roles = new List<string>() { "rax_managed", "rack_connect" };

            SetProviderData.Execute(ProviderData);
            ExecutableProcessQueue.AssertWasCalled(
                queue => queue.Enqueue("netsh", "advfirewall firewall add rule name=\"RS_FIREWALL_RULE\" enable=yes dir=in profile=public,private,domain localip=any remoteip=10.177.212.96,10.181.136.241,10.176.89.224,10.177.212.79 protocol=tcp localport=445 remoteport=any edge=no action=allow"));
        }

        [Test]
        public void should_have_called_executable_process_when_firewall_rule_is_not_available_and_one_rule_name_match()
        {
            Setup();
            NetshFirewallRuleNameAvailable.Stub(x => x.IsRuleAvailable(Constants.SoftwareFirewallRuleName)).Return(false);
            ProviderData.roles = new List<string>() { "rax_managed", "dark_knight" };

            SetProviderData.Execute(ProviderData);
            ExecutableProcessQueue.AssertWasCalled(
                queue => queue.Enqueue("netsh", "advfirewall firewall add rule name=\"RS_FIREWALL_RULE\" enable=yes dir=in profile=public,private,domain localip=any remoteip=10.177.212.96,10.181.136.241,10.176.89.224,10.177.212.79 protocol=tcp localport=445 remoteport=any edge=no action=allow"));
        }

        [Test]
        public void should_not_have_called_executable_process_because_white_list_ips_count_is_less_than_zero()
        {

            Setup();

            ProviderData.white_List_Ips = new List<string>();
            ProviderData.roles = new List<string>() { "rax_managed", "rack_connect" };

            SetProviderData.Execute(ProviderData);
            ExecutableProcessQueue.AssertWasNotCalled(
                queue => queue.Enqueue(Arg<string>.Is.Equal("netsh"), Arg<string>.Is.Anything));
            Logger.AssertWasCalled(l => l.Log("White List Ips not available. Firewall rules will not be added/updated."));
        }


    }

}
