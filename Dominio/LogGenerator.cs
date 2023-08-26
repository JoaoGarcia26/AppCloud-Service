using System.IO;

namespace AppCloud_Service.Dominio
{
    internal class LogGenerator
    {
        private readonly string path = "C:\\TSMIT AppCloud";
        private readonly string logFileName = "LogServer.txt";

        public LogGenerator()
        {
            CreateFileLog();
        }
        public void CreateFileLog()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!File.Exists($@"{path}\{logFileName}"))
                {
                    File.Create($@"{path}\{logFileName}");
                }
            }
        }
        public void WriteLogFile(string logText)
        {
            using (StreamWriter writer = File.AppendText($@"{path}\{logFileName}"))
            {
                writer.WriteLine($"{logText}");
            }
        }
    }
}
