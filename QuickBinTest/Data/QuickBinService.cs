using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace QuickBinTest.Data
{
    interface IQuickBinService
    {
        Task RegisterClient(string clientName, string baseUrl, List<string> issues);
        Task SendAsync(string message);
        Task DisconnectAsync();
        event EventHandler<QuickBin> onMessageReceived;
    }

    public class QuickBinService : IQuickBinService
    {
        private string _hubUrl;
        private HubConnection _hubConnection;
        private string _clientName;
        public event EventHandler<QuickBin> onMessageReceived;

        public async Task RegisterClient(string clientName, string baseUrl, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                issues.Add("Client Name cannot be null or empty.");
                return;
            }

            _clientName = clientName;

            try
            {
                _hubUrl = baseUrl.TrimEnd('/') + QuickBinHub.HubUrl;
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();

                _hubConnection.On<string, string>("Broadcast", HandleIncomingMessage);
                await _hubConnection.StartAsync();
                await SendAsync($"[Notice] {clientName} joined hub.");
            }
            catch (Exception ex)
            {
                issues.Add($"Error regeristing client: {clientName}. {ex.Message} ");
                return;
            }
        }

        public async Task SendAsync(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _hubConnection.SendAsync("Broadcast", _clientName, message);
            }
        }

        private void HandleIncomingMessage(string name, string message)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            bool isMine = name.Equals(_clientName, StringComparison.OrdinalIgnoreCase);
            if (isMine)
            {
                return;
            }
            var quickBin = JsonSerializer.Deserialize<QuickBin>(message);
            onMessageReceived?.Invoke(this, quickBin);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public async Task DisconnectAsync()
        {
            await SendAsync($"[Notice] {_clientName} left hub.");

            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();

            _hubConnection = null;
        }
    }
}
