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
    public class QuickBin
    {
        private Socket socket { get; set; }
        public List<string> RecivedMessages { get; set; } = new List<string>();
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }       
        public string QuickBinIpAddress { get; set; }
        public int QuickBinSocketPort { get; set; }
        public List<string> Activity { get; set; } = new List<string>();

        public event EventHandler onMessageReceived;

        public void IntializeSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Connect()
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
                    Activity.Add($"{DateTime.Now} Connecting to: {QuickBinIpAddress}:{QuickBinSocketPort}");
                    await socket.ConnectAsync(QuickBinIpAddress, QuickBinSocketPort);
                    if (socket.Connected)
                    {
                        Activity.Add($"{DateTime.Now} Connected {Environment.NewLine}");
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

        public void Disconnect()
        {
            socket.Close();
            if (Activity.Count > 100)
            {
                Activity.Remove(Activity.First());
            }
            Activity.Add($"{DateTime.Now}> Disconnected and closed socket.");
        }

        private void HandleMessageReceived(IAsyncResult ar)
        {
            try
            {
                if (socket.Available == 0)
                {
                    Activity.Add("Socket not available");
                }

                socket.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = socket.Receive(buf); //, buf.Length, 0);

                if (rec < buf.Length)
                    Array.Resize<byte>(ref buf, rec);
                socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, HandleMessageReceived, null);

                RecivedMessages.Add($"{DateTime.Now} {Encoding.UTF8.GetString(buf)}");                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Activity.Add($"{DateTime.Now}> Callback failed: {ex.Message}");
            }

            onMessageReceived?.Invoke(this, EventArgs.Empty);
        }
        
        public void SendCommand(string commandString)
        {
            if (!socket.Connected)
            {
                Activity.Add("{DateTime.Now}> Quickbin not connected.");
            }

            try
            {                
                socket.Send(Encoding.UTF8.GetBytes(commandString + Environment.NewLine));
            }
            catch (Exception ex)
            {
                Activity.Add($"{DateTime.Now}> Error sending command to QuickBin: {ex.Message}");
            }
        }

        public void SendLines()
        {
            var message = $"DRAW{Line1}^{Line2}^{Line3}";
            SendCommand(message);
            Activity.Add($"{DateTime.Now}> Sent lines {Line1} {Line2} {Line3}");
        }

    }
}
