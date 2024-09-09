using QS.DomainModel.Entity;
using QS.Project.Filter;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties
{
	public class PhonesJournalFilterViewModel : FilterViewModelBase<PhonesJournalFilterViewModel>
	{
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private bool _showArchive;
		private Employee _restrictEmployee;

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

		public Employee Employee
		{
			get => _restrictEmployee;
			set => SetField(ref _restrictEmployee, value);
		}

		[PropertyChangedAlso(nameof(CanChangeEmployee))]
		public Employee RestrictEmployee
		{
			get => _restrictEmployee;
			set
			{
				if(SetField(ref _restrictEmployee, value))
				{
					if(RestrictEmployee != null)
					{
						Employee = RestrictEmployee;
					}
				}
			}
		}

		public bool CanChangeEmployee => RestrictEmployee != null;
	}
}
