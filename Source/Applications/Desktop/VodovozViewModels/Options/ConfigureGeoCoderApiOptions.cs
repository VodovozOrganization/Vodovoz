using GeoCoderApi.Client.Options;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings;

namespace Vodovoz.ViewModels.Options
{
	public class ConfigureGeoCoderApiOptions : IConfigureOptions<GeoCoderApiOptions>
	{
		private readonly ISettingsController _settingsController;

		public ConfigureGeoCoderApiOptions(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public void Configure(GeoCoderApiOptions options)
		{
			options.BaseUri = _settingsController.GetValue<string>("GeoCoderApiOptions.BaseUri");
			options.ApiToken = _settingsController.GetValue<string>("GeoCoderApiOptions.ApiToken");
		}
	}
}
