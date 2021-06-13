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
    interface IQuickBinService
    {
        Task ConnectToQuickBin(string ipAddress, int port, List<string> events);
        event EventHandler<QuickBin> onMessageReceived;
    }

    public class QuickBinService : IQuickBinService
    {       
        public event EventHandler<QuickBin> onMessageReceived;
        public Socket socket { get; set; }
        private List<string> Messages { get; set; } = new List<string>();

        protected virtual void Dispose(bool disposing)
        {
            
        }

        public async Task ConnectToQuickBin(string QuickBinIpAddress, int QuickBinWebSocketPort, List<string> events)
        {
            if (socket.Connected)
            {
                var remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                if (remoteEndPoint.Address == IPAddress.Parse(QuickBinIpAddress))
                {
                    events.Add($"QuickBin connected at end point: {QuickBinIpAddress}");
                }                
            }
            if (!socket.Connected)
            { 
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    events.Add($"Connecting to: {QuickBinIpAddress}:{QuickBinWebSocketPort}");
                    await socket.ConnectAsync(QuickBinIpAddress, QuickBinWebSocketPort);
                    if (socket.Connected)
                    {
                        events.Add("CONNECTED" + Environment.NewLine);
                        socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, HandleMessageReceived, null);
                    }
                    else
                    {
                        events.Add($"Failed to connect to {QuickBinIpAddress}:{QuickBinWebSocketPort}");
                    }

                }
                catch (Exception ex)
                {
                    events.Add(ex.Message);
                }

            }
        }

        private void HandleMessageReceived(IAsyncResult ar)
        {
            string msgHolder = null;

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

        }
    }
}
