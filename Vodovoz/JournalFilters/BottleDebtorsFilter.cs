using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottleDebtorsFilter : RepresentationFilterBase<BottleDebtorsFilter>
	{
		IUnitOfWork nomenclatureUoW;

		protected override void ConfigureWithUow()
		{
			yvalidatedentryDebtTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDebtFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesTo.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryBottlesFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;

			entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM();
			entryreferenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM();
			entityviewmodelentryNomenclature.SetEntityAutocompleteSelectorFactory(
						new EntityRepresentationAdapterFactory(typeof(Nomenclature),
						() => {
							var vm = new ViewModel.NomenclatureForSaleVM(new NomenclatureRepFilter(UnitOfWorkFactory.CreateWithoutRoot()));
							vm.Filter.AvailableCategories = Nomenclature.GetCategoriesForSale();
							return vm;
						})
			);

			yenumcomboboxOPF.ItemsEnum = typeof(PersonType);

			entryreferenceClient.Changed += (sender, e) => OnRefiltered();
			yenumcomboboxOPF.ChangedByUser += (sender, e) => OnRefiltered();
			entryreferenceDeliveryPoint.ChangedByUser += (sender, e) => OnRefiltered();
			buttonDebtBottleCountOK.Clicked += (sender, e) => OnRefiltered();

			buttonOrderBottleCountOK.Clicked += (sender, e) => OnRefiltered();
			ydateperiodpickerLastOrder.PeriodChanged += (sender, e) => OnRefiltered();
			entityviewmodelentryNomenclature.ChangedByUser += (sender, e) => OnRefiltered();

			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = UoW?.Session.QueryOver<DiscountReason>().List();
			ycomboboxReason.Changed += (sender, e) => OnRefiltered();

		}

		public Counterparty Client {
			get { return entryreferenceClient.Subject as Counterparty; }
			set {
				entryreferenceClient.Subject = value;
				entryreferenceClient.Sensitive = false;
			}
		}

		public DeliveryPoint Address {
			get { return entryreferenceDeliveryPoint.Subject as DeliveryPoint; }
			set {
				entryreferenceDeliveryPoint.Subject = value;
				entryreferenceDeliveryPoint.Sensitive = false;
			}
		}

		public PersonType? OPF{
			get { return yenumcomboboxOPF.SelectedItem as PersonType?; }
			set { yenumcomboboxOPF.SelectedItem = value; }
		}


		public DateTime? StartDate {
			get { return ydateperiodpickerLastOrder.StartDateOrNull; }
			set {
				ydateperiodpickerLastOrder.StartDate = (DateTime)value;
				ydateperiodpickerLastOrder.Sensitive = false;
			}
		}

		public DateTime? EndDate {
			get { return ydateperiodpickerLastOrder.EndDateOrNull; }
			set {
				ydateperiodpickerLastOrder.EndDate = (DateTime)value;
				ydateperiodpickerLastOrder.Sensitive = false;
			}
		}

		public int? DebtBottlesFrom { get; set; }

		public int? DebtBottlesTo { get; set; }

		public int? LastOrderBottlesFrom { get; set; }

		public int? LastOrderBottlesTo { get; set; }

		public Nomenclature LastOrderNomenclature {
			get { return entityviewmodelentryNomenclature.Subject as Nomenclature; }
			set { entityviewmodelentryNomenclature.Subject = value; }
		}

		public DiscountReason DiscountReason {
			get { return ycomboboxReason.SelectedItem as DiscountReason; }
			set { ycomboboxReason.SelectedItem = value; }
		}

		public BottleDebtorsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public BottleDebtorsFilter()
		{
			this.Build();
		}

		protected void OnYvalidatedentryDebtFromChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yvalidatedentryDebtFrom.Text, out int result))
				DebtBottlesFrom = result;
		}

		protected void OnYvalidatedentryDebtToChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yvalidatedentryDebtTo.Text, out int result))
				DebtBottlesTo = result;
		}

		protected void OnYvalidatedentryBottlesFromChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yvalidatedentryBottlesFrom.Text, out int result))
				LastOrderBottlesFrom = result;
		}

		protected void OnYvalidatedentryBottlesToChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yvalidatedentryBottlesTo.Text, out int result))
				LastOrderBottlesTo = result;
		}

	}
}
