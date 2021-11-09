using System;
using QS.Commands;
using QS.Navigation;
using QS.Services;
using Vodovoz.Parameters;
using TabViewModelBase = QS.ViewModels.TabViewModelBase;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class GeneralSettingsViewModel : TabViewModelBase
	{
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;

		private string _routeListPrintedFormPhones;
		private DelegateCommand _saveCommand;
		
		public GeneralSettingsViewModel(
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			ICommonServices commonServices,
			INavigationManager navigation = null) : base(commonServices?.InteractiveService, navigation)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_generalSettingsParametersProvider =
				generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));

			TabName = "Общие настройки";

			RouteListPrintedFormPhones = _generalSettingsParametersProvider.GetRouteListPrintedFormPhones;
			CanEditRouteListPrintedFormPhones =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_route_List_printed_form_phones");
		}
		
		public bool CanEditRouteListPrintedFormPhones { get; }

		public string RouteListPrintedFormPhones
		{
			get => _routeListPrintedFormPhones;
			set => SetField(ref _routeListPrintedFormPhones, value);
		}
		public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(
			() =>
			{
				Save();
				Close(false, CloseSource.Save);
			})
		);

		private void Save()
		{
			_generalSettingsParametersProvider.UpdateRouteListPrintedFormPhones(RouteListPrintedFormPhones);
		}
	}
}
