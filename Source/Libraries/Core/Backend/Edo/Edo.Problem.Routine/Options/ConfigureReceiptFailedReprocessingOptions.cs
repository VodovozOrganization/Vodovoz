using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Edo.Problem.Routine.Options
{
	/// <summary>
	/// Конфигуратор параметров переобработки чеков с ошибками фискализации из секции конфигурации
	/// </summary>
	public class ConfigureReceiptFailedReprocessingOptions : IConfigureOptions<ReceiptFailedReprocessingOptions>
	{
		private readonly IConfiguration _configuration;

		/// <summary>
		/// Конструктор конфигуратора параметров
		/// </summary>
		/// <param name="configuration">Конфигурация приложения</param>
		public ConfigureReceiptFailedReprocessingOptions(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		/// <summary>
		/// Привязка параметров из секции конфигурации
		/// </summary>
		/// <param name="options">Объект настроек</param>
		public void Configure(ReceiptFailedReprocessingOptions options)
		{
			var section = _configuration.GetSection(ReceiptFailedReprocessingOptions.SectionName);
			section.Bind(options);
		}
	}
}
