using System;
using DeliveryRulesService.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeliveryRulesService.Options
{
	/// <summary>
	/// Класс для добавления новых json форматеров
	/// </summary>
	public class ConfigureMvcJsonOptions : IConfigureOptions<MvcOptions>
	{
		private readonly string _jsonSettingsName;
		private readonly IOptionsMonitor<JsonOptions> _jsonOptions;
		private readonly ILoggerFactory _loggerFactory;

		public ConfigureMvcJsonOptions(
			string jsonSettingsName,
			IOptionsMonitor<JsonOptions> jsonOptions,
			ILoggerFactory loggerFactory
			)
		{
			_jsonSettingsName = jsonSettingsName ?? throw new ArgumentNullException(nameof(jsonSettingsName));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}
		
		/// <summary>
		/// Добавление новых форматеров
		/// </summary>
		/// <param name="options">Настройки</param>
		public void Configure(MvcOptions options)
		{
			var jsonOptions = _jsonOptions.Get(_jsonSettingsName);
			var inputLogger = _loggerFactory.CreateLogger<CustomSystemTextJsonInputFormatter>();
			
			options.InputFormatters.Insert(
				0,
				new CustomSystemTextJsonInputFormatter(
					_jsonSettingsName,
					jsonOptions,
					inputLogger));
			
			options.OutputFormatters.Insert(
				0,
				new CustomSystemTextJsonOutputFormatter(
					_jsonSettingsName,
					jsonOptions.JsonSerializerOptions));
		}
	}
}
