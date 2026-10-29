using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace DeliveryRulesService.Common
{
	public class CustomSystemTextJsonInputFormatter : SystemTextJsonInputFormatter
	{
		public CustomSystemTextJsonInputFormatter(
			string settingsName,
			JsonOptions options,
			ILogger<CustomSystemTextJsonInputFormatter> logger
		) : base(options, logger)
		{
			SettingsName = settingsName;
		}

		/// <summary>
		/// Название применимых настроек
		/// </summary>
		public string SettingsName { get; }

		public override bool CanRead(InputFormatterContext context)
		{
			if(context.HttpContext.GetJsonSettingsName() != SettingsName)
			{
				return false;
			}

			return base.CanRead(context);
		}
	}
}
