using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Specs {
    [TestFixture]
    public class JsonSpec {
        private Json<Command> _jsonCommand;
        private Json<NetworkInterface> _jsonInterface;
        private string _partialJsonStringForImport;
        private string _jsonStringFromCustomException;
        private string _fullJsonStringWithObjectCompletelyPopulated;
        private string _fullJsonStringWithObjectPartiallyPopulated;
        private string _fullInterfaceJsonString;
        private string _updateJson;
        private Json<ProviderData> _jsonProviderData;

        [SetUp]
        public void Setup()
        {
            _updateJson = "{\"name\":\"agentupdate\",\"value\":\"http://c0473242.cdn.cloudfiles.rackspacecloud.com/AgentService.zip,e6b39323fc3cf982b270fc114b9bb9e5\"}";

            _jsonCommand = new Json<Command>();
            _jsonInterface = new Json<NetworkInterface>();
            _partialJsonStringForImport = "{\"name\":\"password\",\"value\":\"somepassword\"}";
            _fullJsonStringWithObjectCompletelyPopulated = "{\"name\":\"password\",\"value\":\"somepassword\",\"key\":\"67745jhgj7683\"}";
            _fullJsonStringWithObjectPartiallyPopulated = "{\"key\":null,\"name\":\"password\",\"value\":\"somepassword\"}";

            _fullInterfaceJsonString = "{\"mac\":\"40:40:ed:65:h6\",\"dns\":[\"1.1.1.1\",\"64.39.2.138\"],\"label\":\"Label 1\",\"ips\":[{\"Ip\":\"3.3.3.3\",\"NetMask\":\"255.255.255.0\"},{\"Ip\":\"4.4.4.4\",\"NetMask\":\"255.255.255.0\"}],\"gateway\":\"10.1.1.100\"}";

            _jsonProviderData = new Json<ProviderData>();
        }

        [Test]
        public void should_return_empty_string_if_given_empty_string()
        {
            Assert.That(_jsonCommand.Deserialize(""), Is.Null);
        }

        [Test]
        [ExpectedException(typeof(UnsuccessfulCommandExecutionException), ExpectedMessage = "Problem deserializing the following json: '{'")]
        public void should_throw_exception_with_curly_brace()
        {
            _jsonCommand.Deserialize("{");
        }

        [Test]
        [ExpectedException(typeof(UnsuccessfulCommandExecutionException), ExpectedMessage = "Problem deserializing the following json: '{\"name\":\"password\", \"value\":\"abcdefghijklmnopqrstuvwxyzabcdefghijklmnoqrstuvwxyz'")]
        public void should_throw_exception_with_json_input_is_broken()
        {
            _jsonCommand.Deserialize("{\"name\":\"password\", \"value\":\"abcdefghijklmnopqrstuvwxyzabcdefghijklmnoqrstuvwxyz");
        }

        [Test]
        public void should_return_an_instance_of_a_command_object_from_partial_json_string() {
            var command = _jsonCommand.Deserialize(_partialJsonStringForImport);

            Assert.AreEqual("password", command.name);
            Assert.AreEqual("somepassword", command.value);
            Assert.IsNull(command.key);
        }

        [Test]
        public void should_return_an_instance_of_a_command_object_from_full_json_string() {
            var command = _jsonCommand.Deserialize(_fullJsonStringWithObjectCompletelyPopulated);

            Assert.AreEqual("password", command.name);
            Assert.AreEqual("somepassword", command.value);
            Assert.AreEqual("67745jhgj7683", command.key);
        }

        [Test]
        public void should_return_an_instance_of_an_interface_object()
        {
            var interface1 = _jsonInterface.Deserialize(_fullInterfaceJsonString);
            Assert.AreEqual("64.39.2.138", interface1.dns[1]);
        }

        [Test]
        public void should_serialize_partially_filled_object_to_json()
        {
            var command = new Command {name = "password", value = "somepassword"};

            Assert.AreEqual(_fullJsonStringWithObjectPartiallyPopulated, _jsonCommand.Serialize(command));
        }

        [Test]
        public void should_serialize_custom_exception_to_json() {
            var command = new { returncode = "1", message = "Key init was not called prior to Set Password command" };

            var _jsonObject = new Json<object>();
            _jsonStringFromCustomException = "{\"returncode\":\"1\",\"message\":\"Key init was not called prior to Set Password command\"}";
            Assert.AreEqual(_jsonStringFromCustomException, _jsonObject.Serialize(command));
        }

        [Test]
        public void print_serialized_json_string_and_deserialize()
        {
            const string stringWrong = "{\"mac\":\"40:40:92:9e:44:48\",\"dns\":[\"72.3.128.240\",\"72.3.128.241\"],\"label\":\"public\",\"ips\":[{\"ip\":\"98.129.220.138\",\"netmask\":\"255.255.255.0\"}],\"gateway\":\"98.129.220.1\",\"slice\":74532}";
            const string stringCorrt = "{\"mac\":\"40:40:92:9e:44:48\",\"dns\":[\"72.3.128.240\",\"72.3.128.241\"],\"label\":\"public\",\"ips\":[{\"ip\":\"98.129.220.138\",\"netmask\":\"255.255.255.0\"}],\"gateway\":\"98.129.220.1\"}";
            const string stringSomething =
                "{\"label\": \"private\", \"ips\": [{\"netmask\": \"255.255.224.0\", \"ip\": \"10.176.64.48\"}], \"mac\": \"40:40:d0:ed:cb:96\"}";

            var interface1 = new NetworkInterface
                                 {
                                     gateway = "98.129.220.1",
                                     label = "public",
                                     mac = "40:40:92:9e:44:48",
                                     dns = new[] { "72.3.128.240", "72.3.128.241" },
                                     ips =
                                         new[]
                                             {
                                                 new Ipv4Tuple {ip = "98.129.220.138", netmask = "255.255.255.0", enabled = "1"},
                                             },
                                     ip6s = new[]
                                                {
                                                    new Ipv6Tuple {ip = "2001:4801:787F:202:278E:89D8:FF06:B476", netmask = "96", enabled = "1", gateway = "fe80::def"}
                                                }
                                 };

            var serialized = _jsonInterface.Serialize(interface1);
            Assert.That(serialized, Is.EqualTo("{\"mac\":\"40:40:92:9e:44:48\",\"dns\":[\"72.3.128.240\",\"72.3.128.241\"],\"label\":\"public\",\"ips\":[{\"ip\":\"98.129.220.138\",\"netmask\":\"255.255.255.0\",\"enabled\":\"1\"}]," +
                "\"ip6s\":[{\"ip\":\"2001:4801:787F:202:278E:89D8:FF06:B476\",\"netmask\":\"96\",\"gateway\":\"fe80::def\",\"enabled\":\"1\"}]," +
                "\"gateway\":\"98.129.220.1\",\"routes\":null}"));

            _jsonInterface.Deserialize(stringCorrt);
            _jsonInterface.Deserialize(stringWrong);
            _jsonInterface.Deserialize(stringSomething);
            _jsonCommand.Deserialize(_updateJson);
        }

        [Test]
        public void should_deserilize_agent_update()
        {
            var command = _jsonCommand.Deserialize("{\"name\":\"version\",\"value\":\"agent\"}");
            Assert.That(command.name, Is.EqualTo("version"));
            Assert.That(command.value, Is.EqualTo("agent"));
        }

        [Test]
        public void should_remove_duplicate_dns_entries()
        {
            var interface1 = new NetworkInterface
            {
                gateway = "98.129.220.1",
                label = "public",
                mac = "40:40:92:9e:44:48",
                dns = new[] { "72.3.128.240", "72.3.128.240", "72.3.128.241", "72.3.128.241", },
                ips =
                    new[]
                                             {
                                                 new Ipv4Tuple {ip = "98.129.220.138", netmask = "255.255.255.0", enabled = "1"},
                                             },
                ip6s = new[]
                                                {
                                                    new Ipv6Tuple {ip = "2001:4801:787F:202:278E:89D8:FF06:B476", netmask = "96", enabled = "1", gateway = "fe80::def"}
                                                }
            };

            var serialized = _jsonInterface.Serialize(interface1);
            Assert.That(serialized, Is.EqualTo("{\"mac\":\"40:40:92:9e:44:48\",\"dns\":[\"72.3.128.240\",\"72.3.128.240\",\"72.3.128.241\",\"72.3.128.241\"],\"label\":\"public\",\"ips\":[{\"ip\":\"98.129.220.138\",\"netmask\":\"255.255.255.0\",\"enabled\":\"1\"}]," +
                "\"ip6s\":[{\"ip\":\"2001:4801:787F:202:278E:89D8:FF06:B476\",\"netmask\":\"96\",\"gateway\":\"fe80::def\",\"enabled\":\"1\"}]," +
                "\"gateway\":\"98.129.220.1\",\"routes\":null}"));

            var deserialized = _jsonInterface.Deserialize(serialized);

            deserialized.dns = deserialized.dns.Distinct().ToArray();

            var serializedAfterRemovingDuplicateDNSEntries = _jsonInterface.Serialize(deserialized);

            Assert.That(serializedAfterRemovingDuplicateDNSEntries, Is.EqualTo("{\"mac\":\"40:40:92:9e:44:48\",\"dns\":[\"72.3.128.240\",\"72.3.128.241\"],\"label\":\"public\",\"ips\":[{\"ip\":\"98.129.220.138\",\"netmask\":\"255.255.255.0\",\"enabled\":\"1\"}]," +
                "\"ip6s\":[{\"ip\":\"2001:4801:787F:202:278E:89D8:FF06:B476\",\"netmask\":\"96\",\"gateway\":\"fe80::def\",\"enabled\":\"1\"}]," +
                "\"gateway\":\"98.129.220.1\",\"routes\":null}"));

        }

        [Test]
        public void should_serialize_complete_provider_data()
        {
            var providerData = new ProviderData()
                                   {
                                       provider = "TheDarkKnightRises",
                                       region = "Gotham City",
                                       ip_whitelist = new List<string>() {"0", "1"},
                                       white_List_Ips = new List<string>() {"72.3.128.241", "72.3.128.241"},
                                       roles = new List<string>() {"rav_connect", "rav_managed"}
                                   };

            var serialzed = _jsonProviderData.Serialize(providerData);
            
            Assert.That(serialzed, Is.EqualTo("{\"region\":\"Gotham City\",\"roles\":[\"rav_connect\",\"rav_managed\"],\"ip_whitelist\":[\"0\",\"1\"],\"provider\":\"TheDarkKnightRises\",\"white_List_Ips\":[\"72.3.128.241\",\"72.3.128.241\"]}"));
        }

        [Test]
        public void should_serialize_partial_provider_data()
        {
            var providerData = new ProviderData()
            {
                provider = "TheDarkKnightRises",
                region = "Gotham City",
                ip_whitelist = new List<string>() { "" },
                white_List_Ips = new List<string>() { },
                roles = new List<string>() { "rav_connect", "rav_managed" }
            };

            var serialzed = _jsonProviderData.Serialize(providerData);

            Assert.That(serialzed, Is.EqualTo("{\"region\":\"Gotham City\",\"roles\":[\"rav_connect\",\"rav_managed\"],\"ip_whitelist\":[\"\"],\"provider\":\"TheDarkKnightRises\",\"white_List_Ips\":[]}"));
        }

        [Test]
        public void should_deserialize_complete_provider_data()
        {
            var stringProviderData =
                "{\"region\":\"Gotham City\",\"roles\":[\"rav_connect\",\"rav_managed\"],\"ip_whitelist\":[\"0\",\"1\"],\"provider\":\"TheDarkKnightRises\",\"white_List_Ips\":[\"72.3.128.241\",\"72.3.128.241\"]}";

            var deserialzed = _jsonProviderData.Deserialize(stringProviderData);
            Assert.AreEqual("rav_connect", deserialzed.roles[0]);
            Assert.AreEqual("Gotham City", deserialzed.region);
            Assert.AreEqual("TheDarkKnightRises", deserialzed.provider);
            Assert.IsTrue(deserialzed.ip_whitelist.Contains("0"));
            Assert.IsTrue(deserialzed.white_List_Ips.Contains("72.3.128.241"));
        }
        [Test]

        public void should_deserialize_partial_provider_data()
        {
            var stringProviderData =
                "{\"region\":\"Gotham City\",\"roles\":[\"rav_connect\",\"rav_managed\"],\"ip_whitelist\":[\"\"],\"provider\":\"TheDarkKnightRises\",\"white_List_Ips\":[]}";

            var deserialzed = _jsonProviderData.Deserialize(stringProviderData);
            Assert.AreEqual("rav_connect", deserialzed.roles[0]);
            Assert.AreEqual("Gotham City", deserialzed.region);
            Assert.AreEqual("TheDarkKnightRises", deserialzed.provider);
            Assert.AreEqual(string.Empty,deserialzed.ip_whitelist[0]);
            Assert.IsEmpty(deserialzed.white_List_Ips);
        }


    }
}
