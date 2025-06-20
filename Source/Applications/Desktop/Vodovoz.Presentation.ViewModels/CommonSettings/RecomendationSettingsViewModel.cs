using QS.Commands;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Presentation.ViewModels.CommonSettings
{
	public class RecomendationSettingsViewModel : WidgetViewModelBase
	{
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IRecomendationSettings _recomendationSettings;
		private int _robotCount;
		private int _operatorCount;
		private int _ipzCount;
		private bool _canSave;

		public RecomendationSettingsViewModel(
			ICurrentPermissionService currentPermissionService,
			IRecomendationSettings recomendationSettings)
		{
			_currentPermissionService = currentPermissionService
				?? throw new ArgumentNullException(nameof(currentPermissionService));
			_recomendationSettings = recomendationSettings
				?? throw new ArgumentNullException(nameof(recomendationSettings));

			CanSave = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.RecomendationSettings.CanChangeSettings);

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

		public bool CanSave
		{
			get => _canSave;
			private set => SetField(ref _canSave, value);
		}

		public DelegateCommand SaveCommand { get; }

		private void Save()
		{
			_recomendationSettings.SetRobotCount(RobotCount);
			_recomendationSettings.SetOperatorCount(OperatorCount);
			_recomendationSettings.SetIpzCount(IpzCount);
		}
	}
}
