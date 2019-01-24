using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Repositories.Store;
using Vodovoz.ViewWidgets.Store;

namespace Vodovoz
{
	public partial class CarUnloadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarUnloadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		IList<Equipment> alreadyUnloadedEquipment;

		public override bool HasChanges => true;

		#region Конструкторы
		public CarUnloadDocumentDlg()
		{
			this.Build();
			ConfigureNewDoc();
			ConfigureDlg();
		}


		public CarUnloadDocumentDlg(int routeListId, int? warehouseId)
		{
			this.Build();
			ConfigureNewDoc();

			if(warehouseId.HasValue)
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarUnloadDocument>(id);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(CarUnloadDocument sub) : this(sub.Id)
		{}
		#endregion

		#region Методы

		void ConfigureNewDoc()
		{
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarUnloadDocument>();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.CarUnloadEdit);
		}

		void ConfigureDlg()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.CarUnloadEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.CarUnloadEdit, Entity.Warehouse);
			yentryrefRouteList.IsEditable = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = editing;
			returnsreceptionview1.Sensitive = 
				bottlereceptionview1.Sensitive = 
					nonserialequipmentreceptionview1.Sensitive = 
						defectiveitemsreceptionview1.Sensitive = editing;
			
			bottlereceptionview1.UoW = 
				defectiveitemsreceptionview1.UoW = 
					returnsreceptionview1.UoW = UoW;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.CarUnloadEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictStatus = RouteListStatus.EnRoute);
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryrefRouteList.CanEditReference = QSMain.User.Permissions["can_delete"];

			defectiveitemsreceptionview1.Warehouse = returnsreceptionview1.Warehouse = Entity.Warehouse;

			UpdateWidgetsVisible();
			if(!UoW.IsNew)
				LoadReception();
		}

		public override bool Save()
		{
			if(!CarUnloadRepository.IsUniqDocument(UoW,Entity.RouteList,Entity.Warehouse,Entity.Id)) 
			{
				MessageDialogWorks.RunErrorDialog("Документ по данному МЛ и складу уже сформирован");
				return false;
			}

			if(!UpdateReceivedItemsOnEntity())
				return false;
				
			var valid = new QSValidation.QSValidator<CarUnloadDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info("Сохраняем разгрузочный талон...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null) {
				ytextviewRouteListInfo.Buffer.Text = String.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				String.Format("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
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
			returnsreceptionview1.AlreadyUnloadedEquipment = alreadyUnloadedEquipment;
		}

		void fillOtherReturnsTable()
		{
			if(Entity.RouteList == null || Entity.Warehouse == null)
				return;
			Dictionary<int, decimal> returns = Repositories.Store.CarUnloadRepository.NomenclatureUnloaded(UoW, Entity.RouteList, Entity.Warehouse, Entity);

			treeOtherReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Nomenclature>()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Количество").AddTextRenderer(x => ((int)returns[x.Id]).ToString())
				.Finish();

			Nomenclature nomenclatureAlias = null;

			var query = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
						   .WhereRestrictionOn(() => nomenclatureAlias.Id)
						   .IsIn(returns.Keys)
						   .List<Nomenclature>();

			treeOtherReturns.ItemsDataSource = query;
		}

		void SetupForNewRouteList()
		{
			UpdateRouteListInfo();
			if(Entity.RouteList != null) {
				UpdateAlreadyUnloaded();
			}
			nonserialequipmentreceptionview1.RouteList = 
				defectiveitemsreceptionview1.RouteList =
					returnsreceptionview1.RouteList = Entity.RouteList;
		}

		private void UpdateWidgetsVisible()
		{
			bottlereceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveBottles;
			nonserialequipmentreceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
		}

		void LoadReception()
		{
			foreach(var item in Entity.Items) {
				var bottle = bottlereceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
				if(bottle != null) {
					bottle.Amount = (int)item.MovementOperation.Amount;
					continue;
				}

				if(defectiveitemsreceptionview1.Items.Any(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id))
					continue;

				var returned = item.MovementOperation.Equipment != null
					? returnsreceptionview1.Items.FirstOrDefault(x => x.EquipmentId == item.MovementOperation.Equipment.Id)
					: returnsreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
				if(returned != null) {
					returned.Amount = (int)item.MovementOperation.Amount;
					returned.Redhead = item.Redhead;
					continue;
				}

				if(item.ReciveType == ReciveTypes.Equipment) {
					var equipmentByNomenclature = nonserialequipmentreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.MovementOperation.Nomenclature.Id);
					if(equipmentByNomenclature != null) {
						equipmentByNomenclature.Amount = (int)item.MovementOperation.Amount;
						continue;
					} else {
						nonserialequipmentreceptionview1.Items.Add(new ReceptionNonSerialEquipmentItemNode {
							NomenclatureCategory = NomenclatureCategory.equipment,
							NomenclatureId = item.MovementOperation.Nomenclature.Id,
							Amount = (int)item.MovementOperation.Amount,
							Name = item.MovementOperation.Nomenclature.Name
						});
						continue;
					}
				}

				logger.Warn("Номенклатура {0} не найдена в заказа мл, добавляем отдельно...", item.MovementOperation.Nomenclature);
				var newItem = new ReceptionItemNode(item);
				if(item.MovementOperation.Equipment != null) {
					newItem.EquipmentId = item.MovementOperation.Equipment.Id;
				}
				returnsreceptionview1.AddItem(newItem);
			}

			foreach(var item in bottlereceptionview1.Items) {
				var returned = Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == item.NomenclatureId);
				item.Amount = returned != null ? (int)returned.MovementOperation.Amount : 0;
			}
		}

		bool UpdateReceivedItemsOnEntity()
		{
			//Собираем список всего на возврат из разных виджетов.
			var tempItemList = new List<InternalItem>();
			foreach(var node in bottlereceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Bottle,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount
				};
				tempItemList.Add(item);
			}

			var defectiveItemsList = new List<InternalItem>();
			foreach(var node in defectiveitemsreceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Defective,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					MovementOperationId = node.MovementOperation != null ? node.MovementOperation.Id : 0,
					TypeOfDefect = node.TypeOfDefect,
					Source = node.Source
				};

				if(!defectiveItemsList.Any(i => i.EqualsToAnotherInternalItem(item)))
					defectiveItemsList.Add(item);
			}

			foreach(var node in returnsreceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Returnes,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					Redhead = node.Redhead
				};
				tempItemList.Add(item);
			}

			foreach(var node in nonserialequipmentreceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Equipment,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount
				};
				tempItemList.Add(item);
			}

			//Обновляем Entity
			foreach(var tempItem in defectiveItemsList) {
				//валидация брака
				if(tempItem.TypeOfDefect == null) {
					MessageDialogWorks.RunWarningDialog("Для брака необходимо указать его вид");
					return false;
				}

				//проверка на дубли. если несколько одинаковых, то устанавливаем кол-во в 0 для последующего удаления из коллекции
				if(tempItem.Amount > 0 && defectiveItemsList.Count(i => i.EqualsToAnotherInternalItem(tempItem)) > 1)
					tempItem.Amount = 0;
			}

			foreach(var tempItem in defectiveItemsList) {
				var item = Entity.Items.FirstOrDefault(x => x.MovementOperation.Id > 0 && x.MovementOperation.Id == tempItem.MovementOperationId);
				if(item == null) {
					Entity.AddItem(
						tempItem.ReciveType,
						UoW.GetById<Nomenclature>(tempItem.NomenclatureId),
						null,
						tempItem.Amount,
						null,
						null,
						tempItem.Source,
						tempItem.TypeOfDefect
					);
				} else {
					if(item.MovementOperation.Amount != tempItem.Amount)
						item.MovementOperation.Amount = tempItem.Amount;
					if(item.TypeOfDefect != tempItem.TypeOfDefect)
						item.TypeOfDefect = tempItem.TypeOfDefect;
					if(item.Source != tempItem.Source)
						item.Source = tempItem.Source;
				}
			}

			var nomenclatures = UoW.GetById<Nomenclature>(tempItemList.Select(x => x.NomenclatureId).ToArray());
			foreach(var tempItem in tempItemList) {
				var item = Entity.Items.FirstOrDefault(x => x.MovementOperation.Nomenclature.Id == tempItem.NomenclatureId);
				if(item == null) {
					var nomenclature = nomenclatures.First(x => x.Id == tempItem.NomenclatureId);
					Entity.AddItem(
						tempItem.ReciveType,
						nomenclature,
						null,
						tempItem.Amount,
						null,
						tempItem.Redhead
					);
				} else {
					if(item.MovementOperation.Amount != tempItem.Amount)
						item.MovementOperation.Amount = tempItem.Amount;
					if(item.Redhead != tempItem.Redhead)
						item.Redhead = tempItem.Redhead;
				}
			}

			foreach(var item in Entity.Items.ToList()) {
				bool exist = true;
				if(item.ReciveType != ReciveTypes.Defective)
					exist = tempItemList.Any(x => x.NomenclatureId == item.MovementOperation.Nomenclature?.Id);
				else
					exist = defectiveItemsList.Any(x => x.MovementOperationId == item.MovementOperation.Id && x.Amount > 0);

				if(!exist) {
					UoW.Delete(item.MovementOperation);
					Entity.ObservableItems.Remove(item);
				}
			}

			return true;
		}
		#endregion

		#region События
		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(CarUnloadDocument), "талона"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = Entity.Title,
				Identifier = "Store.CarUnloadDoc",
				Parameters = new Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}

		protected void OnYentryrefWarehouseChanged(object sender, EventArgs e)
		{
			UpdateWidgetsVisible();
			returnsreceptionview1.Warehouse = Entity.Warehouse;
			fillOtherReturnsTable();
		}

		protected void OnYentryrefRouteListChanged(object sender, EventArgs e)
		{
			SetupForNewRouteList();
			fillOtherReturnsTable();
		}
		#endregion

		class InternalItem
		{
			public ReciveTypes ReciveType;
			public int NomenclatureId;

			public decimal Amount;
			public string Redhead;

			public DefectSource Source;
			public CullingCategory TypeOfDefect;

			public int MovementOperationId;

			public bool EqualsToAnotherInternalItem(InternalItem item){
				if(item.TypeOfDefect == null || TypeOfDefect == null)
					return false;
				bool eq = item.ReciveType == ReciveType;
				eq &= item.Source == Source;
				eq &= item.NomenclatureId == NomenclatureId;
				eq &= item.TypeOfDefect.Id == TypeOfDefect.Id;
				return eq;
			}
		}
	}
}

