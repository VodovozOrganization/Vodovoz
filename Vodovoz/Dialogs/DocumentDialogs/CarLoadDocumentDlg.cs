using System;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Additions.Store;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class CarLoadDocumentDlg : OrmGtkDialogBase<CarLoadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		bool editing = false;
		public CarLoadDocumentDlg()
		{
			this.Build();

			ConfigureNewDoc();
			ConfigureDlg();
		}

		public CarLoadDocumentDlg (int routeListId, int? warehouseId)
		{
			this.Build();
			ConfigureNewDoc();

			if (warehouseId.HasValue)
			{
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);

			}
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarLoadDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarLoadDocument> (id);
			ConfigureDlg ();
		}

		public CarLoadDocumentDlg (CarLoadDocument sub) : this (sub.Id)
		{
		}

		void ConfigureNewDoc()
		{
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarLoadDocument>();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.CarLoadEdit);
		}

		void ConfigureDlg ()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.CarLoadEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.CarLoadEdit, Entity.Warehouse);
			yentryrefRouteList.IsEditable = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = editing;
			
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.CarLoadEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.RestrictStatus = RouteListStatus.InLoading;
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();

			UpdateRouteListInfo();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyLoaded(UoW);
			Entity.UpdateInRouteListAmount(UoW);
			carloaddocumentview1.DocumentUoW = UoWGeneric;
			carloaddocumentview1.SetButtonEditing(editing);
			if(UoW.IsNew && Entity.Warehouse != null)
				carloaddocumentview1.FillItemsByWarehouse();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<CarLoadDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if(Entity.Items.Any(x => x.Amount == 0))
			{
				if (MessageDialogWorks.RunQuestionDialog("В списке есть нулевые позиции. Убрать нулевые позиции перед сохранением?"))
					Entity.ClearItemsFromZero();
			}

			Entity.UpdateOperations(UoW);

			logger.Info ("Сохраняем погрузочный талон...");
			UoWGeneric.Save ();

			logger.Info ("Меняем статус маршрутного листа...");
			if (Entity.RouteList.ShipIfCan(UoW))
				MessageDialogWorks.RunInfoDialog("Маршрутный лист отгружен полностью.");
			UoW.Save(Entity.RouteList);
			UoW.Commit();

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

		protected void OnYentryrefRouteListChangedByUser(object sender, EventArgs e)
		{
			UpdateRouteListInfo();
			if (Entity.Warehouse != null && Entity.RouteList != null)
				carloaddocumentview1.FillItemsByWarehouse();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(CarLoadDocument), "талона"))
				Save ();

			var reportInfo = new QSReport.ReportInfo
				{
					Title = Entity.Title,
					Identifier = "Store.CarLoadDoc",
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

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			Entity.UpdateStockAmount(UoW);
			carloaddocumentview1.UpdateAmounts();
		}

	}
}

