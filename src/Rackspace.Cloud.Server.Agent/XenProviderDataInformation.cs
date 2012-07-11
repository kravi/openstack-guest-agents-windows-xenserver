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
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using System.Linq;

namespace Rackspace.Cloud.Server.Agent
{
    public interface IXenProviderDataInformation
    {
        ProviderData Get();
    }

    public class XenProviderDataInformation : IXenProviderDataInformation
    {
        private readonly IXenStore _xenStore;

        public XenProviderDataInformation(IXenStore xenstore)
        {
            _xenStore = xenstore;
        }

        public ProviderData Get()
        {
            return GetProviderData();
        }

        private ProviderData GetProviderData()
        {
            var providerData = new ProviderData();

            GetProviderName(ref providerData);
            GetRoles(ref providerData);
            GetRegion(ref providerData);
            GetWhiteListIps(ref providerData); 
            return providerData;
        }

        public void GetProviderName(ref ProviderData providerData)
        {
            providerData.provider = _xenStore.ReadVmProviderDataKey(Constants.Provider);
        }

        public void GetRoles(ref ProviderData providerData)
        {
            var jsonData = _xenStore.ReadVmProviderDataKey(Constants.Roles);
            providerData.roles = new Json<List<string>>().Deserialize(jsonData);
        }

        public void GetRegion(ref ProviderData providerData)
        {
            providerData.region = _xenStore.ReadVmProviderDataKey(Constants.Region);
        }

        public void GetWhiteListIps(ref ProviderData providerData)
        {
            var numberOfWhiteListIps = _xenStore.Read(Constants.Combine(Constants.ReadOnlyDataConfigBase, Constants.ProviderDataBase, Constants.IpWhiteList));

            if (numberOfWhiteListIps != null && numberOfWhiteListIps.Any())
            {
                providerData.ip_whitelist.AddRange(numberOfWhiteListIps);

                foreach (var numberOfWhiteListIp in numberOfWhiteListIps)
                {
                    var jsonData = _xenStore.ReadVmProviderDataKey((Constants.Combine(Constants.IpWhiteList, numberOfWhiteListIp)));
                    if (!string.IsNullOrEmpty(jsonData)) providerData.white_List_Ips.Add(jsonData);
                }
            }
        }

    }
}
