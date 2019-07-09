using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSOrmProject;
using Vodovoz.Additions.Store;
using Vodovoz.Core.DataService;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Services;
using Vodovoz.Domain.Employees;
using QS.Services;

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
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.SelfDeliveryEdit);
			var validationResult = CheckPermission(EmployeeRepository.GetEmployeeForCurrentUser(UoW));
			if(!validationResult.CanRead) {
				MessageDialogHelper.RunErrorDialog("Нет прав для доступа к документу отпуска самовывоза");
				FailInitialize = true;
				return;
			}

			if(!validationResult.CanCreate) {
				MessageDialogHelper.RunErrorDialog("Нет прав для создания документа отпуска самовывоза");
				FailInitialize = true;
				return;
			}

			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<SelfDeliveryDocument>(id);
			var validationResult = CheckPermission(EmployeeRepository.GetEmployeeForCurrentUser(UoW));
			if(!validationResult.CanRead) {
				MessageDialogHelper.RunErrorDialog("Нет прав для доступа к документу отпуска самовывоза");
				FailInitialize = true;
				return;
			}
			canEditDocument = validationResult.CanUpdate;

			ConfigureDlg();
		}

		public SelfDeliveryDocumentDlg(SelfDeliveryDocument sub) : this(sub.Id)
		{
		}

		private IPermissionResult CheckPermission(Employee employee)
		{
			IPermissionService permissionService = ServicesConfig.PermissionService;
			return permissionService.ValidateUserPermission(typeof(SelfDeliveryDocument), Repositories.HumanResources.UserRepository.GetCurrentUser(UoW).Id);
		}

		private bool canEditDocument;

		void ConfigureDlg()
		{
			var validationResult =  CheckPermission(EmployeeRepository.GetEmployeeForCurrentUser(UoW));

			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.SelfDeliveryEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			vbox4.Sensitive = canEditDocument;
			buttonCancel.Sensitive = true;

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.SelfDeliveryEdit, Entity.Warehouse);
			yentryrefOrder.IsEditable = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = editing && canEditDocument;
			selfdeliverydocumentitemsview1.Sensitive = vBoxBottles.Sensitive = editing && canEditDocument;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.SelfDeliveryEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new OrdersFilter(UoW);
			filter.SetAndRefilterAtOnce(
				x => x.RestrictSelfDelivery = true,
				x => x.RestrictStatus = OrderStatus.OnLoading
			);
			yentryrefOrder.RepresentationModel = new ViewModel.OrdersVM(filter);
			yentryrefOrder.Binding.AddBinding(Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			yentryrefOrder.CanEditReference = UserPermissionRepository.CurrentUserPresetPermissions["can_delete"];

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

			Entity.LastEditor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);
			Entity.UpdateReceptions(UoW, BottlesReceptionList, GoodsReceptionList, new NomenclatureRepository(), new BottlesRepository());

			IStandartNomenclatures standartNomenclatures = new BaseParametersProvider();
			if(Entity.FullyShiped(UoW, standartNomenclatures))
				MessageDialogHelper.RunInfoDialog("Заказ отгружен полностью.");

			logger.Info("Сохраняем документ самовывоза...");
			UoWGeneric.Save();
			//FIXME Необходимо проверить правильность этого кода, так как если заказ именялся то уведомление на его придет и без кода.
			//А если в каком то месте нужно получать уведомления об изменениях текущего объекта, то логично чтобы этот объект на него и подписался.
			//OrmMain.NotifyObjectUpdated(new object[] { Entity.Order });
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
			Entity.FillByOrder();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyUnloaded(UoW);
			UpdateAmounts();
			UpdateWidgets();
		}

		void UpdateAmounts()
		{
			foreach(var item in Entity.Items)
				item.Amount = Math.Min(Entity.GetNomenclaturesCountInOrder(item.Nomenclature) - item.AmountUnloaded, item.AmountInStock);
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
			OrmReference refWin = new OrmReference(new NomenclatureRepository().NomenclatureOfGoodsWithoutEmptyBottlesQuery()) {
				FilterClass = null,
				Mode = OrmReferenceMode.Select
			};
			refWin.ObjectSelected += RefWin_ObjectSelected;
			this.TabParent.AddTab(refWin, this);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = (e.Subject as Nomenclature);
			if(nomenclature == null) {
				return;
			}
			var node = new GoodsReceptionVMNode {
				Category = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};
			if(!GoodsReceptionList.Any(n => n.NomenclatureId == node.NomenclatureId))
				GoodsReceptionList.Add(node);
		}
	}
}
