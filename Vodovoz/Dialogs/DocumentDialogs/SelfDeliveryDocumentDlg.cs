using System;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Repository.Store;

namespace Vodovoz
{
	public partial class SelfDeliveryDocumentDlg : OrmGtkDialogBase<SelfDeliveryDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		bool isEditingStore = false;

		public override bool HasChanges
		{
			get
			{
				if (bottlereceptionview1.Items.Sum(x => x.Amount) > 0)
					return true;
				return base.HasChanges;
			}
		}

		public SelfDeliveryDocumentDlg()
		{
			this.Build();

			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<SelfDeliveryDocument> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			if (WarehouseRepository.WarehouseByPermission(UoWGeneric) != null)
				Entity.Warehouse = WarehouseRepository.WarehouseByPermission(UoWGeneric);
			else if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				Entity.Warehouse = UoWGeneric.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);

			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<SelfDeliveryDocument> (id);
			ConfigureDlg ();
		}

		public SelfDeliveryDocumentDlg (SelfDeliveryDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			if (QSMain.User.Permissions["store_manage"])
				isEditingStore = true;
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			yentryrefWarehouse.Sensitive = isEditingStore;
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new OrdersFilter(UoW);
			filter.RestrictSelfDelivery = true;
			filter.RestrictStatus = OrderStatus.Accepted;
			yentryrefOrder.RepresentationModel = new ViewModel.OrdersVM(filter);
			yentryrefOrder.Binding.AddBinding(Entity, e => e.Order, w => w.Subject).InitializeFromSource();

			UpdateOrderInfo();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyUnloaded(UoW);
			selfdeliverydocumentitemsview1.DocumentUoW = UoWGeneric;
			bottlereceptionview1.UoW = UoW;
			UpdateWidgets();
			if (Entity.ReturnedItems.Count > 0)
				LoadReturned();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<SelfDeliveryDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			var returnes = bottlereceptionview1.Items.ToDictionary(x => x.NomenclatureId, x => (decimal)x.Amount);
			Entity.UpdateReturnedOperations(UoW, returnes);
			if (Entity.ShipIfCan())
				MessageDialogWorks.RunInfoDialog("Заказ отгружен полностью.");

			logger.Info ("Сохраняем документ самовывоза...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		void UpdateOrderInfo()
		{
			if(Entity.Order == null)
			{
				ytextviewOrderInfo.Buffer.Text = String.Empty;
				return;
			}

			string text = String.Format("Клиент: {0}\nБутылей на возврат: {1}\nАвтор заказа:{2}", 
				              Entity.Order.Client.Name,
				              Entity.Order.BottlesReturn,
				              Entity.Order.Author?.ShortName
			              );
			ytextviewOrderInfo.Buffer.Text = text;
		}

		protected void OnYentryrefOrderChangedByUser(object sender, EventArgs e)
		{
			UpdateOrderInfo();
			Entity.FillByOrder();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyUnloaded(UoW);
			UpdateAmounts();
		}

		protected void OnYentryrefWarehouseChangedByUser(object sender, EventArgs e)
		{
			Entity.UpdateStockAmount(UoW);
			UpdateAmounts();
			UpdateWidgets();
		}

		void UpdateAmounts()
		{
			foreach(var item in Entity.Items)
			{
				if (item.OrderItem != null)
					item.Amount = item.OrderItem.Count - item.AmountUnloaded;
				else
					item.Amount = 1;
				if (item.Amount > item.AmountInStock)
					item.Amount = item.AmountInStock;
			}
		}

		void UpdateWidgets()
		{
			bool bottles = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			bottlereceptionview1.Visible = bottles;
		}

		void LoadReturned()
		{
			foreach(var item in bottlereceptionview1.Items)
			{
				var returned = Entity.ReturnedItems.FirstOrDefault(x => x.Nomenclature.Id == item.NomenclatureId);
				item.Amount = returned != null ? (int)returned.Amount : 0;
			}
		}
	}
}

