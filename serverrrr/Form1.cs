using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serverrrr
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private TcpClient client;
        private Thread thread;
        private bool running;
        private string logFolderPath;

        public Form1()
        {
            InitializeComponent();
            logFolderPath = @"C:\Logs\";
            Directory.CreateDirectory(logFolderPath);


        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            client = await server.AcceptTcpClientAsync();
            System.Console.WriteLine("Есть подключиние");
            byte[] bytesRead = new byte[255];

            string msg = Encoding.UTF8.GetString(bytesRead);

            await Console.Out.WriteLineAsync("Принято сообщение");
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
            clientThread.Start(client);


            running = true;
            thread = new Thread(new ThreadStart(ListenForClients));
            thread.Start();
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void ListenForClients()
        {
            while (running)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start(client);
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }
        private void HandleClient(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] messageBuffer = new byte[4096];
            int bytesRead;

            while (running)
            {
                while (clientStream.DataAvailable)
                {
                    try
                    {
                        bytesRead = clientStream.Read(messageBuffer, 0, messageBuffer.Length);

                        string clientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

                        string logFileName = $"{logFolderPath}\\log.txt";
                        string logMessage = $"{DateTime.Now} - IP: {clientIP} - Сообщение: {Encoding.UTF8.GetString(messageBuffer, 0, bytesRead)}";
                        File.AppendAllText(logFileName, logMessage + Environment.NewLine);

                        string clientMessage = Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);

                        // Добавляем сообщение в textBoxChat с использованием Invoke для обновления элемента управления из другого потока.
                        textBoxChat.Invoke(new Action(() =>
                        {
                            textBoxChat.AppendText($"[{tcpClient.Client.RemoteEndPoint.ToString()}]: {clientMessage}\r\n");
                        }));
                    }
                    catch (IOException)
                    {
                        break;
                    }
                }
            }

            tcpClient.Close();
        }

        private void textBoxChat_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
