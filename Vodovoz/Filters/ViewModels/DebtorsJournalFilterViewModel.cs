using System;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.RepresentationModel.GtkUI;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModel;
using Vodovoz.FilterViewModels.Goods;
using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.Filters.ViewModels
{
	public class DebtorsJournalFilterViewModel : FilterViewModelBase<DebtorsJournalFilterViewModel>, IJournalFilter
	{
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
				x => x.HideActiveCounterparty
			);
		}

		private Counterparty client;
		public Counterparty Client {
			get => client;
			set => SetField(ref client, value, () => Client);
		}

		private DeliveryPoint address;
		public DeliveryPoint Address {
			get => address;
			set => SetField(ref address, value, () => Address);
		}

		private PersonType? opf;
		public PersonType? OPF {
			get => opf;
			set => SetField(ref opf, value, () => OPF);
		}

		private DateTime? startDate;
		public DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(ShowHideActiveCheck))]
		public DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}

		public bool ShowHideActiveCheck { get { return EndDate != null; } }

		private bool hideActiveCounterparty;
		public bool HideActiveCounterparty {
			get => hideActiveCounterparty;
			set => SetField(ref hideActiveCounterparty, value, () => HideActiveCounterparty);
		}

		private bool hideWithOneOrder;
		public bool HideWithOneOrder {
			get => hideWithOneOrder;
			set => UpdateFilterField(ref hideWithOneOrder, value);
		}

		private int? debtBottlesFrom;
		public int? DebtBottlesFrom {
			get => debtBottlesFrom;
			set => SetField(ref debtBottlesFrom, value, () => DebtBottlesFrom);
		}

		private int? debtBottlesTo;
		public int? DebtBottlesTo {
			get => debtBottlesTo;
			set => SetField(ref debtBottlesTo, value, () => DebtBottlesTo);
		}

		private int? lastOrderBottlesFrom;
		public int? LastOrderBottlesFrom {
			get => lastOrderBottlesFrom;
			set => SetField(ref lastOrderBottlesFrom, value, () => LastOrderBottlesFrom);
		}

		private int? lastOrderBottlesTo;
		public int? LastOrderBottlesTo {
			get => lastOrderBottlesTo;
			set => SetField(ref lastOrderBottlesTo, value, () => LastOrderBottlesTo);
		}

		private Nomenclature lastOrderNomenclature;
		public Nomenclature LastOrderNomenclature {
			get => lastOrderNomenclature;
			set => SetField(ref lastOrderNomenclature, value, () => LastOrderNomenclature);
		}

		private DiscountReason discountReason;
		public DiscountReason DiscountReason {
			get => discountReason;
			set => SetField(ref discountReason, value, () => DiscountReason);
		}

		private IRepresentationModel deliveryPointVM;
		public virtual IRepresentationModel DeliveryPointVM {
			get {
				if(deliveryPointVM == null) {
					deliveryPointVM = new DeliveryPointsVM(new DeliveryPointFilter(UoW));
				}
				return deliveryPointVM;
			}
		}

		private IEntityAutocompleteSelectorFactory counterpartyVM;
		public virtual IEntityAutocompleteSelectorFactory CounterpartyVM {
			get {
				if(counterpartyVM == null) {
					counterpartyVM =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
							CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
				};
				return counterpartyVM;
			}
		}
		
		private IEntityAutocompleteSelectorFactory nomenclatureVM;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureVM {
			get {
				if(nomenclatureVM == null) {
					nomenclatureVM =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), CounterpartyVM,
							new NomenclatureRepository(new NomenclatureParametersProvider()), UserSingletonRepository.GetInstance());
				}
				return nomenclatureVM;
			}
		}
	}
}
