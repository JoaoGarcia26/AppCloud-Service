using System;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace AppCloud_Service
{
    public partial class Service1 : ServiceBase
    {
        private TcpListener listener;
        private readonly int port = 24442;
        private readonly IPAddress ipAddress = IPAddress.Any;
        private ServiceStatus serviceStatus = new ServiceStatus();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            StartListener();
        }

        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            StopListener();
        }

        private void StartListener()
        {
            listener = new TcpListener(ipAddress, port);
            listener.Start();

            ThreadPool.QueueUserWorkItem(ListenerThread, cancellationTokenSource.Token);
        }

        private void StopListener()
        {
            listener?.Stop();
        }

        private void ListenerThread(object state)
        {
            CancellationToken token = (CancellationToken)state;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.Interrupted)
                    {
                        Console.WriteLine("Erro ao aceitar cliente: " + ex.Message);
                    }
                }
            }
        }

        private void HandleClient(object state)
        {
            TcpClient client = (TcpClient)state;

            using (client)
            {
                try
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string credentials = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        bool isValidCredentials = ValidarCredenciais(credentials);

                        byte[] response = Encoding.UTF8.GetBytes(isValidCredentials.ToString());
                        stream.Write(response, 0, response.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao lidar com o cliente: " + ex.Message);
                }
            }
        }

        private bool ValidarCredenciais(string credentials)
        {
            string[] parts = credentials.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            string username = parts[0];
            string password = parts[1];

            using (PrincipalContext context = new PrincipalContext(ContextType.Machine, "127.0.0.1", username, password))
            {
                try
                {
                    bool result = context.ValidateCredentials(username, password);
                    if (result == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    return true;
                }
                catch (System.DirectoryServices.AccountManagement.PrincipalOperationException)
                {
                    return false;
                }
            }
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
