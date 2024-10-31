using Microsoft.AspNetCore.SignalR;

namespace GPSLocator.Services
{
	public class GPSHub : Hub
	{
		public async Task SendRequest(string message)
		{
			await Clients.All.SendAsync("ReceiveRequest", message);
		}
	}
}
