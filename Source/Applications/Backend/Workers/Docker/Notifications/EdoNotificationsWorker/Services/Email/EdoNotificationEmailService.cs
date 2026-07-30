using Email.Infrastructure.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EdoNotificationsWorker.Services.Email
{
	public class EdoNotificationEmailService : IEdoNotificationEmailService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IOptionsSnapshot<EdoNotificationsOptions> _options;
		private readonly IBus _bus;
		private readonly ILogger<EdoNotificationEmailService> _logger;

		public EdoNotificationEmailService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmailMessageFactory emailMessageFactory,
			IOptionsSnapshot<EdoNotificationsOptions> options,
			IEmailBus emailBus,
			ILogger<EdoNotificationEmailService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_bus = emailBus ?? throw new ArgumentNullException(nameof(emailBus));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task NotifyAsync(string emails, string subject, string message, CancellationToken cancellationToken = default)
		{
			var emailList = emails
				.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(e => e.Trim())
				.ToList();

			if(emailList.Count == 0)
			{
				return;
			}

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отправка писем - уведомлений о событиях ЭДО");

			foreach(var email in emailList)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					_logger.LogWarning("Рассылка email-уведомлений ЭДО прервана по CancellationToken");
					break;
				}

				await SendToEmailAsync(unitOfWork, email, subject, message, cancellationToken);
			}
		}

		private async Task SendToEmailAsync(
			IUnitOfWork unitOfWork,
			string email,
			string subject,
			string message,
			CancellationToken cancellationToken)
		{
			try
			{
				var storedEmail = _emailMessageFactory.CreateStoredEmail(subject, email, "Уведомление по ЭДО");

				await unitOfWork.SaveAsync(storedEmail, cancellationToken: cancellationToken);

				var sendEmailMessage = _emailMessageFactory.CreateSendEmailMessage(
					unitOfWork,
					storedEmail,
					"Служба уведомлений ЭДО",
					"Весёлый Водовоз",
					_options.Value.EmailForMailing,
					null,
					email,
					storedEmail.Subject,
					message);

				await unitOfWork.CommitAsync(cancellationToken);

				await _bus.Publish(sendEmailMessage, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось отправить email-уведомление ЭДО на адрес {Email}", email);
			}
		}
	}
}
