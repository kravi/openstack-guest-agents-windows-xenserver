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
using System.Management;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent {

    public class AdministratorAccountNameFinder : IAdministratorAccountNameFinder {
        public string Find()
        {
            var admin = GetAdminAccount(true);

            if (string.IsNullOrEmpty(admin))
            {
                //If we get here we are most likely running on a Domain Controller
                //and we need to get the domain admin account
                //NOTE: A password reset for a Domain Admin Account will only work from our system account if we are running on a DC
                admin = GetAdminAccount(false);
            }

            return admin;
        }

        private string GetAdminAccount(bool local)
        {
            var msc = new ManagementScope("\\root\\cimv2");
            const string QUERY_STRING_LOCAL = "SELECT * FROM Win32_UserAccount WHERE LocalAccount = TRUE";
            const string QUERY_STRING_NON_LOCAL = "SELECT * FROM Win32_UserAccount WHERE LocalAccount = FALSE";
            var q = new SelectQuery(local ? QUERY_STRING_LOCAL : QUERY_STRING_NON_LOCAL);
            var query = new ManagementObjectSearcher(msc, q);
            var queryCollection = query.Get();

            var administratorAccountName = "";
            foreach (ManagementObject mo in queryCollection)
            {
                var sid = mo["SID"].ToString();
                if (sid.LastIndexOf("-500") != (sid.Length - 4)) continue;

                administratorAccountName = String.Format("{0}", mo["Name"]);
            }

            return administratorAccountName;
        }
    }

}