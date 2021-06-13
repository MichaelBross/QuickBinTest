using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace QuickBinTest.Data
{
    public class QuickBinVM
    {
        public Socket socket { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        //public Communication MyModel { get; set; } = new Communication();
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }

        public List<string> Issues { get; set; } = new List<string>();
        public List<QuickBin> QuickBins { get; set; } = new List<QuickBin>();
        public string UserName { get; set; } = "Mike";
        public string MessageToSend { get; set; }
        public string QuickBinIpAddress { get; set; }
        public int QuickBinSocketPort { get; set; }
        public List<string> Activity { get; set; } = new List<string>();
        public event EventHandler onMessageReceived;

        public void IntializeSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ConnectToQuickBin()
        {
            if (socket.Connected)
            {
                var remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                if (remoteEndPoint.Address.ToString() == QuickBinIpAddress)
                {
                    Activity.Add($"QuickBin connected at end point: {QuickBinIpAddress}");
                }
            }
            if (!socket.Connected)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Activity.Add($"Connecting to: {QuickBinIpAddress}:{QuickBinSocketPort}");
                    await socket.ConnectAsync(QuickBinIpAddress, QuickBinSocketPort);
                    if (socket.Connected)
                    {
                        Activity.Add("CONNECTED" + Environment.NewLine);
                        socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, HandleMessageReceived, null);
                    }
                    else
                    {
                        Activity.Add($"Failed to connect to {QuickBinIpAddress}:{QuickBinSocketPort}");
                    }

                }
                catch (Exception ex)
                {
                    Activity.Add(ex.Message);
                }

            }
        }

        private void HandleMessageReceived(IAsyncResult ar)
        {
            try
            {
                if (socket.Available == 0)
                {
                    Messages.Add("Socket not available");
                }

                socket.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = socket.Receive(buf); //, buf.Length, 0);

                if (rec < buf.Length)
                    Array.Resize<byte>(ref buf, rec);
                socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, HandleMessageReceived, null);

                Messages.Add($"{DateTime.Now} {Encoding.UTF8.GetString(buf)}");                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Messages.Add($"callback failed: {ex.Message}");
            }

            onMessageReceived?.Invoke(this, EventArgs.Empty);
        }

        public void SendMessage()
        {
            if (!socket.Connected)
            {
                Messages.Add("Quickbin not connected.");
            }

            try
            {
                var formattedMessage = UserName + ">>> " + MessageToSend + Environment.NewLine;
                socket.Send(Encoding.UTF8.GetBytes(formattedMessage));
            }
            catch (Exception ex)
            {
                Messages.Add($"Error sending message to QuickBin: {ex.Message}");
            }
        }

        public void SendLines()
        {
            var message = $"DRAW{Line1}^{Line2}^{Line3}";
            //MyModel.Response = SendTcpMessage("192.168.86.27", 80, message);
        }

    }
}
