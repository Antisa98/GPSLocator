using Microsoft.AspNetCore.SignalR;

namespace GPSLocator.Hubs
{
	public class GPSHub : Hub
	{
		public async Task SendRequest(string message)
		{
			await Clients.All.SendAsync("ReceiveRequest", message);
		}
	}
}
