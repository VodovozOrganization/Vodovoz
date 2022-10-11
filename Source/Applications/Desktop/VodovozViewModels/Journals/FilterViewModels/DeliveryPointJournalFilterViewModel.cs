using System;
using QS.Project.Filter;
using Vodovoz.Domain.Client;

namespace Vodovoz.Filters.ViewModels
{
	public class DeliveryPointJournalFilterViewModel : FilterViewModelBase<DeliveryPointJournalFilterViewModel>
	{
		private bool restrictOnlyActive;
		public virtual bool RestrictOnlyActive {
			get => restrictOnlyActive;
			set => UpdateFilterField(ref restrictOnlyActive, value, () => RestrictOnlyActive);
		}

		private Counterparty counterparty;
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => UpdateFilterField(ref counterparty, value, () => Counterparty);
		}

		private bool restrictOnlyNotFoundOsm;
		public virtual bool RestrictOnlyNotFoundOsm {
			get => restrictOnlyNotFoundOsm;
			set => UpdateFilterField(ref restrictOnlyNotFoundOsm, value, () => RestrictOnlyNotFoundOsm);
		}

		private bool restrictOnlyWithoutStreet;
		public virtual bool RestrictOnlyWithoutStreet {
			get => restrictOnlyWithoutStreet;
			set => UpdateFilterField(ref restrictOnlyWithoutStreet, value, () => RestrictOnlyWithoutStreet);
		}
	}
}
