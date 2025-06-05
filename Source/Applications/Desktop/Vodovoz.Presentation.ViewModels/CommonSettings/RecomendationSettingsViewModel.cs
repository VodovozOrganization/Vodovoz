using QS.Commands;
using QS.ViewModels;
using System;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Presentation.ViewModels.CommonSettings
{
	public class RecomendationSettingsViewModel : WidgetViewModelBase
	{
		private readonly IRecomendationSettings _recomendationSettings;
		private int _robotCount;
		private int _operatorCount;
		private int _ipzCount;

		public RecomendationSettingsViewModel(IRecomendationSettings recomendationSettings)
		{
			_recomendationSettings = recomendationSettings ?? throw new ArgumentNullException(nameof(recomendationSettings));

			_robotCount = _recomendationSettings.RobotCount;
			_operatorCount = _recomendationSettings.OperatorCount;
			_ipzCount = _recomendationSettings.IpzCount;

			SaveCommand = new DelegateCommand(Save, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);
		}

		public int RobotCount
		{
			get => _robotCount;
			set => SetField(ref _robotCount, value);
		}

		public int OperatorCount
		{
			get => _operatorCount;
			set => SetField(ref _operatorCount, value);
		}

		public int IpzCount
		{
			get => _ipzCount;
			set => SetField(ref _ipzCount, value);
		}

		public bool CanSave => true;

		public DelegateCommand SaveCommand { get; }

		private void Save()
		{
			_recomendationSettings.SetRobotCount(RobotCount);
			_recomendationSettings.SetOperatorCount(OperatorCount);
			_recomendationSettings.SetIpzCount(IpzCount);
		}
	}
}
