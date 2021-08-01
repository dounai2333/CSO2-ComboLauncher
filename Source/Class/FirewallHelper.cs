using System;

using NetFwTypeLib;

namespace CSO2_ComboLauncher
{
    class Firewall : IDisposable
    {
        public INetFwMgr Mgr { get; private set; }

        public bool Running
        {
            get
            {
                return Mgr.LocalPolicy.CurrentProfile.FirewallEnabled;
            }
            set
            {
                Mgr.LocalPolicy.CurrentProfile.FirewallEnabled = value;
            }
        }

        public bool ICMPRequestAllowed
        {
            get
            {
                return Mgr.LocalPolicy.CurrentProfile.IcmpSettings.AllowInboundEchoRequest;
            }
            set
            {
                Mgr.LocalPolicy.CurrentProfile.IcmpSettings.AllowInboundEchoRequest = value;
            }
        }

        public Firewall()
        {
            try
            {
                Mgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            }
            catch
            {
                Mgr = null;
            }
        }

        public void AddProgramException(string name, string path)
        {
            foreach (INetFwAuthorizedApplication item in Mgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
                if (item.ProcessImageFileName == path)
                    return;

            INetFwAuthorizedApplication exception = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication")) as INetFwAuthorizedApplication;
            exception.Name = name;
            exception.ProcessImageFileName = path;
            exception.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            exception.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
            exception.Enabled = true;

            Mgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(exception);
        }

        public void RemoveProgramException(string path)
        {
            foreach (INetFwAuthorizedApplication item in Mgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
            {
                if (item.ProcessImageFileName == path)
                {
                    Mgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Remove(path);
                    break;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Mgr = null;
            }
        }
    }
}
