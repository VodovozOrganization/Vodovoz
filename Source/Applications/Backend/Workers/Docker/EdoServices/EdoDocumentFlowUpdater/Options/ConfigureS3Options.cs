using System;
using Microsoft.Extensions.Options;
using Vodovoz.Application.Options;
using Vodovoz.Settings;

namespace EdoDocumentFlowUpdater.Options
{
	public class ConfigureS3Options : IConfigureOptions<S3Options>
	{
		private readonly ISettingsController _settingsController;

		public ConfigureS3Options(ISettingsController settingsController)
		{
			_settingsController = settingsController
				?? throw new ArgumentNullException(nameof(settingsController));
		}

		public void Configure(S3Options options)
		{
			options.ServiceUrl = _settingsController.GetValue<string>("S3.ServiceUrl");
			options.SecretKey = _settingsController.GetValue<string>("S3.SecretKey");
			options.AccessKey = _settingsController.GetValue<string>("S3.AccessKey");
		}
	}
}
