using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class ClientBalanceFilter : FilterViewBase<ClientBalanceFilterViewModel>
	{
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter = new DeliveryPointJournalFilterViewModel();


		public ClientBalanceFilter(ClientBalanceFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			entryNomenclature.ViewModel = ViewModel.NomenclatureViewModel;

			entryClient.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(ViewModel.LifetimeScope));
			var dpFactory = ViewModel.DeliveryPointJournalFactory;
			dpFactory.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilter);
			evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			evmeDeliveryPoint.Changed += (sender, args) => ViewModel.Update();

			if(ViewModel.Journal.UserHaveAccessOnlyToWarehouseAndComplaints)
			{
				evmeDeliveryPoint.CanEditReference = false;
			}

			checkIncludeSold.Toggled += (s, e) => ViewModel.RestrictIncludeSold = checkIncludeSold.Active;
		}

		protected void OnSpeccomboStockItemSelected(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			ViewModel.Update();
		}

		protected void OnEntryClientChanged(object sender, EventArgs e)
		{
			evmeDeliveryPoint.Sensitive = ViewModel.RestrictCounterparty != null;
			if(ViewModel.RestrictCounterparty == null)
			{
				evmeDeliveryPoint.Subject = null;
			}
			else
			{
				evmeDeliveryPoint.Subject = null;
				_deliveryPointJournalFilter.Counterparty = entryClient.Subject as Counterparty;
			}
			ViewModel.Update();
		}

		protected void OnCheckIncludeSoldToggled(object sender, EventArgs e)
		{
			ViewModel.Update();
		}
	}
}
