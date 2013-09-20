using System;
using System.Management;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public class SetHostnameAction : ISetHostnameAction
    {
        public string SetHostname(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
                return 0.ToString();

            var oldName = Environment.MachineName;
            using (var cs = new ManagementObject(@"Win32_Computersystem.Name='" + oldName + "'"))
            {
                cs.Get();
                var inParams = cs.GetMethodParameters("Rename");
                inParams.SetPropertyValue("Name", hostname);
                var methodOptions = new InvokeMethodOptions(null, TimeSpan.MaxValue);
                var outParams = cs.InvokeMethod("Rename", inParams, methodOptions);
                return Convert.ToString(outParams.Properties["ReturnValue"].Value);
            }
        }
    }
}