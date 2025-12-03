using Mailganer.Api.Client;
using Mailganer.Api.Client.Dto;
using Mailganer.Api.Client.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace EmailSend.Library.Services
{
	public class EmailSendService : IEmailSendService
	{
		private readonly ILogger<EmailSendService> _logger;
		private readonly MailganerClientV2 _mailganerClient;

		public EmailSendService(
			ILogger<EmailSendService> logger,
			MailganerClientV2 mailganerClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
		}

		public string EmaiInStopListErrorCodeString => MailganerClientV2.EmailInStopListErrorCode.ToString();

		/// <inheritdoc/>
		public async Task<Result> SendEmail(EmailMessage email)
		{
			_logger.LogInformation("Trying to send email to: {Email}", email.To);
			try
			{
				await _mailganerClient.Send(email);
			}
			catch(EmailInStopListException ex)
			{
				_logger.LogError(ex, "Email is in stop list: {Email}", email.To);
				return Result.Failure(new Error(EmaiInStopListErrorCodeString, "Email is in stop list"));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to send email: {Email}", email.To);
				return Result.Failure(new Error(string.Empty, $"Failed to send email: {ex.Message}"));
			}
			_logger.LogInformation("Email sent successfully to: {Email}", email.To);
			return Result.Success();
		}

		/// <inheritdoc/>
		public async Task<Result> CheckAndRemoveSpamEmailFromStopList(string emailTo, string emailFrom)
		{
			try
			{
				_logger.LogInformation(
					"Trying to remove email from stop list. Email address: {Email}",
					emailTo);

				var bounceInfo = await _mailganerClient.GetEmailBounseMessageInStopList(emailTo);

				if(string.IsNullOrEmpty(bounceInfo))
				{
					_logger.LogWarning(
						"Bounce info is empty. Cannot determine if email should be removed from stop list. Email address: {Email}",
						emailTo);
					return Result.Failure(new Error(string.Empty, "Bounce info is empty"));
				}

				if(!bounceInfo.ToLower().Contains("spam"))
				{
					_logger.LogWarning(
						"Bounce info does not indicate spam. Email will not be removed from stop list. Email address: {Email}. Bounce info: {BounceInfo}",
						emailTo,
						bounceInfo);
					return Result.Failure(new Error(string.Empty, "Email is in stop list and could not be removed. Bounce info does not indicate spam"));
				}

				await _mailganerClient.RemoveEmailFromStopList(emailFrom, emailTo);

				_logger.LogInformation(
					"Email removed from stop list successfully. Email address: {Email}",
					emailTo);

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to remove email from stop list. Email address: {Email}",
					emailTo);

				return Result.Failure(new Error(string.Empty, $"Failed to remove email from stop list: {ex.Message}"));
			}
		}
	}
}
