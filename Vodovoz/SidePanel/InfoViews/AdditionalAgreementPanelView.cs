using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NHibernate.Proxy;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository;
using Vodovoz.Repository.Client;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdditionalAgreementPanelView : Gtk.Bin, IPanelView
	{
		private AdditionalAgreement[] WaterAgreements;

		DeliveryPoint DeliveryPoint{get;set;}

		public AdditionalAgreementPanelView()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			labelNextService.LineWrapMode = Pango.WrapMode.WordChar;
			labelRent.LineWrapMode = Pango.WrapMode.WordChar;
			labelEquipmentCount.LineWrapMode = Pango.WrapMode.WordChar;
		}

		#region IPanelView implementation

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if (DeliveryPoint == null)
				return;
			var allEquipmentAtDeliveryPoint = EquipmentRepository.GetEquipmentAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			labelEquipmentCount.Text = allEquipmentAtDeliveryPoint.Count + " шт.";
			var nextServiceText = "";		
			var equipmentsWithNextServiceDate = allEquipmentAtDeliveryPoint
				.Where(eq => eq.NextServiceDate.HasValue);				
			var eqWithMinDate = equipmentsWithNextServiceDate
				.Aggregate<Equipment,Equipment,Equipment>(null,(minEq,eq)=>(minEq==null || (eq.NextServiceDate<minEq.NextServiceDate) ? eq : minEq),r=>r);
			if (eqWithMinDate != null)
			{
				var nextServiceDate = eqWithMinDate.LastServiceDate.AddMonths(6);
				var daysTillNextService = (nextServiceDate - DateTime.Today).Days;
				nextServiceText = String.Format(
					"{0} (осталось {1} {2})",
					nextServiceDate.ToShortDateString(),
					daysTillNextService,
					RusNumber.Case(daysTillNextService, "день", "дня", "дней")
				);
			}
			labelNextService.Text = nextServiceText;
			var agreements = AdditionalAgreementRepository.GetActiveAgreementsForDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var dailyAgreements = agreements
				.OfType<DailyRentAgreement>()
				.OrderBy(a => a.EndDate);
			vboxRent.Visible = dailyAgreements.Count() > 0;
			var rentText = String.Join(
				"\n", 
				dailyAgreements.Select(a =>String.Format(
					"{0} - A до {1}",
					a.AgreementNumber,
					a.EndDate.ToShortDateString()
				)).ToArray()
			);
			labelRent.Text = rentText;
			var freeRent = agreements.OfType<FreeRentAgreement>();
			var requiredBottlesThisMonth = freeRent.SelectMany(a => a.Equipment).Sum(eq => eq.WaterAmount);
			var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			var monthEnd = monthStart.AddMonths(1).AddDays(-1);
			var bottlesThisMonth = DeliveryPointRepository.GetBottlesOrderedForPeriod(InfoProvider.UoW, DeliveryPoint, monthStart, monthEnd);
			var bottlesLeftToOrder = requiredBottlesThisMonth - bottlesThisMonth;
			var leftToOrderText = "";
			if (bottlesLeftToOrder > 0)
				leftToOrderText = String.Format(" (осталось: {0})", bottlesLeftToOrder);
			labelBottlesPerMonth.Text = String.Format("{0} из {1}{2}", bottlesThisMonth, requiredBottlesThisMonth, leftToOrderText);

			List<WaterSalesAgreementFixedPrice> fixedPricesList = new List<WaterSalesAgreementFixedPrice>();

			foreach(AdditionalAgreement agreement in agreements)
			{
				var fixedPrices = AdditionalAgreementRepository.GetFixedPricesForAgreement(InfoProvider.UoW, agreement);
				fixedPricesList.AddRange(fixedPrices);
			}

			ytreeviewFixedPrices.ColumnsConfig = ColumnsConfigFactory.Create<WaterSalesAgreementFixedPrice>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Цена")
				.AddTextRenderer(x => String.Format("{0}р.", x.Price))
				.Finish();

			ytreeviewFixedPrices.SetItemsSource(fixedPricesList);

			ytreeviewFixedPrices.Visible = fixedPricesList.Count > 0;
			hboxNotFixedPriceInfo.Visible = fixedPricesList.Count == 0;

			WaterAgreements = agreements.Where(a => a.Type == AgreementType.WaterSales).ToArray();

			buttonWaterAgreement.Label = WaterAgreements.Length > 0 ? "Открыть доп. согл." : "Создать доп. согл.";
		}

		public bool VisibleOnPanel
		{
			get
			{
				return DeliveryPoint != null;
			}
		}

		public void OnCurrentObjectChanged(object changedObject)
		{			
			var deliveryPoint = changedObject as DeliveryPoint;
			if (deliveryPoint!=null)
			{
				Refresh();
			}
		}

		public IInfoProvider InfoProvider { get; set; }

		public CounterpartyContract Contract => (InfoProvider as IContractInfoProvider)?.Contract;

		protected void OnYtreeviewFixedPricesRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var selectedPrice = ytreeviewFixedPrices.GetSelectedObject() as WaterSalesAgreementFixedPrice;
			var type = NHibernateProxyHelper.GuessClass(selectedPrice.AdditionalAgreement);
			var dialog = OrmMain.CreateObjectDialog(type, selectedPrice.AdditionalAgreement.Id);
			TDIMain.MainNotebook.OpenTab(
				OrmMain.GenerateDialogHashName(type, selectedPrice.AdditionalAgreement.Id),
				() => dialog
			);
		}

		protected void OnButtonWaterAgreementClicked(object sender, EventArgs e)
		{
			if(WaterAgreements.Length > 0)
			{
				foreach(var wa in WaterAgreements) {
					TDIMain.MainNotebook.OpenTab(
						OrmMain.GenerateDialogHashName<WaterSalesAgreement>(wa.Id),
						() => new WaterAgreementDlg(wa.Id)
					);
				}
			}
			else
			{
				if(Contract == null || DeliveryPoint == null)
					return;
				
				var waDlg = new WaterAgreementDlg(Contract, DeliveryPoint);
				TDIMain.MainNotebook.AddTab(waDlg);
			}
		}

		#endregion
	}
}

