using System;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using AppCloud_Service.Dominio;

namespace AppCloud_Service
{
    public partial class ServiceMain : ServiceBase
    {
        private readonly TcpConnection tcpConnection;
        private ServiceStatus serviceStatus = new ServiceStatus();
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public ServiceMain()
        {
            InitializeComponent();
            tcpConnection = new TcpConnection();
        }

        protected override void OnStart(string[] args)
        {
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            tcpConnection.StartListener();
        }

        protected override void OnStop()
        {
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            tcpConnection.StopListener();
        }

        public enum ServiceState
            {
                SERVICE_STOPPED = 0x00000001,
                SERVICE_START_PENDING = 0x00000002,
                SERVICE_STOP_PENDING = 0x00000003,
                SERVICE_RUNNING = 0x00000004,
                SERVICE_CONTINUE_PENDING = 0x00000005,
                SERVICE_PAUSE_PENDING = 0x00000006,
                SERVICE_PAUSED = 0x00000007,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ServiceStatus
            {
                public int dwServiceType;
                public ServiceState dwCurrentState;
                public int dwControlsAccepted;
                public int dwWin32ExitCode;
                public int dwServiceSpecificExitCode;
                public int dwCheckPoint;
                public int dwWaitHint;
            }
    }
}
