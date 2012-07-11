using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.Logging;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs
{
    public class ReadProviderDataInformationSpecBase
    {
        protected XenProviderDataInformation _reader;
        protected IXenStore _store;
        protected ProviderData _providerData;
        protected ILogger _logger;
    }

    [TestFixture]
    public class ReadProviderDataInformationSpec : ReadProviderDataInformationSpecBase
    {

        [SetUp]
        public void Setup()
        {
            _store = MockRepository.GenerateMock<IXenStore>();
            _logger = MockRepository.GenerateMock<ILogger>();
            _reader = new XenProviderDataInformation(_store);
            SetupDataProvider();
            _providerData = _reader.Get();
        }


        private void SetupDataProvider()
        {
            _store.Stub(x => x.ReadVmProviderDataKey("region")).Return("DFW");
            _store.Stub(x => x.ReadVmProviderDataKey("provider")).Return("Rackspace");
            _store.Stub(x => x.ReadVmProviderDataKey("roles")).Return("[\"rack_connect\", \"identity:user-admin\", \"rax_managed\", \"admin\"]");
            _store.Stub(x => x.Read("vm-data/provider_data/ip_whitelist")).Return(new string[] {"0","1","2","3"});
            _store.Stub(x => x.ReadVmProviderDataKey("ip_whitelist/0")).Return("10.10.4.12/23");
            _store.Stub(x => x.ReadVmProviderDataKey("ip_whitelist/1")).Return("10.10.4.11/23");
            _store.Stub(x => x.ReadVmProviderDataKey("ip_whitelist/2")).Return("10.11.4.12/22");
        }

        [Test]
        public void should_have_4_ip_whitelist()
        {
            Assert.AreEqual(4, _providerData.ip_whitelist.Count);
        }

        [Test]
        public void should_have_4_roles()
        {
            Assert.AreEqual(4, _providerData.roles.Count);
        }

        [Test]
        public void should_get_fakeproviderdata_information()
        {
            Assert.AreEqual("DFW", _providerData.region);
            Assert.AreEqual("rack_connect", _providerData.roles[0]);
            Assert.AreEqual("1", _providerData.ip_whitelist[1]);
        }

        [Test]
        public void should_be_4_ip_white_list()
        {
            Assert.AreEqual(4, _providerData.ip_whitelist.Count);
        }

        [Test]
        public void should_be_3_white_list_ips_in_collection()
        {
            Assert.IsTrue(_providerData.white_List_Ips.Contains("10.10.4.12/23"));
            Assert.IsTrue(_providerData.white_List_Ips.Contains("10.11.4.12/22"));
            Assert.IsTrue(_providerData.white_List_Ips.Contains("10.10.4.11/23"));
            Assert.IsFalse(_providerData.white_List_Ips.Contains("10.10.0.11/23"));
        }

        [Test]
        public void should_not_be_fake_white_list_ips_in_collection()
        {
            Assert.AreNotEqual("10.10.0.0/10", _providerData.white_List_Ips[0]);
            Assert.AreNotEqual("10.10.0.0/10", _providerData.white_List_Ips[1]);
            Assert.AreNotEqual("10.10.0.0/10", _providerData.white_List_Ips[2]);
        }

    }
}
