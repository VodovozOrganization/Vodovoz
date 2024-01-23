using Autofac;
using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.Filters.ViewModels
{
	public class DebtorsJournalFilterViewModel : FilterViewModelBase<DebtorsJournalFilterViewModel>
	{
		private ILifetimeScope _lifetimeScope;
		private Counterparty _client;
		private DeliveryPoint _address;
		private PersonType? _opf;
		private DateTime? _endDate;
		private DateTime? _startDate;
		private int? _debtBottlesTo;
		private int? _debtBottlesFrom;
		private bool? _withOneOrder;
		private int? _lastOrderBottlesTo;
		private int? _lastOrderBottlesFrom;
		private int? _deliveryPointsTo;
		private int? _deliveryPointsFrom;
		private bool _hideActiveCounterparty;
		private bool _hideWithoutEmail;
		private bool _hideWithoutFixedPrices = false;
		private bool _showSuspendedCounterparty;
		private bool _showCancellationCounterparty;
		private DebtorsTaskStatus? _debtorsTaskStatus;
		private DiscountReason _discountReason;
		private Nomenclature _lastOrderNomenclature;
		private DeliveryPointCategory _selectedDeliveryPointCategory;
		private IEnumerable<DeliveryPointCategory> _deliveryPointCategories;
		private IEntityAutocompleteSelectorFactory _counterpartySelectorFactory;
		private IEntityAutocompleteSelectorFactory _deliveryPointSelectorFactory;

		public DebtorsJournalFilterViewModel(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			UpdateWith(
				x => x.Client,
				x => x.Address,
				x => x.OPF,
				x => x.StartDate,
				x => x.EndDate,
				x => x.DebtBottlesFrom,
				x => x.DebtBottlesTo,
				x => x.LastOrderBottlesFrom,
				x => x.LastOrderBottlesTo,
				x => x.LastOrderNomenclature,
				x => x.DiscountReason,
				x => x.HideActiveCounterparty,
				x => x.ShowSuspendedCounterparty,
				x => x.ShowCancellationCounterparty,
				x => x.DebtorsTaskStatus
			);
		}

		public Counterparty Client
		{
			get => _client;
			set => SetField(ref _client, value);
		}

		public DeliveryPoint Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		public PersonType? OPF
		{
			get => _opf;
			set => SetField(ref _opf, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(ShowHideActiveCheck))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool ShowHideActiveCheck => EndDate != null;

		public bool HideActiveCounterparty
		{
			get => _hideActiveCounterparty;
			set => SetField(ref _hideActiveCounterparty, value);
		}

		[PropertyChangedAlso(nameof(ShowCancellationCounterparty))]
		public bool ShowSuspendedCounterparty
		{
			get => _showSuspendedCounterparty;
			set => SetField(ref _showSuspendedCounterparty, value);
		}

		[PropertyChangedAlso(nameof(ShowSuspendedCounterparty))]
		public bool ShowCancellationCounterparty
		{
			get => _showCancellationCounterparty;
			set => SetField(ref _showCancellationCounterparty, value);
		}

		public bool? WithOneOrder
		{
			get => _withOneOrder;
			set => UpdateFilterField(ref _withOneOrder, value);
		}

		public bool HideWithoutEmail
		{
			get => _hideWithoutEmail;
			set => UpdateFilterField(ref _hideWithoutEmail, value);
		}

		public bool HideWithoutFixedPrices
		{
			get => _hideWithoutFixedPrices;
			set => UpdateFilterField(ref _hideWithoutFixedPrices, value);
		}

		public int? DebtBottlesFrom
		{
			get => _debtBottlesFrom;
			set => SetField(ref _debtBottlesFrom, value);
		}

		public int? DebtBottlesTo
		{
			get => _debtBottlesTo;
			set => SetField(ref _debtBottlesTo, value);
		}

		public int? LastOrderBottlesFrom
		{
			get => _lastOrderBottlesFrom;
			set => SetField(ref _lastOrderBottlesFrom, value);
		}

		public int? LastOrderBottlesTo
		{
			get => _lastOrderBottlesTo;
			set => SetField(ref _lastOrderBottlesTo, value);
		}

		public int? DeliveryPointsFrom
		{
			get => _deliveryPointsFrom;
			set => UpdateFilterField(ref _deliveryPointsFrom, value);
		}

		public int? DeliveryPointsTo
		{
			get => _deliveryPointsTo;
			set => UpdateFilterField(ref _deliveryPointsTo, value);
		}

		public Nomenclature LastOrderNomenclature
		{
			get => _lastOrderNomenclature;
			set => SetField(ref _lastOrderNomenclature, value);
		}

		public DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}

		public DeliveryPointCategory SelectedDeliveryPointCategory
		{
			get => _selectedDeliveryPointCategory;
			set => UpdateFilterField(ref _selectedDeliveryPointCategory, value);
		}

		public DebtorsTaskStatus? DebtorsTaskStatus
		{
			get => _debtorsTaskStatus;
			set => UpdateFilterField(ref _debtorsTaskStatus, value);
		}

		public IEnumerable<DeliveryPointCategory> DeliveryPointCategories =>
		 _deliveryPointCategories ?? (_deliveryPointCategories = UoW.GetAll<DeliveryPointCategory>());

		public DeliveryPointJournalFilterViewModel DeliveryPointJournalFilterViewModel { get; set; }
			= new DeliveryPointJournalFilterViewModel();

		public virtual IEntityAutocompleteSelectorFactory DeliveryPointSelectorFactory =>
			_deliveryPointSelectorFactory ?? (_deliveryPointSelectorFactory =
				_lifetimeScope.Resolve<IDeliveryPointJournalFactory>(new TypedParameter(typeof(DeliveryPointJournalFilterViewModel), DeliveryPointJournalFilterViewModel))
					.CreateDeliveryPointAutocompleteSelectorFactory());

		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory =>
			_counterpartySelectorFactory ?? (_counterpartySelectorFactory =
				_lifetimeScope.Resolve<ICounterpartyJournalFactory>().CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
