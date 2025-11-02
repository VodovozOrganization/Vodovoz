using QS.Commands;
using QS.Tdi;
using QS.ViewModels;
using System;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using VodovozBusiness.Controllers;

namespace Vodovoz.ViewModels.ViewModels.SidePanels
{
	public class EdoLightsMatrixPanelViewModel : UoWWidgetViewModelBase
	{
		private readonly ICounterpartyEdoAccountController _counterpartyEdoAccountController;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ITdiTab _tdiTab;
		private DelegateCommand<int> _openEdoTabInCounterparty;

		public EdoLightsMatrixPanelViewModel(
			EdoLightsMatrixViewModel edoLightsMatrixViewModel,
			ICounterpartyEdoAccountController counterpartyEdoAccountController,
			IGtkTabsOpener gtkTabsOpener,
			ITdiTab tdiTab = null)
		{
			EdoLightsMatrixViewModel = edoLightsMatrixViewModel ?? throw new ArgumentNullException(nameof(edoLightsMatrixViewModel));
			_counterpartyEdoAccountController = counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_tdiTab = tdiTab;
		}

		public EdoLightsMatrixViewModel EdoLightsMatrixViewModel { get; }

		public void Refresh(Domain.Client.Counterparty counterparty, int? organizationId)
		{
			var currentEdoAccount =
				_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(counterparty, organizationId);
			
			EdoLightsMatrixViewModel.RefreshLightsMatrix(currentEdoAccount);
		}

		public DelegateCommand<int> OpenEdoTabInCounterparty =>
			_openEdoTabInCounterparty ?? (_openEdoTabInCounterparty = new DelegateCommand<int>(counterpartyId =>
				{
					_gtkTabsOpener.OpenCounterpartyEdoTab(counterpartyId, _tdiTab);
				}));
	}
}
