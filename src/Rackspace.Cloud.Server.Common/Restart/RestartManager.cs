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
using System.Diagnostics;
using System.Text;
using Rackspace.Cloud.Server.Common.Commands;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Common.Restart
{
    public static class RestartManager
    {
        static RestartManager()
        {
            RestartNeeded = false;
            CommandSetsToRun = 1;
            NoCommandsFoundRestartCount = 2;
        }

        public static bool RestartNeeded { get; set; }

        public static int CommandSetsToRun { get; set; }
        public static int NoCommandsFoundRestartCount { get; set; }

        public static void RestartMachine()
        {
            Process.Start(@"shutdown.exe", "/r /t 5 /f /d p:02:04");
            CommandsController.ProcessCommands = false;
        }

        public static void CommandsRunRestartCheck(ILogger logger)
        {
            if (RestartNeeded)
            {
                if (CommandSetsToRun <= 0)
                {
                    logger.Log("Restart needed...");
                    RestartMachine();
                    logger.Log("Restart command sent...");
                }
                CommandSetsToRun -= 1;
            }
        }

        public static void NoCommandsFoundRestartCheck(ILogger logger)
        {
            if (RestartNeeded)
            {
                if (NoCommandsFoundRestartCount <= 0)
                {
                    CommandsRunRestartCheck(logger);
                }
                NoCommandsFoundRestartCount -= 1;
            }
        }
    }
}
