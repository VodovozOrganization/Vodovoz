using System;
using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// Заполняет интервалы уведомлений о зависших чеках из параметров БД.
	/// </summary>
	public class ConfigureReceiptQueueNotificationOptions : IConfigureOptions<ReceiptQueueNotificationOptions>
	{
		private readonly IEdoReceiptSettings _receiptSettings;

		/// <summary>
		/// Создаёт настройку интервалов обработки очереди чеков.
		/// </summary>
		/// <param name="receiptSettings">Настройки чеков ЭДО</param>
		public ConfigureReceiptQueueNotificationOptions(IEdoReceiptSettings receiptSettings)
		{
			_receiptSettings = receiptSettings ?? throw new ArgumentNullException(nameof(receiptSettings));
		}

		/// <inheritdoc/>
		public void Configure(ReceiptQueueNotificationOptions options)
		{
			options.WorkerInterval = _receiptSettings.ReceiptQueueNotificationWorkerInterval;
			options.RepeatInterval = _receiptSettings.ReceiptQueueNotificationRepeatInterval;
			options.LookbackDays = _receiptSettings.ReceiptQueueNotificationLookbackDays;
		}
	}
}
