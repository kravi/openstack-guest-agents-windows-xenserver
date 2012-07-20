﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Commands;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs {
    [TestFixture]
    public class ResetNetworkSpec {
        private ResetNetwork command;
        private IXenNetworkInformation xenNetworkInformation;
        private ISetNetworkInterface setNetworkInterface;
        private Network network;
        private ExecutableResult result;
        private NetworkInterface networkInterface;
        private ISetNetworkRoutes setNetworkRoutes;

        private ISetProviderData setProviderData;
        private IXenProviderDataInformation xenProviderDataInformation;
        private ProviderData providerData;

        [SetUp]
        public void Setup() {
            xenNetworkInformation = MockRepository.GenerateMock<IXenNetworkInformation>();
            setNetworkInterface = MockRepository.GenerateMock<ISetNetworkInterface>();
            setNetworkRoutes = MockRepository.GenerateMock<ISetNetworkRoutes>();

            xenProviderDataInformation = MockRepository.GenerateMock<IXenProviderDataInformation>();
            setProviderData = MockRepository.GenerateMock<ISetProviderData>();
            

            networkInterface = new NetworkInterface();
            network = new Network();
            network.Interfaces.Add("fakemac", networkInterface);

            providerData = new ProviderData();

            command = new ResetNetwork(setNetworkInterface, xenNetworkInformation, setNetworkRoutes, setProviderData, xenProviderDataInformation);
            xenNetworkInformation.Stub(x => x.Get()).Return(network);
            xenProviderDataInformation.Stub(x => x.Get()).Return(providerData);

            result = command.Execute(null);            
        }

        [Test]
        public void should_set_interface_from_interfaceconfigiuration() {
            setNetworkInterface.AssertWasCalled(x => x.Execute(new List<NetworkInterface> { networkInterface }));
            setNetworkRoutes.AssertWasCalled(x => x.Execute(network));
            setProviderData.AssertWasCalled(x => x.Execute(providerData));
        }

        [TearDown]
        public void TearDown() {
            setNetworkInterface.VerifyAllExpectations();
        }
    }
}
