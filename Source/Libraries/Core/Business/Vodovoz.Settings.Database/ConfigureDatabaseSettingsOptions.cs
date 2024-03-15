using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Vodovoz.Settings.Database;

namespace Vodovoz.Settings
{
	public class ConfigureDatabaseSettingsOptions<TSettings> : IConfigureOptions<TSettings>
		where TSettings : class
	{
		private readonly ISettingsController _settingsController;

		public ConfigureDatabaseSettingsOptions(ISettingsController settingsController)
		{
			_settingsController = settingsController
				?? throw new ArgumentNullException(nameof(settingsController));
		}

		public void Configure(TSettings options)
		{
			_settingsController.RefreshSettings();

			var settingsProperties = typeof(TSettings)
				.GetProperties()
				.Select(pi => pi.Name)
				.ToList();

			foreach(var property in settingsProperties)
			{
				var propertyType = options.GetType().GetProperty(property).PropertyType;

				object propertyValue = null;

				var methodDefinition = typeof(ISettingsController).GetMethod(nameof(_settingsController.GetType));

				string propertyName = string.Empty;

				if(_settingsController.ContainsSetting(property))
				{
					propertyName = property;
				}
				else if(_settingsController.ContainsSetting(property.FromPascalCaseToSnakeCase()))
				{
					propertyName = property.FromPascalCaseToSnakeCase();
				}

				propertyValue = methodDefinition.MakeGenericMethod(propertyType).Invoke(_settingsController, new[] { propertyName });

				typeof(TSettings)
					.GetProperty(property)
					.SetValue(options, propertyValue);
			}
		}
	}
}
