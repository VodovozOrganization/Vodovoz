using System;
using QS.Project.Filter;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Widgets.Search;

namespace Vodovoz.Filters.ViewModels
{
	public class DeliveryPointJournalFilterViewModel : FilterViewModelBase<DeliveryPointJournalFilterViewModel>
	{
		private bool _restrictOnlyActive;
		private Counterparty _counterparty;
		private bool _restrictOnlyNotFoundOsm;
		private bool _restrictOnlyWithoutStreet;
		private int? _restrictDeliveryPointId;
		private string _restrictCounterpartyNameLike;
		private readonly CompositeSearchViewModel _searchByAddressViewModel;

		public DeliveryPointJournalFilterViewModel()
		{
			_searchByAddressViewModel = new CompositeSearchViewModel();
			_searchByAddressViewModel.OnSearch += OnSearchByAddressViewModel;
		}

		public CompositeSearchViewModel SearchByAddressViewModel => _searchByAddressViewModel;

		public virtual bool RestrictOnlyActive {
			get => _restrictOnlyActive;
			set => UpdateFilterField(ref _restrictOnlyActive, value, () => RestrictOnlyActive);
		}

		public virtual Counterparty Counterparty {
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value, () => Counterparty);
		}

		public virtual bool RestrictOnlyNotFoundOsm {
			get => _restrictOnlyNotFoundOsm;
			set => UpdateFilterField(ref _restrictOnlyNotFoundOsm, value, () => RestrictOnlyNotFoundOsm);
		}

		public virtual bool RestrictOnlyWithoutStreet {
			get => _restrictOnlyWithoutStreet;
			set => UpdateFilterField(ref _restrictOnlyWithoutStreet, value, () => RestrictOnlyWithoutStreet);
		}

		public virtual int? RestrictDeliveryPointId
		{
			get => _restrictDeliveryPointId;
			set => SetField(ref _restrictDeliveryPointId, value);
		}

		public virtual string RestrictCounterpartyNameLike
		{
			get => _restrictCounterpartyNameLike;
			set => SetField(ref _restrictCounterpartyNameLike, value);
		}

		private void OnSearchByAddressViewModel(object sender, EventArgs e)
		{
			Update();
		}

		public override void Dispose()
		{
			_searchByAddressViewModel.OnSearch -= OnSearchByAddressViewModel;
			base.Dispose();
		}
	}
}
