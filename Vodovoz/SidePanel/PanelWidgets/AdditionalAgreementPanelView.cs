using System;
using Vodovoz.Domain;
using QSProjectsLib;
using Vodovoz.Repository;
using System.Linq;

namespace Vodovoz.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdditionalAgreementPanelView : Gtk.Bin, IPanelView
	{
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

		public IInfoProvider InfoProvider
		{
			get;
			set;
		}

		#endregion
	}
}

