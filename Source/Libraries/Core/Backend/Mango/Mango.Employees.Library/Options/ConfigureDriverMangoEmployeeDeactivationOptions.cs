using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Mango;

namespace Mango.Employees.Library.Options
{
	public class ConfigureDriverMangoEmployeeDeactivationOptions : IConfigureOptions<DriverMangoEmployeeDeactivationOptions>
	{
		private readonly IMangoSettings _mangoSettings;

		public ConfigureDriverMangoEmployeeDeactivationOptions(IMangoSettings mangoSettings)
		{
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		public void Configure(DriverMangoEmployeeDeactivationOptions options)
		{
			options.Interval = _mangoSettings.DriverMangoEmployeeDeactivationInterval;
			options.RunTime = _mangoSettings.DriverMangoEmployeeDeactivationRunTime;
			options.ExtensionNumberPoolStart = _mangoSettings.DriverMangoExtensionNumberPoolStart;
			options.ExtensionNumberPoolEnd = _mangoSettings.DriverMangoExtensionNumberPoolEnd;
		}
	}
}
