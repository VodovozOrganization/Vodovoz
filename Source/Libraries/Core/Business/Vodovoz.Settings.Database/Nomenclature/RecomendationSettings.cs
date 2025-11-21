using System;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Settings.Database.Nomenclature
{
	public class RecomendationSettings : IRecomendationSettings
	{
		private readonly TimeSpan _cacheTimeOut = TimeSpan.FromMinutes(5);
		private const string _sectionName = "Goods:Recomendations";
		private const string _robotCountKey = "RobotCount";
		private const string _operatorCountKey = "OperatorCount";
		private const string _ipzCountKey = "IpzCount";
		private readonly ISettingsController _settingsController;

		public RecomendationSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			InitializeSettings();
		}

		public int RobotCount => _settingsController.GetIntValue($"{_sectionName}:{_robotCountKey}");
		public int OperatorCount => _settingsController.GetIntValue($"{_sectionName}:{_operatorCountKey}");
		public int IpzCount => _settingsController.GetIntValue($"{_sectionName}:{_ipzCountKey}");

		public void SetRobotCount(int count)
		{
			_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_robotCountKey}", count.ToString(), _cacheTimeOut);
		}

		public void SetOperatorCount(int count)
		{
			_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_operatorCountKey}", count.ToString(), _cacheTimeOut);
		}

		public void SetIpzCount(int count)
		{
			_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_ipzCountKey}", count.ToString(), _cacheTimeOut);
		}

		private void InitializeSettings()
		{
			if(!_settingsController.ContainsSetting($"{_sectionName}:{_robotCountKey}"))
			{
				_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_robotCountKey}", "0", _cacheTimeOut);
			}
			if(!_settingsController.ContainsSetting($"{_sectionName}:{_operatorCountKey}"))
			{
				_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_operatorCountKey}", "0", _cacheTimeOut);
			}
			if(!_settingsController.ContainsSetting($"{_sectionName}:{_ipzCountKey}"))
			{
				_settingsController.CreateOrUpdateSetting($"{_sectionName}:{_ipzCountKey}", "0", _cacheTimeOut);
			}
		}
	}
}
