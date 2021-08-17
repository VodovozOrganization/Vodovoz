using Mailjet.Api.Abstractions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MailjetDebugAPI.Hubs
{
	public class EmailsHub : Hub
	{
		public const string HubUrl = "/emails";

		public async Task EmailRecieved(EmailMessage emailMessage)
		{
			await Clients.All.SendAsync("EmailRecieved", emailMessage);
		}
	}
}
