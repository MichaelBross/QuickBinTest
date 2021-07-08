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
        public string IncommingMessage { get; set; }
        public List<string> RecivedMessages { get; set; } = new List<string>();
        public string Line1 { get; set; } = "SUP2020-10";
        public string Line2 { get; set; } = "AqAAAA";
        public string Line3 { get; set; } = "COUNT: 20";
        public string QuickBinIpAddress { get; set; }
        public int QuickBinSocketPort { get; set; }
        public List<string> Activity { get; set; } = new List<string>();
        public int RawWeightReading { get; set; }
        public int X0 { get; set; } = 8377163;
        public int X1 { get; set; } = 8354336;
        public float CalibrationWeightGrams { get; set; } = (float)100.00;
        public float WeightInGrams 
        {
            get
            {
                float ratio_1 = (float)(RawWeightReading - X0);
                float ratio_2 = (float)(X1 - X0);
                float ration = ratio_1 / ratio_2;
                float mass = CalibrationWeightGrams * ration;
                float rounded = (float)Math.Round(mass, 1);
                return rounded;
            }

        }

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
            if (!socket.Connected)
            {
                Activity.Add($"Cannot disconnect because the socket is not connected.");
                return;
            }

            socket.Disconnect(false);
            
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
                    onMessageReceived?.Invoke(this, EventArgs.Empty);
                }

                socket.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = socket.Receive(buf); //, buf.Length, 0);

                if (rec < buf.Length)
                    Array.Resize<byte>(ref buf, rec);
                socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, HandleMessageReceived, null);

                IncommingMessage += Encoding.UTF8.GetString(buf);

                if (IncommingMessage.EndsWith("\n"))
                {
                    RecivedMessages.Add($"{DateTime.Now} {IncommingMessage}");
                    
                    if (IncommingMessage.Contains("WEIGHT"))
                    {
                        GetRawWeightReading();
                    }
                    IncommingMessage = string.Empty;
                    onMessageReceived?.Invoke(this, EventArgs.Empty);
                }

                               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Activity.Add($"{DateTime.Now}> Callback failed: {ex.Message}");
                IncommingMessage = string.Empty;
            }

            
        }

        private void GetRawWeightReading()
        {
            int result = 0;
            var messageSegments = IncommingMessage.Split(" ");
            var valueString = messageSegments[1];
            int.TryParse(valueString, out result);
            if (result != 0)
            {
                RawWeightReading = result;
            }
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
            Activity.Add($"{DateTime.Now}> {message}");
        }


        public void GetReading()
        {
            var message = $"SEND AVERAGE";
            SendCommand(message);
            Activity.Add($"{DateTime.Now}> {message}");
        }
    }
}
