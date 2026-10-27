using System;
using Microsoft.Extensions.Options;
using Vodovoz.Core.Application.Options;
using Vodovoz.Settings;

namespace Vodovoz.Infrastructure.FileStorage.Options
{
	/// <summary>
	/// Настраивает доступ к S3 по настройкам приложения в БД.
	/// </summary>
	public class ConfigureS3Options : IConfigureOptions<S3Options>
	{
		private readonly ISettingsController _settingsController;

		/// <summary>
		/// Создает конфигуратор S3.
		/// </summary>
		/// <param name="settingsController">Источник настроек приложения.</param>
		public ConfigureS3Options(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		/// <summary>
		/// Заполняет параметры подключения к S3.
		/// </summary>
		/// <param name="options">Параметры S3.</param>
		public void Configure(S3Options options)
		{
			options.ServiceUrl = _settingsController.GetValue<string>("S3.ServiceUrl");
			options.SecretKey = _settingsController.GetValue<string>("S3.SecretKey");
			options.AccessKey = _settingsController.GetValue<string>("S3.AccessKey");
		}
	}
}
