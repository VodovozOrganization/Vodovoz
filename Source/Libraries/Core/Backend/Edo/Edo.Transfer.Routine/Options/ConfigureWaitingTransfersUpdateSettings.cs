﻿using System;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer.Routine.Options
{
	public class ConfigureWaitingTransfersUpdateSettings
	{
		private readonly IEdoTransferSettings _edoTransferSettings;

		public ConfigureWaitingTransfersUpdateSettings(IEdoTransferSettings edoTransferSettings)
		{
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
		}

		public void Configure(WaitingTransfersUpdateSettings settings)
		{
			settings.Interval = _edoTransferSettings.WaitingTransfersUpdateInterval;
		}
	}
}
