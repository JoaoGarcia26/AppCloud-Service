using System;
using System.DirectoryServices.AccountManagement;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppCloud_Service.Dominio
{
    internal class TcpConnection
    {
        private TcpListener listener;
        private readonly LogGenerator logGenerator;
        private readonly int port = 24442;
        private readonly IPAddress ipAddress = IPAddress.Any;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private DateTime dataTime;

        public TcpConnection()
        {
            logGenerator = new LogGenerator();
        }
        public void StartListener()
        {
            try
            {
                dataTime = DateTime.Now;
                listener = new TcpListener(ipAddress, port);
                listener.Start();
                logGenerator.WriteLogFile($"{dataTime}: Porta {port} iniciada com sucesso");
                ThreadPool.QueueUserWorkItem(ListenerThread, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                dataTime = DateTime.Now;
                logGenerator.WriteLogFile($"{dataTime}: Falha ao iniciar a porta {port} | {ex.Message}");
            }
        }

        public void StopListener()
        {
            try
            {
                cancellationTokenSource.Cancel();
                listener?.Stop();
                dataTime = DateTime.Now;
                logGenerator.WriteLogFile($"{dataTime}: Porta {port} fechada com sucesso");
            }
            catch (Exception ex)
            {
                dataTime = DateTime.Now;
                logGenerator.WriteLogFile($"{dataTime}: Erro ao fechar a porta {port} | {ex.Message}");
            }
        }

        private void ListenerThread(object state)
        {
            CancellationToken token = (CancellationToken)state;
            

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    IPEndPoint remoteIpClient = client.Client.RemoteEndPoint as IPEndPoint;
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                    dataTime = DateTime.Now;
                    logGenerator.WriteLogFile($"{dataTime}: Request realizado por {remoteIpClient}");
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.Interrupted)
                    {
                        dataTime = DateTime.Now;
                        logGenerator.WriteLogFile($"{dataTime}: Request interrompido | {ex.Message}");
                    }
                }
            }
        }

        private async void HandleClient(object state)
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

                        bool isValidCredentials = await Task.Run(() => ValidarCredenciaisAsync(credentials));

                        byte[] response = Encoding.UTF8.GetBytes(isValidCredentials.ToString());
                        stream.Write(response, 0, response.Length);
                    }
                }
                catch (Exception ex)
                {
                    dataTime = DateTime.Now;
                    logGenerator.WriteLogFile($"{dataTime}: Pacote de request inválido | {ex.Message}");
                }
            }
        }
        private async Task<bool> ValidarCredenciaisAsync(string credentials)
        {
            string[] parts = credentials.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            string username = parts[0];
            string password = parts[1];

            try
            {
                bool result = await Task.Run(() => ValidarCredenciaisSincrono(username, password));
                return result;
            }
            catch (Exception ex)
            {
                dataTime = DateTime.Now;
                logGenerator.WriteLogFile($"{dataTime}: Erro na validação de credenciais | {ex.Message}");
                return false;
            }
        }
        private bool ValidarCredenciaisSincrono(string username, string password)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine, "127.0.0.1", username, password))
            {
                dataTime = DateTime.Now;
                try
                {
                    bool result = context.ValidateCredentials(username, password);
                    if (result)
                    {
                        logGenerator.WriteLogFile($"{dataTime}: {username} conectou com sucesso");
                    }
                    else
                    {
                        logGenerator.WriteLogFile($"{dataTime}: {username} falhou ao se conectar, senha inválida");
                    }
                    return result;
                }
                catch (System.IO.FileNotFoundException)
                {
                    logGenerator.WriteLogFile($"{dataTime}: {username} conectou com sucesso");
                    return true;
                }
                catch (System.DirectoryServices.AccountManagement.PrincipalOperationException)
                {
                    logGenerator.WriteLogFile($"{dataTime}: {username} falhou ao se conectar, senha inválida");
                    return false;
                }
            }
        }
    }
}
