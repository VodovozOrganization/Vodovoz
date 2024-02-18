using System;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Settings.Database.Delivery
{
	public class DeliveryScheduleSettings : IDeliveryScheduleSettings
	{
		private readonly ISettingsController _settingsController;

		public DeliveryScheduleSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int ClosingDocumentDeliveryScheduleId => _settingsController.GetIntValue("closing_document_delivery_schedule_id");
	}
}
