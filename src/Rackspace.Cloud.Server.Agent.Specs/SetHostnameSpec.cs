using System;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Commands;
using Rackspace.Cloud.Server.Common.Logging;
using Rhino.Mocks;
using System.Linq;

namespace Rackspace.Cloud.Server.Agent.Specs
{
    [TestFixture]
    public class SetHostnameSpec
    {
        private ISetHostnameAction _action;
        private ILogger _logger;
        private SetHostname _command;

        [SetUp]
        public void Setup() {
            _action = MockRepository.GenerateMock<ISetHostnameAction>();
            _logger = MockRepository.GenerateMock<ILogger>();
            _command = new SetHostname(_logger, _action);
        }

        [Test]
        public void should_call_sethostname_action()
        {
            const string newhost = "newhost";

            _action.Expect(a => a.SetHostname(newhost)).Return("0");
            var result = _command.Execute(newhost);

            Assert.AreEqual("0", result.ExitCode);
        }

        [Test]
        public void should_not_execute_set_password_when_diffiehellman_statics_not_set()
        {
            const string newhost = "newhost";

            _action.Expect(a => a.SetHostname(newhost)).Throw(new Exception("sample error"));
            var result = _command.Execute(newhost);

            Assert.AreEqual("1", result.ExitCode);
            Assert.AreEqual("SetHostname failed", result.Error.First());
        }

        [TearDown]
        public void Teardown()
        {
            _action.VerifyAllExpectations();
        }
        
    }
}