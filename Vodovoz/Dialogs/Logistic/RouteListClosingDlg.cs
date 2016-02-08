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
		List<TotalReturnsNode> allReturnsToWarehouse;

		public RouteListClosingDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteListClosing> ();
			routelist = UoW.GetById<RouteList>(id);
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
			Initialize(routeListAddressesView.Items);

			allReturnsToWarehouse = GetReturnsToWarehouseByCategory(routelist.Id, Nomenclature.GetCategoriesForShipment());
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
					item.ActualCount = routeListItem.Status==RouteListItemStatus.Completed ? item.Count : 0;
				}
				var equipments = routeListItem.Order.OrderEquipments;
				foreach(var item in equipments)
				{
					item.Confirmed = routeListItem.Status==RouteListItemStatus.Completed;
				}
				routeListItem.BottlesReturned = routeListItem.Order.BottlesReturn;
				routeListItem.TotalCash = routeListItem.Order.SumToReceive;
			}
		}
			
		protected bool FindNomenclatureDiscrepancy(
			List<TotalReturnsNode> fromClient, List<TotalReturnsNode> toWarehouse,
			out TotalReturnsNode nodeFrom, out TotalReturnsNode nodeTo)
		{
			for (int i = 0; i < fromClient.Count; i++)
			{
				var sameNomenclatureToWarehouse = toWarehouse.FirstOrDefault(t => t.NomenclatureId == fromClient[i].NomenclatureId);
				if (sameNomenclatureToWarehouse == null)
				{
					nodeFrom = fromClient[i];
					nodeTo = null;
					return true;
				}					
				if (fromClient[i].Amount != sameNomenclatureToWarehouse.Amount)
				{
					nodeFrom = fromClient[i];
					nodeTo = sameNomenclatureToWarehouse;
					return true;
				}
			}

			for (int i = 0; i < toWarehouse.Count; i++)
			{
				if (!fromClient.Any(f => f.NomenclatureId == toWarehouse[i].NomenclatureId))
				{
					nodeFrom = null;
					nodeTo = toWarehouse[i];
					return true;
				}
			}
			nodeFrom = null;
			nodeTo = null;
			return false;
		}

		protected bool FindEquipmentDiscrepancy(
			List<TotalReturnsNode> fromClient, List<TotalReturnsNode> toWarehouse,
			out TotalReturnsNode nodeFrom, out TotalReturnsNode nodeTo)
		{
			for (int i = 0; i < fromClient.Count; i++)
			{
				var sameEquipmentToWarehouse = toWarehouse.FirstOrDefault(t => t.Id == fromClient[i].Id);
				if (sameEquipmentToWarehouse == null)
				{
					nodeFrom = fromClient[i];
					nodeTo = null;
					return true;
				}					
				if (fromClient[i].Amount != sameEquipmentToWarehouse.Amount)
				{
					nodeFrom = fromClient[i];
					nodeTo = sameEquipmentToWarehouse;
					return true;
				}
			}

			for (int i = 0; i < toWarehouse.Count; i++)
			{
				if (!fromClient.Any(f => f.Id == toWarehouse[i].Id))
				{
					nodeFrom = null;
					nodeTo = toWarehouse[i];
					return true;
				}
			}
			nodeFrom = null;
			nodeTo = null;
			return false;
		}

		protected bool isConsistentWithUnloadDocument(){
			var orderClosingItems = routeListAddressesView.Items
				.SelectMany(item => item.Order.OrderItems)
				.Where(item=>Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.ToList();
			var nomenclatureItems = orderClosingItems.Where(item => !item.Nomenclature.Serial);
			var nomenclatureReturnedFromClient = 
				nomenclatureItems.GroupBy(item => item.Nomenclature,
					item => item.ReturnedCount,
					(nomenclature, amounts) => new
					TotalReturnsNode
					{
						Name = nomenclature.Name,
						NomenclatureId = nomenclature.Id,
						NomenclatureCategory = nomenclature.Category,	
						Amount = amounts.Sum(i => i)
					}).Where(item=>item.Amount>0).ToList();
							
			var nomenclatureReturnedToWarehouse = allReturnsToWarehouse.Where(item=>!item.Trackable).ToList();
			TotalReturnsNode fromClient;
			TotalReturnsNode toWarehouse;
			if (FindNomenclatureDiscrepancy(nomenclatureReturnedFromClient,
				   nomenclatureReturnedToWarehouse, out fromClient, out toWarehouse))
			{
				var name = fromClient != null ? fromClient.Name : toWarehouse.Name;
				var fromClientAmount = fromClient != null ? fromClient.Amount : 0;
				var toWarehouseAmount = toWarehouse != null ? toWarehouse.Amount : 0;
				MessageDialogWorks.RunErrorDialog(String.Format("Сумма возврата не согласуется с документом выгрузки!" +
					" Пожалуйста укажите по какому заказу был осуществлен недовоз \"{0}\"." +
					"Указано: {1}, Выгружено: {2}",
					name,
					fromClientAmount,
					toWarehouseAmount));
				return false;
			}

			var equipmentItems = routeListAddressesView.Items
				.SelectMany(item => item.Order.OrderEquipments).ToList();			
			var equipmentReturnedFromClient = 
				equipmentItems.GroupBy(item => item.Equipment,
					item => item.Confirmed ? 0 : 1,
					(equipment, amounts) => new 
					TotalReturnsNode
					{
						Id = equipment.Id,
						Name = equipment.NomenclatureName,
						NomenclatureCategory = equipment.Nomenclature.Category,							
						Amount = amounts.Sum(i => i)
					}).Where(item=>item.Amount>0).ToList();
			
			var equipmentReturnedToWarehouse = allReturnsToWarehouse.Where(item=>item.Trackable).ToList();
			if (FindEquipmentDiscrepancy(equipmentReturnedFromClient,
				equipmentReturnedToWarehouse, out fromClient, out toWarehouse))
			{
				var name = fromClient != null 
					? fromClient.Name+"(с/н: "+fromClient.Id+")" 
					: toWarehouse.Name+"(с/н: "+toWarehouse.Id+")";
				var fromClientAmount = fromClient != null ? fromClient.Amount : 0;
				var toWarehouseAmount = toWarehouse != null ? toWarehouse.Amount : 0;
				MessageDialogWorks.RunErrorDialog(String.Format("Сумма возврата не согласуется с документом выгрузки!" +
					" Пожалуйста укажите по какому заказу был осуществлен недовоз \"{0}\"." +
					"Указано: {1}, Выгружено: {2}",
					name,
					fromClientAmount,
					toWarehouseAmount
					));
				return false;
			}

			var totalBottlesReturned = routeListAddressesView.Items.Sum(item => item.BottlesReturned);
			var totalBottlesReturnedToWarehouse = GetReturnsToWarehouseByCategory(Entity.RouteList.Id,
				new NomenclatureCategory[]{ NomenclatureCategory.bottle })
				.Sum(item => item.Amount);
			bool bottleDiscrepency = totalBottlesReturned != totalBottlesReturnedToWarehouse;
			if (bottleDiscrepency)
			{
				MessageDialogWorks.RunErrorDialog(String.Format("Сумма возврата бутылей не" +
					" согласуется с документом выгрузки! Указано: {0} Выгружено: {1}",
					totalBottlesReturned, totalBottlesReturnedToWarehouse));
				return false;
			}	

			return true;
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
					/*if (expectedDeposits != item.DepositsCollected)
					{
						MessageDialogWorks.RunWarningDialog(String.Format("Сумма полученных залогов" +
							" не верна для заказа №{0}", item.Order.Id));
						return;
					}
					*/
				}
			}
		}
			

		public override bool Save ()
		{
			var valid = new QSValidator<RouteListClosing> (Entity);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			CheckBottlesAndDeposits();
			if (!isConsistentWithUnloadDocument())
				return false;
			var bottleMovementOperations = Entity.CreateBottlesMovementOperation();
			bottleMovementOperations.ForEach(op => UoW.Save(op));

			var counterpartyMovementOperations = Entity.CreateCounterpartyMovementOperations();
			counterpartyMovementOperations.ForEach(op => UoW.Save(op));

			Entity.Confirm();

			UoW.Save();
			return true;
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			Save();
		}

		public List<TotalReturnsNode> GetReturnsToWarehouseByCategory(int routeListId,NomenclatureCategory[] categories)
		{
			List<TotalReturnsNode> result = new List<TotalReturnsNode>();		
			Nomenclature nomenclatureAlias = null;
			TotalReturnsNode resultAlias = null;
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
				.TransformUsing (Transformers.AliasToBean<TotalReturnsNode> ())
				.List<TotalReturnsNode> ();

			var returnableEquipment = UoW.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(()=>movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(()=>movementOperationAlias.Equipment,()=>equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Category.IsIn(categories))
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)				
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
					.Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					.SelectSum(()=>movementOperationAlias.Amount).WithAlias(()=>resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<TotalReturnsNode> ())
				.List<TotalReturnsNode> ();

			result.AddRange(returnableItems);
			result.AddRange(returnableEquipment);
			return result;
		}
	}

	public class TotalReturnsNode{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}
		public decimal Amount{ get; set;}
		public bool Trackable{ get; set; }
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
