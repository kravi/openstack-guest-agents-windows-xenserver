using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Commands;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs {
    [TestFixture]
    public class ResetNetworkSpec {
        private ResetNetwork command;
        private IXenNetworkInformation xenNetworkInformation;
        private IXenUserMetadata xenUserMetadata;
        private ISetNetworkInterface setNetworkInterface;
        private Network network;
        private ExecutableResult result;
        private NetworkInterface networkInterface;
        private ISetNetworkRoutes setNetworkRoutes;

        private ISetProviderData setProviderData;
        private IXenProviderDataInformation xenProviderDataInformation;
        private ProviderData providerData;
        private List<string> userMetadata;
        private ISetHostnameAction setHostname;
        private IXenStore _xenStore;
        private const string hostname = "abc";
        private static readonly List<string> vmKeys = new List<string>() { "user-metadata" };
        private static readonly List<string> metadata = new List<string>() { "test" };

        [SetUp]
        public void Setup() {
            xenNetworkInformation = MockRepository.GenerateMock<IXenNetworkInformation>();
            xenUserMetadata = MockRepository.GenerateMock<IXenUserMetadata>();
            setNetworkInterface = MockRepository.GenerateMock<ISetNetworkInterface>();
            setNetworkRoutes = MockRepository.GenerateMock<ISetNetworkRoutes>();

            xenProviderDataInformation = MockRepository.GenerateMock<IXenProviderDataInformation>();
            setProviderData = MockRepository.GenerateMock<ISetProviderData>();
            setHostname = MockRepository.GenerateMock<ISetHostnameAction>();
            _xenStore = MockRepository.GenerateMock<IXenStore>();
            

            networkInterface = new NetworkInterface();
            network = new Network();
            network.Interfaces.Add("fakemac", networkInterface);

            providerData = new ProviderData();
            userMetadata = new List<string>();

            command = new ResetNetwork(setNetworkInterface, xenNetworkInformation, setNetworkRoutes, setProviderData, xenProviderDataInformation, setHostname, _xenStore, xenUserMetadata);
        }

        [Test]
        public void should_set_interface_from_interfaceconfigiuration() {
            xenNetworkInformation.Stub(x => x.Get()).Return(network);
            xenUserMetadata.Stub(x => x.GetKeys()).Return(userMetadata);
            xenProviderDataInformation.Stub(x => x.Get()).Return(providerData);
            _xenStore.Stub(x => x.ReadVmData("hostname")).Return(hostname);
            _xenStore.Stub(x => x.Read("vm-data")).Return(vmKeys);
            _xenStore.Stub(x => x.Read("vm-data/user-metadata")).Return(metadata);

            result = command.Execute(null);

            setNetworkInterface.AssertWasCalled(x => x.Execute(new List<NetworkInterface> { networkInterface }));
            setNetworkRoutes.AssertWasCalled(x => x.Execute(network));
            setProviderData.AssertWasCalled(x => x.Execute(providerData, userMetadata));
            setHostname.AssertWasCalled(x => x.SetHostname(hostname));
        }

        [TearDown]
        public void TearDown() {
            setNetworkInterface.VerifyAllExpectations();
        }
    }
}
