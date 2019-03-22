using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottleDebtorsFilter : RepresentationFilterBase<BottleDebtorsFilter>
	{
		protected override void ConfigureWithUow()
		{
			entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM();
			entryreferenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM();
			yenumcomboboxOPF.ItemsEnum = typeof(PersonType);
			yvalidatedentryDebtFrom.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yvalidatedentryDebtBy.ValidationMode = QSWidgetLib.ValidationType.numeric;
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

		public DateTime? StartDate {
			get { return ydateperiodpickerLastOrder.StartDateOrNull ; }
			set {
				ydateperiodpickerLastOrder.StartDate = (DateTime)value;
				ydateperiodpickerLastOrder.Sensitive = false;
			}
		}

		public DateTime? EndDate {
			get { return ydateperiodpickerLastOrder.EndDateOrNull; }
			set {
				ydateperiodpickerLastOrder.EndDate = (System.DateTime)value;
				ydateperiodpickerLastOrder.Sensitive = false;
			}
		}

		public PersonType? OPF{
			get { return yenumcomboboxOPF.SelectedItem as PersonType?; }
			set {
				yenumcomboboxOPF.SelectedItem = value;
				yenumcomboboxOPF.Sensitive = false;
			}
		}

		public int? DebtFrom {
			get {
				try {
					return Convert.ToInt32(yvalidatedentryDebtFrom.Text);
				} catch {
					return null;
				}
			}
				set { yvalidatedentryDebtFrom.Text += value; }
		}

		public int? DebtBy {
			get {
				try {
					return Convert.ToInt32(yvalidatedentryDebtBy.Text);
				} catch {
					return null;
				}
			}
			set { yvalidatedentryDebtBy.Text += value; }
		}

		public BottleDebtorsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public BottleDebtorsFilter()
		{
			this.Build();
		}


		protected void OnEntryreferenceClientChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYenumcomboboxOPFChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYdateperiodpickerLastOrderPeriodChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryreferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnButtonBottleCountOKClicked(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
