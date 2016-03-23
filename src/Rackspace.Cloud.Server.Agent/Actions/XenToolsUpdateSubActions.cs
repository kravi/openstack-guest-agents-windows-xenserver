using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IXenToolsUpdateSubActions
    {
        bool IsXenToolsUpdateSignalPresent();
        bool RemoveXenToolsUpdateSignal();
        bool WriteXenToolsUpdateSignal();
    }

    public class XenToolsUpdateSubActions : IXenToolsUpdateSubActions
    {
        private readonly ILogger _logger;

        public XenToolsUpdateSubActions(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsXenToolsUpdateSignalPresent()
        {
            using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey))
            {
                if (rk != null)
                {
                    var signal = rk.GetValue(Constants.XenToolsUpdateSignalKey);

                    if (signal != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool RemoveXenToolsUpdateSignal()
        {
            try
            {
                using (var rk = Registry.LocalMachine.OpenSubKey(Constants.RackspaceRegKey, true))
                {
                    if (rk != null)
                    {
                        rk.DeleteValue(Constants.XenToolsUpdateSignalKey);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
                return false;
            }
        }

        public bool WriteXenToolsUpdateSignal()
        {
            try
            {
                using (var rsrk = Registry.LocalMachine.CreateSubKey(Constants.RackspaceRegKey))
                {
                    if (rsrk != null)
                    {
                        rsrk.SetValue(Constants.XenToolsUpdateSignalKey, "True");
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.ToString());
                return false;
            }
        }

    }
}
