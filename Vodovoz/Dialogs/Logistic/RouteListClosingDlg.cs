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

namespace Vodovoz
{
	public partial class RouteListClosingDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		List<TotalReturnsNode> totalReturns;

		public RouteListClosingDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (Entity.Logistican == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				FailInitialize = true;
				return;
			}
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListClosingDlg (RouteList sub) : this (sub.Id)
		{
		}

		public RouteListClosingDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			referenceCar.SubjectType = typeof(Car);
			referenceCar.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = false;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = false;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = false;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoWGeneric);
			speccomboShift.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = false;

			yspinPlannedDistance.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.PlannedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinPlannedDistance.Sensitive = false;

			yspinActualDistance.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.IsEditable = true;

			datePickerDate.Binding.AddBinding(UoWGeneric.Root, routelist => routelist.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = false;

			routelistclosingitemsview1.RouteListUoW = UoWGeneric;

			totalReturns = RouteListClosingDlg.GetTotalWarehouseIncome(UoWGeneric, UoWGeneric.Root.Id,Nomenclature.GetCategoriesForShipment());
		}

		public override bool Save ()
		{			
			var orderClosingItems = routelistclosingitemsview1.Items.SelectMany(item => item.OrderClosingItems).ToList();
			var equipmentItems = orderClosingItems.Where(item => item.OrderItem.Nomenclature.Serial);
			var nomenclatureItems = orderClosingItems.Where(item => !item.OrderItem.Nomenclature.Serial);
			var nomenclatureReturns = 
				nomenclatureItems.GroupBy(item => item.OrderItem.Nomenclature,
					item => item.Amount,
					(nomenclature, amounts) => new 
					{
						Key = nomenclature,
						Value = amounts.Sum(i => i)
					});
			bool nomenclaturesReturned = nomenclatureReturns.All(kv => 
				{					
					if(!totalReturns.Any(ret => ret.NomenclatureId == kv.Key.Id && ret.Amount == kv.Value)){
						MessageDialogWorks.RunErrorDialog("Сумма возврата не согласуется с документом выгрузки! Пожалуйста укажите по какому заказу был осуществлен недовоз \""+kv.Key.Name+"\".");
					}
					return totalReturns.Any(ret => ret.NomenclatureId == kv.Key.Id && ret.Amount == kv.Value);
				}
			);

			var equipmentReturns = 
				equipmentItems.GroupBy(item => item.OrderItem.Equipment,
					item => item.Amount,
					(equipment, amounts) => new 
					{
						Key = equipment,
						Value = amounts.Sum(i => i)
					});
			bool equipmentsReturned = equipmentReturns.All(kv => 
				{					
					if(!totalReturns.Any(ret => ret.Id == kv.Key.Id && ret.Amount == kv.Value)){
						MessageDialogWorks.RunErrorDialog("Сумма возврата не согласуется с документом выгрузки! Пожалуйста укажите по какому заказу был осуществлен недовоз \""+kv.Key.Title+"\".");
					}
					return totalReturns.Any(ret => ret.Id == kv.Key.Id && ret.Amount == kv.Value);
				}
			);

			logger.Info(nomenclaturesReturned && equipmentsReturned);

			var bottlesReturned = routelistclosingitemsview1.Items.Sum(item => item.BottlesReturned);
			var bottlesReturnedToWarehouse = GetTotalWarehouseIncome(UoW, UoWGeneric.Root.Id, new NomenclatureCategory[]{ NomenclatureCategory.bottle })
				.Sum(item=>item.Amount);
			//var bottlesReturnedFromOrders = routelistclosingitemsview1.Items.Sum(item => item.RouteListItem.Order.BottlesReturn);
			// TODO для каждого заказа проверить возврат бутылей и залогов
			// в orderItems добавить дополнительную колонку (было изначально) а текущая - по факту

			if (bottlesReturned != bottlesReturnedToWarehouse)
			{
				MessageDialogWorks.RunErrorDialog(String.Format("Сумма возврата бутылей не согласуется с документом выгрузки! Указано: {0} Выгружено: {1}", bottlesReturned, bottlesReturnedToWarehouse));
			}

			return false;

			UoWGeneric.Root.Status = RouteListStatus.Closed;
			foreach (var item in UoWGeneric.Root.Addresses)
			{
				//item.Order.OrderStatus = Vodovoz.Domain.Orders.OrderStatus.Closed; //??

			}
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}			

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			Save();
		}

		public static List<TotalReturnsNode> GetTotalWarehouseIncome(IUnitOfWork UoW,int routeListId,NomenclatureCategory[] categories)
		{
			List<TotalReturnsNode> result = new List<TotalReturnsNode>();
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			Nomenclature nomenclatureAlias = null;
			TotalReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			OrderEquipment orderEquipmentAlias = null;
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


}
