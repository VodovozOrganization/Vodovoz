using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
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
				.ToList();

			foreach(var propertyDefinition in settingsProperties)
			{
				var propertyType = options.GetType().GetProperty(propertyDefinition.Name).PropertyType;

				object propertyValue = null;

				var methodDefinition = _settingsController.GetType().GetMethod(nameof(_settingsController.GetValue));

				string propertyName = string.Empty;

				var customPropertyNameAttribute = propertyDefinition.GetCustomAttribute<CustomSettingPropertyNameAttribute>();

				if(customPropertyNameAttribute != null)
				{
					propertyName = customPropertyNameAttribute.CustomPropertyName;
				}
				else if(_settingsController.ContainsSetting(propertyDefinition.Name))
				{
					propertyName = propertyDefinition.Name;
				}
				else if(_settingsController.ContainsSetting(propertyDefinition.Name.FromPascalCaseToSnakeCase()))
				{
					propertyName = propertyDefinition.Name.FromPascalCaseToSnakeCase();
				}

				propertyValue = methodDefinition.MakeGenericMethod(propertyType).Invoke(_settingsController, new[] { propertyName });

				typeof(TSettings)
					.GetProperty(propertyDefinition.Name)
					.SetValue(options, propertyValue);
			}
		}
	}
}
