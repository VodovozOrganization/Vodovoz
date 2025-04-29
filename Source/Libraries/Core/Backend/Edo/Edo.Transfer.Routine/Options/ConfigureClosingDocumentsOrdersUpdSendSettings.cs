using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer.Routine.Options
{
	public class ConfigureClosingDocumentsOrdersUpdSendSettings : IConfigureOptions<ClosingDocumentsOrdersUpdSendSettings>
	{
		private readonly IEdoTransferSettings _edoTransferSettings;

		public ConfigureClosingDocumentsOrdersUpdSendSettings(IEdoTransferSettings edoTransferSettings)
		{
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public void Configure(ClosingDocumentsOrdersUpdSendSettings settings)
		{
			settings.Interval = _edoTransferSettings.ClosingDocumentsOrdersUpdSendInterval;
			settings.MaxDaysFromDeliveryDate = _edoTransferSettings.ClosingDocumentsOrdersUpdSendMaxDaysFromDeliveryDate;
		}
	}
}
