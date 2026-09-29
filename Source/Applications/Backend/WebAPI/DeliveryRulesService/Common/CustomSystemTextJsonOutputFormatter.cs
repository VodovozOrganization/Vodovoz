using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace DeliveryRulesService.Common
{
	public class CustomSystemTextJsonOutputFormatter : SystemTextJsonOutputFormatter
	{
		public CustomSystemTextJsonOutputFormatter(
			string settingsName,
			JsonSerializerOptions jsonSerializerOptions
		) : base(jsonSerializerOptions)
		{
			SettingsName = settingsName;
		}

		/// <summary>
		/// Название применимых настроек
		/// </summary>
		public string SettingsName { get; }

		public override bool CanWriteResult(OutputFormatterCanWriteContext context)
		{
			if(context.HttpContext.GetJsonSettingsName() != SettingsName)
			{
				return false;
			}

			return base.CanWriteResult(context);
		}
	}
}
