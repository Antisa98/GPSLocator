using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace GPSLocatorClient
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var connection = new HubConnectionBuilder()
				.WithUrl("https://localhost:7028/gpsHub")
				.Build();

			connection.On<string>("ReceiveRequest", message =>
			{
				Console.WriteLine($"New request received: {message}");
			});

			try
			{
				await connection.StartAsync();
				Console.WriteLine("Connected to GPSLocator SignalR hub.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error connecting: {ex.Message}");
				return;
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			await connection.StopAsync();
		}
	}
}
