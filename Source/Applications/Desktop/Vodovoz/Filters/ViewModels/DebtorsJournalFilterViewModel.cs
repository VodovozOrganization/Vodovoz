using System;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Filters.ViewModels
{
	public class DebtorsJournalFilterViewModel : FilterViewModelBase<DebtorsJournalFilterViewModel>
	{
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
		private bool _showSuspendedCounterparty;
		private bool _showCancellationCounterparty;
		private DebtorsTaskStatus? _debtorsTaskStatus;
		private DiscountReason _discountReason;
		private Nomenclature _lastOrderNomenclature;
		private DeliveryPointCategory _selectedDeliveryPointCategory;
		private IEnumerable<DeliveryPointCategory> _deliveryPointCategories;
		private IEntityAutocompleteSelectorFactory _counterpartySelectorFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;
		private IEntityAutocompleteSelectorFactory _deliveryPointSelectorFactory;


		public DebtorsJournalFilterViewModel()
		{
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

		public Counterparty Client {
			get => _client;
			set => SetField(ref _client, value, () => Client);
		}

		public DeliveryPoint Address {
			get => _address;
			set => SetField(ref _address, value, () => Address);
		}

		public PersonType? OPF {
			get => _opf;
			set => SetField(ref _opf, value, () => OPF);
		}

		public DateTime? StartDate {
			get => _startDate;
			set => SetField(ref _startDate, value, () => StartDate);
		}

		[PropertyChangedAlso(nameof(ShowHideActiveCheck))]
		public DateTime? EndDate {
			get => _endDate;
			set => SetField(ref _endDate, value, () => EndDate);
		}

		public bool ShowHideActiveCheck => EndDate != null;

		public bool HideActiveCounterparty {
			get => _hideActiveCounterparty;
			set => SetField(ref _hideActiveCounterparty, value, () => HideActiveCounterparty);
		}
		
		[PropertyChangedAlso(nameof(ShowCancellationCounterparty))]
		public bool ShowSuspendedCounterparty {
			get => _showSuspendedCounterparty;
			set => SetField(ref _showSuspendedCounterparty, value);
		}
		
		[PropertyChangedAlso(nameof(ShowSuspendedCounterparty))]
		public bool ShowCancellationCounterparty {
			get => _showCancellationCounterparty;
			set => SetField(ref _showCancellationCounterparty, value);
		}

		public bool? WithOneOrder {
			get => _withOneOrder;
			set => UpdateFilterField(ref _withOneOrder, value);
		}

		public bool HideWithoutEmail
		{
			get => _hideWithoutEmail;
			set => UpdateFilterField(ref _hideWithoutEmail, value);
		}

		public int? DebtBottlesFrom {
			get => _debtBottlesFrom;
			set => SetField(ref _debtBottlesFrom, value, () => DebtBottlesFrom);
		}

		public int? DebtBottlesTo {
			get => _debtBottlesTo;
			set => SetField(ref _debtBottlesTo, value, () => DebtBottlesTo);
		}

		public int? LastOrderBottlesFrom {
			get => _lastOrderBottlesFrom;
			set => SetField(ref _lastOrderBottlesFrom, value, () => LastOrderBottlesFrom);
		}

		public int? LastOrderBottlesTo {
			get => _lastOrderBottlesTo;
			set => SetField(ref _lastOrderBottlesTo, value, () => LastOrderBottlesTo);
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

		public Nomenclature LastOrderNomenclature {
			get => _lastOrderNomenclature;
			set => SetField(ref _lastOrderNomenclature, value, () => LastOrderNomenclature);
		}

		public DiscountReason DiscountReason {
			get => _discountReason;
			set => SetField(ref _discountReason, value, () => DiscountReason);
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
				new DeliveryPointJournalFactory(DeliveryPointJournalFilterViewModel)
					.CreateDeliveryPointAutocompleteSelectorFactory());

		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory =>
			_counterpartySelectorFactory ?? (_counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
					CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices));

		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory =>
			_nomenclatureSelectorFactory ?? (_nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope()),
					new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())), new UserRepository()));
	}

	public enum DebtorsTaskStatus
	{
		[Display(Name = "Да")]
		HasTask,
		[Display(Name = "Нет")]
		WithoutTask
	}
}
