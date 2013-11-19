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
using System.Linq;
using System.Management;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent
{
    public class XenStoreWmi : IXenStore
    {
        private readonly ILogger _logger;
        private const string rootWmi = @"\\.\root\wmi";

        private static ManagementObject _session;

        public XenStoreWmi(ILogger logger)
        {
            _logger = logger;
        }

        public ManagementObject Session
        {
            get
            {
                if (_session == null) CreateSession();
                return _session;
            }
        }

        private void CreateSession()
        {
            try
            {
                var mc = new ManagementClass(rootWmi, "CitrixXenStoreBase", new ObjectGetOptions());
                var paramList = new Dictionary<string, string> { { "Id", "newSession" } };
                var inParams = addPramsToMethod(mc, "AddSession", paramList);

                uint sessionId = 0;
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    var outParams = mo.InvokeMethod("AddSession", inParams, null);
                    sessionId = (uint)outParams.Properties["SessionId"].Value;
                }

                var query = string.Format(@"select * from CitrixXenStoreSession where SessionId={0}", sessionId);
                var objectSearcher = new ManagementObjectSearcher(rootWmi, query);
                var enumerator = objectSearcher.Get().GetEnumerator();
                enumerator.MoveNext();
                _session = (ManagementObject)enumerator.Current;
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
                _logger.Log(e.StackTrace);
                throw;
            }
        }

        public IEnumerable<string> Read(string key)
        {
            var paramList = new Dictionary<string, string> { { "Pathname", key } };
            var inParams = addPramsToMethod(Session, "GetChildren", paramList);
            
            try
            {
                var managementBaseObject = Session.InvokeMethod("GetChildren", inParams, null);

                if (managementBaseObject == null)
                    return new List<string>();
                
                var children = (ManagementBaseObject) managementBaseObject.GetPropertyValue("children");

                var keys = new List<string>();
                var value = (IEnumerable<string>) children.GetPropertyValue("ChildNodes");
                foreach (var v in value)
                {
                    _logger.Log(string.Format("Key: {0}, Value: {1}", key, v.Replace(key + "/", "")));
                    keys.Add(v.Replace(key + "/", ""));
                }

                return keys;
            }
            catch(Exception e)
            {
                return new List<string>();
            }
        }

        public IList<Command> GetCommands()
        {
            var commands = new List<Command>();
            try
            {
                
                var keys = Read(Constants.WritableDataHostBase).ValidateAndClean();

                foreach (var messageKey in keys)
                {
                    _logger.Log(messageKey);
                    var result = ReadKey(messageKey);
                    if (result.Contains("The system cannot find the file specified.")) continue;
                    var command = new Json<Command>().Deserialize(result);
                    command.key = messageKey;
                    commands.Add(command);
                }
            }
            catch (Exception e)
            {
                _logger.Log(e.ToString());
            }
            return commands;
        }

        public string ReadKey(string key)
        {
            return ReadDataKey(Constants.Combine(Constants.WritableDataHostBase, key));
        }

        public string ReadVmDataKey(string key)
        {
            return ReadDataKey(Constants.Combine(Constants.ReadOnlyDataConfigBase, Constants.NetworkingBase, key));
        }

        public string ReadVmProviderDataKey(string key)
        {
            var result = ReadDataKey(Constants.Combine(Constants.ReadOnlyDataConfigBase, Constants.ProviderDataBase, key));
            _logger.Log("ProviderData key:" + key + " value:" + result);
            return result;
        }

        public string ReadVmData(string key)
        {
            return ReadDataKey(Constants.Combine(Constants.ReadOnlyDataConfigBase, key));
        }

        private string ReadDataKey(string keyPath)
        {
            var paramList = new Dictionary<string, string>{ { "Pathname", keyPath}};
            var inParams = addPramsToMethod(Session, "GetValue", paramList);
            var managementBaseObject = Session.InvokeMethod("GetValue", inParams, null);
            if (managementBaseObject == null) return string.Empty;
            return (string) managementBaseObject.GetPropertyValue("value");
        }

        public void Write(string key, string value)
        {
            var keyPath = Constants.Combine(Constants.WritableDataGuestBase, key);
            var paramList = new Dictionary<string, string> { { "Pathname", keyPath }, { "value", value} };
            var inParams = addPramsToMethod(Session, "SetValue", paramList);
            Session.InvokeMethod("SetValue", inParams, null);
        }

        public void Remove(string key)
        {
            var keyPath = Constants.Combine(Constants.WritableDataHostBase, key);
            var paramList = new Dictionary<string, string> { { "Pathname", keyPath } };
            var inParams = addPramsToMethod(Session, "RemoveValue", paramList);
            Session.InvokeMethod("RemoveValue", inParams, null);
        }

        private static ManagementBaseObject addPramsToMethod(ManagementObject mObject, string methodName,
         Dictionary<string, string> param)
        {
            var inParams = mObject.GetMethodParameters(methodName);
            foreach (var keyValuePair in param)
                inParams.SetPropertyValue(keyValuePair.Key, keyValuePair.Value);
            return inParams;
        }
    }
}