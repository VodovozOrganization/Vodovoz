using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class CarUnloadDocumentDlg : OrmGtkDialogBase<CarUnloadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		IList<Equipment> alreadyUnloadedEquipment;

		public CarUnloadDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarUnloadDocument> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				Entity.Warehouse = UoWGeneric.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);

			ConfigureDlg ();
		}

		public CarUnloadDocumentDlg (int routeListId, int? warehouseId) : this()
		{
			if(warehouseId.HasValue)
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			UpdateRouteListInfo();
		}

		public CarUnloadDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarUnloadDocument> (id);
			ConfigureDlg ();
		}

		public CarUnloadDocumentDlg (CarUnloadDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.RestrictStatus = RouteListStatus.EnRoute;
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();

			//Entity.UpdateStockAmount(UoW);
			//Entity.UpdateAlreadyLoaded(UoW);
			//Entity.UpdateInRouteListAmount(UoW);
			//carloaddocumentview1.DocumentUoW = UoWGeneric;
			bottlereceptionview1.UoW = UoW;
			LoadBottleReception();
			returnsreceptionview2.UoW = UoW;
			returnsreceptionview2.Warehouse = Entity.Warehouse;

			SetupForNewRouteList();

			UpdateWidgetsVisible();
		}

		public override bool Save ()
		{
			UpdateBottlesOnEntity();

			var valid = new QSValidation.QSValidator<CarUnloadDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			//Entity.UpdateOperations(UoW);
			//if (Entity.ShipIfCan())
			//	MessageDialogWorks.RunInfoDialog("Маршрутный лист отгружен полностью.");

			logger.Info ("Сохраняем разгрузочный талон...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null)
			{
				ytextviewRouteListInfo.Buffer.Text = String.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				String.Format ("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
					Entity.RouteList.Id,
					Entity.RouteList.Date,
					Entity.RouteList.Driver.FullName,
					Entity.RouteList.Car.Model,
					Entity.RouteList.Car.RegistrationNumber,
					Entity.RouteList.Forwarder != null ? Entity.RouteList.Forwarder.FullName : "(Отсутствует)" 
				);
		}

		void UpdateAlreadyUnloaded()
		{
			alreadyUnloadedEquipment = Repository.EquipmentRepository.GetEquipmentUnloadedTo(UoW, Entity.RouteList);
			returnsreceptionview2.AlreadyUnloadedEquipment = alreadyUnloadedEquipment;
		}

		protected void OnYentryrefRouteListChangedByUser(object sender, EventArgs e)
		{
			SetupForNewRouteList();
		}

		void SetupForNewRouteList()
		{
			UpdateRouteListInfo();
			equipmentreceptionview1.RouteList = Entity.RouteList;
			returnsreceptionview2.RouteList = Entity.RouteList;
			if (Entity.RouteList != null)
			{
				UpdateAlreadyUnloaded();
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(CarUnloadDocument), "талона"))
				Save ();

			var reportInfo = new QSReport.ReportInfo
				{
					Title = Entity.Title,
					Identifier = "Store.CarUnloadDoc",
					Parameters = new System.Collections.Generic.Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					}
				};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}

		private void UpdateWidgetsVisible()
		{
			bottlereceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			equipmentreceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
		}

		void LoadBottleReception()
		{
			foreach(var item in bottlereceptionview1.Items)
			{
				var returned = Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == item.NomenclatureId);
				item.Amount = returned != null ? (int)returned.MovementOperation.Amount : 0;
			}
		}

		void UpdateBottlesOnEntity()
		{
			var nomenclatures = UoW.GetById<Nomenclature>(bottlereceptionview1.Items.Select(x => x.NomenclatureId).ToArray());
			foreach (Vodovoz.ViewModel.BottleReceptionVMNode node in bottlereceptionview1.Items) {
				var item = Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == node.NomenclatureId);
				if (node.Amount != 0 && item == null) {
					Entity.AddItem(
						nomenclatures.First(x => x.Id == node.NomenclatureId),
						null,
						node.Amount
					);
				}
				else if(node.Amount != 0 && item != null)
				{
					item.MovementOperation.Amount = node.Amount;
				}
				else if(node.Amount == 0 && item != null)
				{
					UoW.Delete(item.MovementOperation);
					Entity.ObservableItems.Remove(item);
				}
			}
		}

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			UpdateWidgetsVisible();
			returnsreceptionview2.Warehouse = Entity.Warehouse;
		}
	}
}

