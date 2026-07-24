using EdoNotifications.Application.Providers;
using EdoNotifications.Contracts;
using EdoNotificationsWorker.Services.Bitrix;
using EdoNotificationsWorker.Services.Email;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vodovoz.Extensions;

namespace EdoNotificationsWorker
{
	public class EdoNotificationsConsumer : IConsumer<EdoNotificationMessage>
	{
		private readonly IEdoNotificationsSettingsProvider _edoNotificationsSettingsProvider;
		private readonly IEdoNotificationEmailService _edoNotificationEmailService;
		private readonly IEdoNotificationBitrixService _edoNotificationBitrixService;
		private readonly ILogger<EdoNotificationsConsumer> _logger;

		public EdoNotificationsConsumer(
			IEdoNotificationsSettingsProvider edoNotificationsSettingsProvider,
			IEdoNotificationEmailService edoNotificationEmailService,
			IEdoNotificationBitrixService edoNotificationBitrixService,
			ILogger<EdoNotificationsConsumer> logger)
		{
			_edoNotificationsSettingsProvider = edoNotificationsSettingsProvider ?? throw new ArgumentNullException(nameof(edoNotificationsSettingsProvider));
			_edoNotificationEmailService = edoNotificationEmailService ?? throw new ArgumentNullException(nameof(edoNotificationEmailService));
			_edoNotificationBitrixService = edoNotificationBitrixService ?? throw new ArgumentNullException(nameof(edoNotificationBitrixService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<EdoNotificationMessage> context)
		{
			var edoNotification = context.Message;
			var edoNotificationSetting = _edoNotificationsSettingsProvider.GetEdoNotificationSetting(edoNotification);

			if(edoNotificationSetting == null)
			{
				throw new ArgumentNullException(nameof(edoNotificationSetting));
			}

			var resultText = edoNotificationSetting.Template;

			foreach(var param in edoNotification.TemplateParams)
			{
				var placeholder = "{" + param.Key + "}";
				var value = param.Value ?? string.Empty;

				resultText = Regex.Replace(
					resultText,
					Regex.Escape(placeholder),
					value,
					RegexOptions.IgnoreCase
				);
			}

			await _edoNotificationBitrixService.NotifyAsync(edoNotificationSetting.BitrixDialogs, resultText, context.CancellationToken);

			var emailSubject = $"Уведомление об ЭДО - {edoNotification.EdoNotificationType.GetEnumDisplayName()}";
			await _edoNotificationEmailService.NotifyAsync(edoNotificationSetting.Emails, emailSubject, resultText, context.CancellationToken);
		}
	}
}
