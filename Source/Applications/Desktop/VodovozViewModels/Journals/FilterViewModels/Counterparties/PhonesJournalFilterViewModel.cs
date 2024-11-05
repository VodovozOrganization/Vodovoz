using QS.Project.Filter;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties
{
	public class PhonesJournalFilterViewModel : FilterViewModelBase<PhonesJournalFilterViewModel>
	{
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private bool _showArchive;

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		public bool ShowArchive
		{
			get => _showArchive;
			set => SetField(ref _showArchive, value);
		}
	}
}
