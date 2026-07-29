using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Mango;

namespace Mango.Employees.Library.Options
{
	public class ConfigureDriverMangoEmployeeRegistrationOptions : IConfigureOptions<DriverMangoEmployeeRegistrationOptions>
	{
		private readonly IMangoSettings _mangoSettings;

		public ConfigureDriverMangoEmployeeRegistrationOptions(IMangoSettings mangoSettings)
		{
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		public void Configure(DriverMangoEmployeeRegistrationOptions options)
		{
			options.Interval = _mangoSettings.DriverMangoEmployeeRegistrationInterval;
			options.AccessRoleId = _mangoSettings.DriverAccessRoleId;
			options.LineId = _mangoSettings.DriverLineId;
			options.DriversGroupId = _mangoSettings.DriversGroupId;
			options.ExtensionNumberPoolStart = _mangoSettings.DriverMangoExtensionNumberPoolStart;
			options.ExtensionNumberPoolEnd = _mangoSettings.DriverMangoExtensionNumberPoolEnd;
		}
	}
}
