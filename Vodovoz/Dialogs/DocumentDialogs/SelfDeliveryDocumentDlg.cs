using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz
{
	public partial class SelfDeliveryDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<SelfDeliveryDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public override bool HasChanges => BottlesReceptionList.Sum(x => x.Amount) >= 0 
		                                                       || GoodsReceptionList.Sum(x => x.Amount) >= 0 
		                                                       || base.HasChanges;

		GenericObservableList<GoodsReceptionVMNode> BottlesReceptionList;
		GenericObservableList<GoodsReceptionVMNode> GoodsReceptionList = new GenericObservableList<GoodsReceptionVMNode>();

		public SelfDeliveryDocumentDlg()
		{
			this.Build();

			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<SelfDeliveryDocument>();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.SelfDeliveryEdit);
			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<SelfDeliveryDocument>(id);
			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(SelfDeliveryDocument sub) : this(sub.Id)
		{
		}

		void ConfigureDlg()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.SelfDeliveryEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.SelfDeliveryEdit, Entity.Warehouse);
			yentryrefOrder.IsEditable = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = editing;
			selfdeliverydocumentitemsview1.Sensitive = vBoxBottles.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.SelfDeliveryEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new OrdersFilter(UoW);
			filter.SetAndRefilterAtOnce(
				x => x.RestrictSelfDelivery = true,
				x => x.RestrictStatus = OrderStatus.Accepted
			);
			yentryrefOrder.RepresentationModel = new ViewModel.OrdersVM(filter);
			yentryrefOrder.Binding.AddBinding(Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			yentryrefOrder.CanEditReference = QSMain.User.Permissions["can_delete"];
			              
			UpdateOrderInfo();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyUnloaded(UoW);
			selfdeliverydocumentitemsview1.DocumentUoW = UoWGeneric;
			//bottlereceptionview1.UoW = UoW;
			UpdateWidgets();

			IColumnsConfig bottlesColumnsConfig = FluentColumnsConfig<GoodsReceptionVMNode>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Amount).WidthChars(3)
				.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 100, 0))
				.Editing(true)
				.AddColumn("")
				.Finish();
			yTreeBottles.ColumnsConfig = bottlesColumnsConfig;
			FillTrees();

			IColumnsConfig goodsColumnsConfig = FluentColumnsConfig<GoodsReceptionVMNode>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Amount)
				.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 100, 0))
				.Editing(true)
				.AddColumn("Категория").AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("")
				.Finish();
			yTreeOtherGoods.ColumnsConfig = goodsColumnsConfig;
			yTreeOtherGoods.ItemsDataSource = GoodsReceptionList;
		}

		void FillTrees()
		{
			GoodsReceptionVMNode resultAlias = null;
			Nomenclature nomenclatureAlias = null;

			var orderBottles = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
								  .Where(n => n.Category == NomenclatureCategory.bottle)
								  .SelectList(list => list
											  .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
											  .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
											 ).TransformUsing(Transformers.AliasToBean<GoodsReceptionVMNode>())
								  .List<GoodsReceptionVMNode>();
			BottlesReceptionList = new GenericObservableList<GoodsReceptionVMNode>(orderBottles);
			yTreeBottles.ItemsDataSource = BottlesReceptionList;

			if(Entity.ReturnedItems.Any())
				LoadReturned();
		}

		void LoadReturned()
		{
			foreach(GoodsReceptionVMNode item in BottlesReceptionList) {
				var returned = Entity.ReturnedItems.FirstOrDefault(x => x.Nomenclature.Id == item.NomenclatureId);
				item.Amount = returned != null ? (int)returned.Amount : 0;
			}

			GoodsReceptionList.Clear();
			foreach(var item in Entity.ReturnedItems) {
				if(item.Nomenclature.Category != NomenclatureCategory.bottle)
					GoodsReceptionList.Add(new GoodsReceptionVMNode {
						NomenclatureId = item.Nomenclature.Id,
						Name = item.Nomenclature.Name,
						Category = item.Nomenclature.Category,
						Amount = (int)item.Amount
					});
			}
		}

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<SelfDeliveryDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			foreach(GoodsReceptionVMNode item in BottlesReceptionList) {
				Entity.UpdateReturnedOperation(UoW, item.NomenclatureId, item.Amount);
			}
			foreach(GoodsReceptionVMNode item in GoodsReceptionList) {
				Entity.UpdateReturnedOperation(UoW, item.NomenclatureId, item.Amount);
			}
			if(Entity.FullyShiped(UoW))
				MessageDialogWorks.RunInfoDialog("Заказ отгружен полностью.");

			logger.Info("Сохраняем документ самовывоза...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		void UpdateOrderInfo()
		{
			if(Entity.Order == null) {
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
			foreach(var item in Entity.Items) {
				if(item.OrderItem != null)
					item.Amount = item.OrderItem.Count - item.AmountUnloaded;
				else
					item.Amount = 1;
				if(item.Amount > item.AmountInStock)
					item.Amount = item.AmountInStock;
			}
		}

		void UpdateWidgets()
		{
			bool bottles = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			bool goods = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
			vBoxBottles.Visible = bottles;
			vBoxOtherGoods.Visible = goods;
		}

		protected void OnBtnAddOtherGoodsClicked(object sender, EventArgs e)
		{
			OrmReference refWin = new OrmReference(NomenclatureRepository.NomenclatureOfGoodsWithoutEmptyBottlesQuery());
			refWin.FilterClass = null;
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			this.TabParent.AddTab(refWin, this);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = (e.Subject as Nomenclature);
			if(nomenclature == null) {
				return;
			}
			var node = new GoodsReceptionVMNode() {
				Category = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};
			if(!GoodsReceptionList.Any(n => n.NomenclatureId == node.NomenclatureId))
				GoodsReceptionList.Add(node);
		}
	}

	public class GoodsReceptionVMNode
	{
		public int NomenclatureId { get; set; }
		public string Name { get; set; }
		public int Amount { get; set; }
		public NomenclatureCategory Category { get; set; }
	}
}
