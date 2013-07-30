// Copyright 2011 OpenStack LLC.
// All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License"); you may
//    not use this file except in compliance with the License. You may obtain
//    a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//    License for the specific language governing permissions and limitations
//    under the License.

using System;
using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Commands {
    public class SetHostname : IExecutableCommand
    {
        private readonly ILogger _logger;
        private readonly ISetHostnameAction _setHostnameAction;

        public SetHostname(ILogger logger, ISetHostnameAction setHostnameAction)
        {
            _logger = logger;
            _setHostnameAction = setHostnameAction;
        }

        public ExecutableResult Execute(string value)
        {
            _logger.Log("Setting hostname to: " + value);
            try
            {
                var returnValue = _setHostnameAction.SetHostname(value);
                return new ExecutableResult {ExitCode = returnValue};
            }
            catch (Exception ex)
            {
                _logger.Log("Exception was : " + ex.Message + "\nStackTrace Was: " + ex.StackTrace);
                return new ExecutableResult { Error = new List<string> { "SetHostname failed" }, ExitCode = "1" };
            }
        }
    }
}
