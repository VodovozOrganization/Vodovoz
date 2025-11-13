using System;
using Vodovoz.Settings.OnlineOrders;

namespace Vodovoz.Settings.Database.OnlineOrders
{
	public class OnlineOrderCancellationReasonSettings : IOnlineOrderCancellationReasonSettings
	{
		private readonly ISettingsController _settingsController;

		public OnlineOrderCancellationReasonSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetDuplicateOnlineOrderCancellationReasonId =>
			_settingsController.GetValue<int>("OnlineOrder.DuplicateCancellationReasonId");
	}
}
