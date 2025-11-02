using QS.Project.DB;
using Vodovoz.Settings.Mango;

namespace Vodovoz.Settings.Database.Mango
{
	public class MangoSettings : IMangoSettings
	{
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
	}
}
