using QS.Commands;
using QS.Tdi;
using QS.ViewModels;
using System;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;

namespace Vodovoz.ViewModels.ViewModels.SidePanels
{
	public class EdoLightsMatrixPanelViewModel : UoWWidgetViewModelBase
	{

		private  EdoLightsMatrixViewModel _edoLightsMatrixViewModel;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ITdiTab _tdiTab;
		private DelegateCommand<int> _openEdoTabInCounterparty;

		public EdoLightsMatrixPanelViewModel(EdoLightsMatrixViewModel edoLightsMatrixViewModel, IGtkTabsOpener gtkTabsOpener, ITdiTab tdiTab = null)
		{
			_edoLightsMatrixViewModel = edoLightsMatrixViewModel ?? throw new ArgumentNullException(nameof(edoLightsMatrixViewModel));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_tdiTab = tdiTab;
		}

		public EdoLightsMatrixViewModel EdoLightsMatrixViewModel => _edoLightsMatrixViewModel;

		public void Refresh(Domain.Client.Counterparty counterparty)
		{
			EdoLightsMatrixViewModel.RefreshLightsMatrix(counterparty);
		}

		public DelegateCommand<int> OpenEdoTabInCounterparty =>
			_openEdoTabInCounterparty ?? (_openEdoTabInCounterparty = new DelegateCommand<int>(counterpartyId =>
				{
					_gtkTabsOpener.OpenCounterpartyEdoTab(counterpartyId, _tdiTab);
				}));
	}
}
