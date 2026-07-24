using System;
using System.Globalization;
using QS.Project.DB;
using Vodovoz.Settings.Mango;

namespace Vodovoz.Settings.Database.Mango
{
	public class MangoSettings : IMangoSettings
	{
		private const string _deactivationLastRunDateSettingName = "Mango.DriverMangoEmployeeDeactivationLastRunDate";

		private readonly ISettingsController _settingsController;
		private readonly IDataBaseInfo _dataBaseInfo;

		public MangoSettings(ISettingsController settingsController, IDataBaseInfo dataBaseInfo)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
			_dataBaseInfo = dataBaseInfo ?? throw new System.ArgumentNullException(nameof(dataBaseInfo));
		}

		public string ServiceHost
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.ServiceHost");
				}
				return _settingsController.GetStringValue("Mango.Work.ServiceHost");
			}
		}

		public uint ServicePort
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetValue<uint>("Mango.Test.ServicePort");
				}
				return _settingsController.GetValue<uint>("Mango.Work.ServicePort");
			}
		}

		public string VpbxApiKey
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.VpbxApiKey");
				}
				return _settingsController.GetStringValue("Mango.Work.VpbxApiKey");
			}
		}

		public string VpbxApiSalt
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.VpbxApiSalt");
				}
				return _settingsController.GetStringValue("Mango.Work.VpbxApiSalt");
			}
		}

		public bool MangoEnabled
		{
			get
			{
				var workDatabase = _settingsController.GetStringValue("Mango.Work.Database");
				var workMode = workDatabase == _dataBaseInfo.Name;
				return workMode || TestMode;
			}
		}

		public bool TestMode
		{
			get
			{
				var testDatabase = _settingsController.GetStringValue("Mango.Test.Database");
				return testDatabase == _dataBaseInfo.Name;
			}
		}

		public int GrpcKeepAliveTimeMs => _settingsController.GetIntValue("Mango.Grpc.KeepAliveTimeMs");
		public int GrpcKeepAliveTimeoutMs => _settingsController.GetIntValue("Mango.Grpc.KeepAliveTimeoutMs");
		public bool GrpcKeepAlivePermitWithoutCalls => _settingsController.GetBoolValue("Mango.Grpc.KeepAlivePermitWithoutCalls");
		public int GrpcMaxPingWithoutData => _settingsController.GetIntValue("Mango.Grpc.MaxPingWithoutData");

		public string DriversCallsLineNumber
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.DriversCallsLineNumber");
				}
				return _settingsController.GetStringValue("Mango.Work.DriversCallsLineNumber");
			}
		}

		public string WebhookCallsUrl
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.WebhookCallsUrl");
				}
				return _settingsController.GetStringValue("Mango.Work.WebhookCallsUrl");
			}
		}

		public string VpbxApiUrl
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.VpbxApiUrl");
				}
				return _settingsController.GetStringValue("Mango.Work.VpbxApiUrl");
			}
		}

		public string DriverAccessRoleId
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.DriverAccessRoleId");
				}
				return _settingsController.GetStringValue("Mango.Work.DriverAccessRoleId");
			}
		}

		public string DriverLineId
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.DriverLineId");
				}
				return _settingsController.GetStringValue("Mango.Work.DriverLineId");
			}
		}

		public string DriversGroupId
		{
			get
			{
				if(TestMode)
				{
					return _settingsController.GetStringValue("Mango.Test.DriversGroupId");
				}
				return _settingsController.GetStringValue("Mango.Work.DriversGroupId");
			}
		}

		public int DriverMangoExtensionNumberPoolStart =>
			_settingsController.GetValue<int>("Mango.DriverMangoExtensionNumberPoolStart");

		public int DriverMangoExtensionNumberPoolEnd =>
			_settingsController.GetValue<int>("Mango.DriverMangoExtensionNumberPoolEnd");

		public bool DriverMangoEmployeeRegistrationWorkerEnabled =>
			_settingsController.GetBoolValue("Mango.DriverMangoEmployeeRegistrationWorkerEnabled");

		public bool DriverMangoEmployeeDeactivationWorkerEnabled =>
			_settingsController.GetBoolValue("Mango.DriverMangoEmployeeDeactivationWorkerEnabled");

		public TimeSpan DriverMangoEmployeeRegistrationInterval =>
			_settingsController.GetValue<TimeSpan>("Mango.DriverMangoEmployeeRegistrationInterval");

		public TimeSpan DriverMangoEmployeeDeactivationInterval =>
			_settingsController.GetValue<TimeSpan>("Mango.DriverMangoEmployeeDeactivationInterval");

		public TimeSpan DriverMangoEmployeeDeactivationRunTime =>
			_settingsController.GetValue<TimeSpan>("Mango.DriverMangoEmployeeDeactivationRunTime");

		public DateTime DriverMangoEmployeeDeactivationLastRunDate =>
			_settingsController.GetDateTimeValue(_deactivationLastRunDateSettingName, CultureInfo.InvariantCulture);

		public void UpdateDriverMangoEmployeeDeactivationLastRunDate(DateTime lastRunDate)
		{
			_settingsController.CreateOrUpdateSetting(
				_deactivationLastRunDateSettingName,
				lastRunDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
		}
	}
}
