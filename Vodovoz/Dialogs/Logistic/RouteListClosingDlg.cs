using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Gtk;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;
using QSProjectsLib;
using Vodovoz.Repository.Logistics;
using System.IO;
using QSReport;
using QSTDI;
using Gamma.Utilities;
using NHibernate.Criterion;
using Vodovoz.Domain.Orders;
using NHibernate.Transform;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;
using Gamma.GtkWidgets;
using System.Linq;
using Vodovoz.Repository;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : OrmGtkDialogBase<RouteListClosing>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		RouteList routelist;
		List<ReturnsNode> allReturnsToWarehouse;
		int bottlesReturnedToWarehouse;

		public RouteListClosingDlg(RouteListClosing closing)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteListClosing>(closing.Id);
			routelist = UoW.GetById<RouteList>(closing.RouteList.Id);
			TabName = String.Format("Закрытие маршрутного листа №{0}",routelist.Id);
			ConfigureDlg ();
		}

		public RouteListClosingDlg (int routeListId)
		{
			this.Build ();

			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteListClosing>();
			routelist = UoW.GetById<RouteList>(routeListId);

			Entity.RouteList = routelist;
			Entity.Cashier = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Cashier == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			TabName = String.Format("Закрытие маршрутного листа №{0}",routelist.Id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{			
			referenceCar.Binding.AddBinding(routelist, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(routelist, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = false;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(routelist, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = false;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(routelist, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = false;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(routelist, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = false;

			yspinPlannedDistance.Binding.AddBinding(routelist, rl => rl.PlannedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinPlannedDistance.Sensitive = false;

			yspinActualDistance.Binding.AddBinding(routelist, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.IsEditable = true;

			datePickerDate.Binding.AddBinding(routelist, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = false;

			routeListAddressesView.UoW = UoW;
			routeListAddressesView.RouteList = routelist;
			foreach (RouteListItem item in routeListAddressesView.Items)
			{
				item.Order.ObservableOrderItems.ElementChanged += OnOrderReturnsChanged;
				item.Order.ObservableOrderEquipments.ElementChanged += OnOrderReturnsChanged;
			}
			routeListAddressesView.Items.ElementChanged += OnRouteListItemChanged;
			routeListAddressesView.OnClosingItemActivated += OnRouteListItemActivated;
			allReturnsToWarehouse = GetReturnsToWarehouseByCategory(routelist.Id, Nomenclature.GetCategoriesForShipment());
			bottlesReturnedToWarehouse = (int)GetReturnsToWarehouseByCategory(routelist.Id, new []{NomenclatureCategory.bottle})
				.Sum(item=>item.Amount);
			var returnableOrderItems = routeListAddressesView.Items
				.Where(address => address.IsDelivered())
				.SelectMany(address => address.Order.OrderItems)
				.Where(orderItem=>!orderItem.Nomenclature.Serial)
				.Where(orderItem => Nomenclature.GetCategoriesForShipment().Any(nom => nom == orderItem.Nomenclature.Category));
			foreach(var item in returnableOrderItems){
				if (allReturnsToWarehouse.All(r => r.NomenclatureId != item.Nomenclature.Id))
					allReturnsToWarehouse.Add(new ReturnsNode{
						Name=item.Nomenclature.Name,
						Trackable=item.Nomenclature.Serial,
						NomenclatureId = item.Nomenclature.Id,
						Amount=0
					});
			}
			if(UoW.IsNew)
				Initialize(routeListAddressesView.Items);
			routelistdiscrepancyview.FindDiscrepancies(routelist.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}

		protected void Initialize(IList<RouteListItem> items)
		{
			foreach (var routeListItem in items)
			{
				var nomenclatures = routeListItem.Order.OrderItems
					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
					.Where(item => !item.Nomenclature.Serial).ToList();
				foreach(var item in nomenclatures)
				{
					item.ActualCount = routeListItem.IsDelivered() ? item.Count : 0;
				}
				var equipments = routeListItem.Order.OrderEquipments.Where(orderEq=>orderEq.Equipment!=null);
				foreach(var item in equipments)
				{
					var returnedToWarehouse = allReturnsToWarehouse.Any(ret => ret.Id == item.Equipment.Id && ret.Amount > 0);
					item.Confirmed = routeListItem.IsDelivered()
						&& (item.Direction == Vodovoz.Domain.Orders.Direction.Deliver && !returnedToWarehouse
							|| item.Direction == Vodovoz.Domain.Orders.Direction.PickUp && returnedToWarehouse);
				}
				routeListItem.BottlesReturned = routeListItem.IsDelivered()
					? routeListItem.Order.BottlesReturn : 0;
				routeListItem.TotalCash = routeListItem.IsDelivered() && 
					routeListItem.Order.PaymentType==PaymentType.cash
					? routeListItem.Order.SumToReceive : 0;
				routeListItem.DepositsCollected = routeListItem.IsDelivered()
					? routeListItem.Order.OrderDepositItems.Sum(depositItem => depositItem.Deposit) : 0;
				routeListItem.RecalculateWages();
			}
		}

		void OnRouteListItemActivated(object sender, RowActivatedArgs args)
		{			
			var node = routeListAddressesView.GetSelectedRouteListItem();
			var dlg = new OrderReturnsView(node);
			TabParent.AddSlaveTab(this, dlg);
		}

		void OnRouteListItemChanged (object aList, int[] aIdx)
		{			
			var item = routeListAddressesView.Items[aIdx[0]];
			item.RecalculateWages();
			OnItemsUpdated();
		}

		void OnOrderReturnsChanged(object aList, int[] aIdx)
		{
			foreach (var item in routeListAddressesView.Items)
			{
				(item as RouteListItem).RecalculateWages();
			}
			routelistdiscrepancyview.FindDiscrepancies(routelist.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}			

		void OnItemsUpdated(){
			CalculateTotal();
			buttonAccept.Sensitive = isConsistentWithUnloadDocument();
		}

		void CalculateTotal ()
		{
			var items = routeListAddressesView.Items.Where(item=>item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			int fullBottlesTotal = items.SelectMany(item => item.Order.OrderItems).Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
			decimal depositsCollectedTotal = items.Sum(item => item.DepositsCollected);
			decimal totalCollected = items.Sum(item => item.TotalCash);
			decimal driverWage = items.Sum(item => item.DriverWage);
			decimal forwarderWage = items.Sum(item => item.ForwarderWage);
			int addressCount = items.Count(item => item.IsDelivered());
			decimal phoneSum = Wages.GetDriverRates().PhoneServiceCompensationRate * addressCount;
			labelAddressCount.Text = String.Format("Адресов:{0}", addressCount);
			labelPhone.Text = String.Format(
				"Сот. связь:{0} {1}",
				phoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelFullBottles.Text = String.Format("Полн. бутылей:{0}", fullBottlesTotal);
			labelDeposits.Text = String.Format(
				"Залогов:{0} {1}",
				depositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelCash.Text = String.Format(
				"Итого(нал.){0} {1}", 
				totalCollected,
				CurrencyWorks.CurrencyShortName
			);
			labelTotalCollected.Text = String.Format(
				"Итого сдано:{0} {1}", 
				totalCollected + depositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelTotal.Markup = String.Format(
				"Итого: <b>{0}</b> {1}",
				totalCollected+depositsCollectedTotal-phoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelWage1.Markup = String.Format(
				"Зарплата водителя:<b>{0}</b> {2}" + "  " + "Зарплата экспедитора:<b>{1}</b> {2}", 
				driverWage,
				forwarderWage,
				CurrencyWorks.CurrencyShortName
			);
			labelEmptyBottles1.Markup = String.Format("Тара: <b>{0}</b><sub>(выгружено)</sub> - <b>{1}</b><sub>(итого сдано)</sub> =",
				bottlesReturnedToWarehouse,
				bottlesReturnedTotal
			);
			var differenceAttributes = bottlesReturnedToWarehouse - bottlesReturnedTotal > 0 ? "background=\"#ff5555\"" : "";
			var bottleDifferenceFormat = "<span {1}><b>{0}</b><sub>(осталось)</sub></span>";
			labelBottleDifference.Markup = String.Format(bottleDifferenceFormat, bottlesReturnedToWarehouse-bottlesReturnedTotal, differenceAttributes);
		}

		protected bool isConsistentWithUnloadDocument(){
			var hasItemsDiscrepancies = routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0);
			var items = routeListAddressesView.Items.Where(item=>item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return !hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies;
		}

		protected void CheckBottlesAndDeposits(){
			foreach(RouteListItem item in routeListAddressesView.Items){
				if (item.Order.PaymentType == PaymentType.cash)
				{
					var totalBottlesReceived = item.Order.OrderItems
						.Where(orderItem => orderItem.Nomenclature.Category == NomenclatureCategory.water)
						.Sum(orderItem => orderItem.ActualCount);

					var expectedDepositsCount = totalBottlesReceived - item.BottlesReturned;
					var expectedDeposits = 
						NomenclatureRepository.GetBottleDeposit(UoW).GetPrice(1)*expectedDepositsCount;

					if (expectedDeposits != item.DepositsCollected)
					{
						MessageDialogWorks.RunWarningDialog(String.Format("Сумма полученных залогов" +
							" не верна для заказа №{0}", item.Order.Id));
						return;
					}

				}
			}
		}
			

		public override bool Save ()
		{
			var valid = new QSValidator<RouteListClosing> (Entity);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoW.Save();
			return true;
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			CheckBottlesAndDeposits();
			if (!isConsistentWithUnloadDocument())
				return;

			var bottleMovementOperations = Entity.CreateBottlesMovementOperation();
			bottleMovementOperations.ForEach(op => UoW.Save(op));

			var counterpartyMovementOperations = Entity.CreateCounterpartyMovementOperations();
			counterpartyMovementOperations.ForEach(op => UoW.Save(op));

			var depositsOperations = Entity.CreateDepositOperations();
			depositsOperations.ForEach(op => UoW.Save(op));

			Entity.Confirm();

			UoW.Save();

			buttonAccept.Sensitive = false;
		}

		public List<ReturnsNode> GetReturnsToWarehouseByCategory(int routeListId,NomenclatureCategory[] categories)
		{
			List<ReturnsNode> result = new List<ReturnsNode>();		
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableItems = UoW.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(()=>movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Nomenclature, ()=>nomenclatureAlias)
				.Where (() => !nomenclatureAlias.Serial)		
				.Where (() => nomenclatureAlias.Category.IsIn(categories))
				.SelectList (list => list
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => false).WithAlias (() => resultAlias.Trackable)
					.Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					.SelectSum(()=>movementOperationAlias.Amount).WithAlias(()=>resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<ReturnsNode> ())
				.List<ReturnsNode> ();

			var returnableEquipment = UoW.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category.IsIn(categories))
				.SelectList(list => list
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.Id)				
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Serial).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Type).WithAlias(() => resultAlias.EquipmentType)
			                          )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			result.AddRange(returnableItems);
			result.AddRange(returnableEquipment);
			return result;
		}
	}

	public class ReturnsNode{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}
		public decimal Amount{ get; set;}
		public bool Trackable{ get; set; }
		public EquipmentType EquipmentType{get;set;}
		public string Serial{ get { 
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}
		public bool Returned {
			get {
				return Amount > 0;
			}
			set {
				Amount = value ? 1 : 0;
			}
		}
	}
}
