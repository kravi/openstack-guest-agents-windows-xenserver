using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Actions;
using StructureMap;

namespace Rackspace.Cloud.Server.Agent.Specs
{
    [TestFixture]
    public class CloudAutomationSubActionsSpec
    {
        [SetUp]
        public void Setup()
        {
            Utility.ConfigureStructureMap();
        }

        [Test]
        public void should_detect_sysprep_key_and_remove_it_and_detect_it_is_gone()
        {
            var cloudAutomationActions = ObjectFactory.GetInstance<ICloudAutomationSubActions>();
            RegistryKey rk = Registry.LocalMachine.CreateSubKey(Constants.RackspaceRegKey);
            rk.SetValue(Constants.CloudAutomationSysPrepRegKey, "True");

            Assert.IsTrue(cloudAutomationActions.IsSysPrepSignalPresent());

            Assert.IsTrue(cloudAutomationActions.RemoveSysPrepSignal());

            Assert.IsFalse(cloudAutomationActions.IsSysPrepSignalPresent());
        }

        [Test]
        public void should_create_then_detect_kmsactivate_key_and_remove_it_and_detect_it_is_gone()
        {
            var cloudAutomationActions = ObjectFactory.GetInstance<ICloudAutomationSubActions>();

            Assert.IsTrue(cloudAutomationActions.WriteKMSActivateSignal());

            Assert.IsTrue(cloudAutomationActions.IsKMSActivateSignalPresent());

            Assert.IsTrue(cloudAutomationActions.RemoveKMSActivateSignal());

            Assert.IsFalse(cloudAutomationActions.IsKMSActivateSignalPresent());
        }

       
    }
}
