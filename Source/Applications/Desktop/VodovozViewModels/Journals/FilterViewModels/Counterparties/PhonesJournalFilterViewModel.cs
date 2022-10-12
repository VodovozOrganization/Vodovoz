using QS.Project.Filter;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties
{
	public class PhonesJournalFilterViewModel : FilterViewModelBase<PhonesJournalFilterViewModel>
	{
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;

		public PhonesJournalFilterViewModel()
		{
			
		}

		public PhonesJournalFilterViewModel(Counterparty counterparty, DeliveryPoint deliveryPoint = null): this()
		{
			_counterparty = counterparty ?? throw new ArgumentNullException(nameof(counterparty));
			_deliveryPoint = deliveryPoint;
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => UpdateFilterField(ref _deliveryPoint, value);
		}
	}
}
