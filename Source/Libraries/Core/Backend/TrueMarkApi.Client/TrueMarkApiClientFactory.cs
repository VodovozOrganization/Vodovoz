﻿using System;
using Vodovoz.Settings.Edo;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClientFactory
	{
		private readonly IEdoSettings _edoSettings;

		public TrueMarkApiClientFactory(IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
		}

		public TrueMarkApiClient GetClient()
		{
			return new TrueMarkApiClient(_edoSettings.TrueMarkApiBaseUrl, _edoSettings.TrueMarkApiToken);
		}
	}
}
