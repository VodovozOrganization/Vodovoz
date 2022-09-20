using System;
using Mailjet.Api.Abstractions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MailjetDebugAPI.Hubs
{
	public class EmailsHub : Hub
	{
		public const string HubUrl = "/emails";
		private readonly ILogger<EmailsHub> _logger;

		public EmailsHub(ILogger<EmailsHub> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task EmailRecieved(EmailMessage emailMessage)
		{
			await Clients.All.SendAsync("EmailRecieved", emailMessage);
		}

		public override async Task OnConnectedAsync()
		{
			_logger.LogInformation($"Client connected to Email Hub.");
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception ex)
		{
			if(ex != null)
			{
				_logger.LogError(ex, ex.Message);
			}
			await base.OnDisconnectedAsync(ex);
		}
	}
}
