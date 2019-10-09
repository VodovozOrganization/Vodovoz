using System;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;

namespace Vodovoz.Filters.ViewModels
{
	public class DebtorsJournalFilterViewModel : FilterViewModelBase<DebtorsJournalFilterViewModel>, IJournalFilter
	{
		public DebtorsJournalFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
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
				x => x.DiscountReason
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
		public DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
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

		private IEntityAutocompleteSelectorFactory nomenclatureVM;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureVM {
			get {
				if(nomenclatureVM == null) {
					nomenclatureVM = new EntityRepresentationAdapterFactory(typeof(Nomenclature),
						() => {
							var vm = new NomenclatureForSaleVM(new NomenclatureRepFilter(UnitOfWorkFactory.CreateWithoutRoot()));
							vm.Filter.AvailableCategories = Nomenclature.GetCategoriesForSale();
							return vm;
						});
				}
				return nomenclatureVM;
			}
		}

		private IEntityAutocompleteSelectorFactory counterpartyVM;
		public virtual IEntityAutocompleteSelectorFactory CounterpartyVM {
			get {
				if(counterpartyVM == null) {
					counterpartyVM = new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices);
				};
				return counterpartyVM;
			}
		}
	}
}
