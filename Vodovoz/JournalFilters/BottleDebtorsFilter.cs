using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottleDebtorsFilter : RepresentationFilterBase<BottleDebtorsFilter>
	{
		protected override void ConfigureWithUow()
		{
			entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM();
			entryreferenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM();
			yentryreferencevmNomenclature.RepresentationModel = new ViewModel.NomenclatureForSaleVM();

			yenumcomboboxOPF.ItemsEnum = typeof(PersonType);

			entryreferenceClient.Changed += (sender, e) => OnRefiltered();
			yenumcomboboxOPF.ChangedByUser += (sender, e) => OnRefiltered();
			entryreferenceDeliveryPoint.ChangedByUser += (sender, e) => OnRefiltered();
			buttonDebtBottleCountOK.Clicked += (sender, e) => OnRefiltered();

			buttonOrderBottleCountOK.Clicked += (sender, e) => OnRefiltered();
			ydateperiodpickerLastOrder.PeriodChanged += (sender, e) => OnRefiltered();
			yentryreferencevmNomenclature.ChangedByUser += (sender, e) => OnRefiltered();

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

		public int? DebtBottlesFrom {
			get { return (int)yspinbuttonDebtForm.Value; }
			set { yspinbuttonDebtForm.Value = value.Value; }
		}

		public int? DebtBottlesTo {
			get { return (int)yspinbuttonDebtTo.Value; }
			set { yspinbuttonDebtTo.Value = value.Value; }
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

		public int? LastOrderBottlesFrom {
			get { return (int)yspinbuttonOrderBottlesFrom.Value; }
			set { yspinbuttonOrderBottlesFrom.Value = value.Value; }
		}

		public int? LastOrderBottlesTo {
			get { return (int)yspinbuttonOrderBottlesTo.Value; }
			set { yspinbuttonOrderBottlesTo.Value = value.Value; }
		}

		public Nomenclature LastOrderNomenclature {
			get { return yentryreferencevmNomenclature.Subject as Nomenclature; }
			set { yentryreferencevmNomenclature.Subject = value; }
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
	}
}
