using Autofac;
using Microsoft.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repository.Store;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.ViewWidgets.Store;


namespace Vodovoz
{
	public partial class CarUnloadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarUnloadDocument>
	{
		private static ILogger<CarUnloadDocumentDlg> _logger;

		private INomenclatureSettings _nomenclatureSettings;

		private IEmployeeRepository _employeeRepository;
		private ICarUnloadRepository _carUnloadRepository;
		private IRouteListRepository _routeListRepository;
		private INomenclatureRepository _nomenclatureRepository;
		
		private IWageParameterService _wageParameterService;
		private ICallTaskWorker _callTaskWorker;
		private ILifetimeScope _lifetimeScope;
		private IOrmConfig _ormConfig;
		private IEventsQrPlacer _eventsQrPlacer;

		private IStoreDocumentHelper _storeDocumentHelper;
		private IRouteListService _routeListService;

		#region Конструкторы
		public CarUnloadDocumentDlg()
		{
			ResolveDependencies();
			Build();
			ConfigureNewDoc();
			ConfigureDlg();
		}


		public CarUnloadDocumentDlg(int routeListId, int? warehouseId)
		{
			ResolveDependencies();
			Build();
			ConfigureNewDoc();

			if(warehouseId.HasValue)
			{
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);
			}

			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(int routeListId, int warehouseId, DateTime date) : this(routeListId, warehouseId)
		{
			Entity.SetTimeStamp(date);
		}

		public CarUnloadDocumentDlg(int id)
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<CarUnloadDocument>(id);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(CarUnloadDocument sub) : this(sub.Id) { }
		#endregion

		public INavigationManager NavigationManager { get; private set; }

		#region Методы

		private void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			_ormConfig = _lifetimeScope.Resolve<IOrmConfig>();
			_logger = _lifetimeScope.Resolve<ILogger<CarUnloadDocumentDlg>>();
			NavigationManager = _lifetimeScope.Resolve<INavigationManager>();

			_nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();

			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_carUnloadRepository = _lifetimeScope.Resolve<ICarUnloadRepository>();
			_routeListRepository = _lifetimeScope.Resolve<IRouteListRepository>();
			_nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
			
			_wageParameterService = _lifetimeScope.Resolve<IWageParameterService>();
			_callTaskWorker = _lifetimeScope.Resolve<ICallTaskWorker>();

			_storeDocumentHelper = _lifetimeScope.Resolve<IStoreDocumentHelper>();
			_eventsQrPlacer = _lifetimeScope.Resolve<IEventsQrPlacer>();
			
			_routeListService = _lifetimeScope.Resolve<IRouteListService>();
		}

		private void ConfigureNewDoc()
		{
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<CarUnloadDocument>();
			Entity.AuthorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			if(Entity.AuthorId == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.CarUnloadEdit);
		}

		private void ConfigureDlg()
		{
			if(_storeDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.CarUnloadEdit, Entity.Warehouse))
			{
				FailInitialize = true;
				return;
			}

			var currentUserId = ServicesConfig.UserService.CurrentUserId;
			var hasPermitionToEditDocWithClosedRL =
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_change_car_load_and_unload_docs", currentUserId);

			var editing = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.CarUnloadEdit, Entity.Warehouse);
			editing &= Entity.RouteList?.Status != RouteListStatus.Closed || hasPermitionToEditDocWithClosedRL;
			Entity.InitializeDefaultValues(UoW, _nomenclatureRepository);

			var routeListViewModel = new LegacyEEVMBuilderFactory<CarUnloadDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.RouteList)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(filter =>
				{
					filter.DisplayableStatuses = new[] { RouteListStatus.EnRoute };
				})
				.Finish();

			entryRouteList.ViewModel = routeListViewModel;
			entryRouteList.ViewModel.Changed += OnYentryrefRouteListChanged;
			OnYentryrefRouteListChanged(null, EventArgs.Empty);

			entryRouteList.Sensitive = editing;
			ytextviewCommnet.Editable = editing;
			returnsreceptionview.Sensitive = editing;
			hbxTareToReturn.Sensitive = editing;
			nonserialequipmentreceptionview1.Sensitive = editing;
			defectiveitemsreceptionview1.Sensitive = editing;

			// 20230309 Если спустя время не понадобится, то вырезать всё, что связано с этим, вместе с CarUnloadDocument.TareToReturn
			hbxTareToReturn.Visible = false;

			defectiveitemsreceptionview1.UoW =
				returnsreceptionview.UoW = UoW;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();

			var warehouseViewModel = new LegacyEEVMBuilderFactory<CarUnloadDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = _storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarUnloadEdit).Select(x => x.Id);
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			warehouseViewModel.IsEditable = editing;

			entryWarehouse.ViewModel = warehouseViewModel;

			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			routeListViewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			Entity.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == nameof(Entity.Warehouse))
				{
					OnWarehouseChanged();
				}

				if(e.PropertyName == nameof(Entity.RouteList))
				{
					UpdateWidgetsVisible();
				}
			};

			lblTareReturnedBefore.Binding.AddFuncBinding(Entity, e => e.ReturnedTareBeforeText, w => w.Text).InitializeFromSource();
			spnTareToReturn.Binding.AddBinding(Entity, e => e.TareToReturn, w => w.ValueAsInt).InitializeFromSource();

			defectiveitemsreceptionview1.Warehouse = returnsreceptionview.Warehouse = Entity.Warehouse;

			UpdateWidgetsVisible();
			buttonSave.Sensitive = editing;
			if(!editing)
			{
				HasChanges = false;
			}

			if(!UoW.IsNew)
			{
				LoadReception();
			}

			var permmissionValidator =
				new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);

			Entity.CanEdit =
				permmissionValidator.Validate(typeof(CarUnloadDocument), currentUserId, nameof(RetroactivelyClosePermission));

			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryRouteList.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entryWarehouse.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewRouteListInfo.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				spnTareToReturn.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				defectiveitemsreceptionview1.Sensitive = false;
				nonserialequipmentreceptionview1.Sensitive = false;
				returnsreceptionview.Sensitive = false;

				buttonSave.Sensitive = false;
			}
			else
			{
				Entity.CanEdit = true;
			}

			spnTareToReturn.ValueChanged += (sender, e) => HasChanges = true;
			((GenericObservableList<ReceptionItemNode>)returnsreceptionview.Items).ListContentChanged += (sender, e) => HasChanges = true;
			((GenericObservableList<ReceptionNonSerialEquipmentItemNode>)nonserialequipmentreceptionview1.Items).ListContentChanged +=
				(sender, e) => HasChanges = true;
			((GenericObservableList<DefectiveItemNode>)defectiveitemsreceptionview1.Items).ListContentChanged += (sender, e) => HasChanges = true;

			nonserialequipmentreceptionview1.Container = this;
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
			{
				return false;
			}

			if(!UpdateReceivedItemsOnEntity(_nomenclatureSettings.NomenclatureIdForTerminal))
			{
				return false;
			}

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			if(!_carUnloadRepository.IsUniqueDocumentAtDay(UoW, Entity.RouteList, Entity.Warehouse, Entity.Id))
			{
				MessageDialogHelper.RunWarningDialog("Документ по данному МЛ и складу уже сформирован");
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditorId == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if(Entity.RouteList.Status == RouteListStatus.Delivered)
			{
				 _routeListService.CompleteRouteAndCreateTask(UoW, Entity.RouteList, _wageParameterService, _callTaskWorker);
			}

			try
			{
				_logger.LogInformation("Сохраняем талон разгрузки авто по МЛ {RouteListId}", Entity.RouteList?.Id);
				UoWGeneric.Save();
				_logger.LogInformation("Талон разгрузки авто успешно сохранен");
			}
			catch(NHibernate.StaleObjectStateException ex)
			{
				var type = _ormConfig.FindMappingByFullClassName(ex.EntityName).MappedClass;
				var objectName = DomainHelper.GetSubjectNames(type)?.Nominative ?? type.Name;

				_logger.LogError(
					ex,
					"Ошибка при сохранении талона разгрузки. Версия документа {DocumentTypeName} №{DocumentId} устарела",
					objectName,
					ex.Identifier);

				MessageDialogHelper.RunErrorDialog(
					$"Ошибка при сохранении талона разгрузки" +
					$"\nВерсия документа {objectName} №{ex.Identifier} устарела" +
					$"\nВаши изменения не будут записаны, чтобы не потерять чужие изменения" +
					$"\nПереоткройте вкладку");
				
				return false;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении талона разгрузки");
				MessageDialogHelper.RunErrorDialog(
					"Ошибка при сохранении талона разгрузки" +
					"\nПереоткройте вкладку");
				return false;
			}

			return true;
		}

		private void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null)
			{
				ytextviewRouteListInfo.Buffer.Text = string.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				string.Format("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
					Entity.RouteList.Id,
					Entity.RouteList.Date,
					Entity.RouteList.Driver.FullName,
					Entity.RouteList.Car.CarModel.Name,
					Entity.RouteList.Car.RegistrationNumber,
					Entity.RouteList.Forwarder != null ? Entity.RouteList.Forwarder.FullName : "(Отсутствует)"
				);
		}

		private void FillOtherReturnsTable()
		{
			if(Entity.RouteList == null || Entity.Warehouse == null)
			{
				return;
			}

			Dictionary<int, decimal> returns = _carUnloadRepository.NomenclatureUnloaded(UoW, Entity.RouteList, Entity.Warehouse, Entity);

			treeOtherReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Nomenclature>()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Количество").AddNumericRenderer(x => returns[x.Id], new RoundedDecimalToStringConverter())
				.Finish();

			Nomenclature nomenclatureAlias = null;

			var query = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
						   .WhereRestrictionOn(() => nomenclatureAlias.Id)
						   .IsIn(returns.Keys)
						   .List<Nomenclature>();

			treeOtherReturns.ItemsDataSource = query;
		}

		private void SetupForNewRouteList()
		{
			UpdateRouteListInfo();

			nonserialequipmentreceptionview1.RouteList =
				defectiveitemsreceptionview1.RouteList =
					returnsreceptionview.RouteList = Entity.RouteList;
		}

		private void UpdateWidgetsVisible()
		{
			//20230320 Если не понадобится после обновления, вырезать всё, что с этим связано
			lblTareReturnedBefore.Visible = false; // Entity.RouteList != null;
			nonserialequipmentreceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
		}

		private void LoadReception()
		{
			foreach(var item in Entity.Items)
			{
				if(defectiveitemsreceptionview1.Items.Any(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id))
				{
					continue;
				}

				var returned =
					returnsreceptionview.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);

				if(returned != null)
				{
					returned.Amount = (int)item.GoodsAccountingOperation.Amount;
					returned.Redhead = item.Redhead;
					continue;
				}

				switch(item.ReciveType)
				{
					case ReciveTypes.Equipment:
						var equipmentByNomenclature = nonserialequipmentreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
						if(equipmentByNomenclature != null)
						{
							equipmentByNomenclature.Amount = (int)item.GoodsAccountingOperation.Amount;
							continue;
						}
						nonserialequipmentreceptionview1.Items.Add(
							new ReceptionNonSerialEquipmentItemNode
							{
								NomenclatureCategory = NomenclatureCategory.equipment,
								NomenclatureId = item.GoodsAccountingOperation.Nomenclature.Id,
								Amount = (int)item.GoodsAccountingOperation.Amount,
								Name = item.GoodsAccountingOperation.Nomenclature.Name
							}
						);
						continue;
					case ReciveTypes.Bottle:
					case ReciveTypes.Returnes:
					case ReciveTypes.ReturnCashEquipment:
						break;
					case ReciveTypes.Defective:
						var defective = defectiveitemsreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
						if(defective != null)
						{
							defective.Amount = (int)item.GoodsAccountingOperation.Amount;
							continue;
						}
						defectiveitemsreceptionview1.Items.Add(
							new DefectiveItemNode
							{
								NomenclatureCategory = item.GoodsAccountingOperation.Nomenclature.Category,
								NomenclatureId = item.GoodsAccountingOperation.Nomenclature.Id,
								Amount = (int)item.GoodsAccountingOperation.Amount,
								Name = item.GoodsAccountingOperation.Nomenclature.Name,
								Source = item.DefectSource,
								TypeOfDefect = item.TypeOfDefect
							}
						);
						continue;
				}

				_logger.LogWarning("Номенклатура {@Nomenclature} не найдена в заказах мл, добавляем отдельно...", item.GoodsAccountingOperation.Nomenclature);
				var newItem = new ReceptionItemNode(item);
				returnsreceptionview.AddItem(newItem);
			}
		}

		private bool UpdateReceivedItemsOnEntity(int terminalId)
		{
			//Собираем список всего на возврат из разных виджетов.
			var tempItemList = new List<InternalItem>();
			if(Entity.TareToReturn > 0)
			{
				tempItemList.Add(
					new InternalItem
					{
						ReciveType = ReciveTypes.Bottle,
						NomenclatureId = Entity.DefBottleId,
						Amount = Entity.TareToReturn
					}
				);
			}

			var defectiveItemsList = new List<InternalItem>();
			foreach(var node in defectiveitemsreceptionview1.Items)
			{
				if(node.Amount == 0)
				{
					continue;
				}

				var item = new InternalItem
				{
					ReciveType = ReciveTypes.Defective,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					MovementOperationId = node.MovementOperation?.Id ?? 0,
					TypeOfDefect = node.TypeOfDefect,
					Source = node.Source
				};

				if(!defectiveItemsList.Any(i => i.EqualsToAnotherInternalItem(item)))
				{
					defectiveItemsList.Add(item);
				}
			}

			foreach(var node in returnsreceptionview.Items)
			{
				if(node.Amount == 0)
				{
					continue;
				}

				var item = new InternalItem
				{
					ReciveType = node.NomenclatureId == terminalId
						? ReciveTypes.ReturnCashEquipment
						: ReciveTypes.Returnes,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					Redhead = node.Redhead
				};

				tempItemList.Add(item);
			}

			foreach(var node in nonserialequipmentreceptionview1.Items)
			{
				if(node.Amount == 0)
				{
					continue;
				}

				var item = new InternalItem
				{
					ReciveType = ReciveTypes.Equipment,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount
				};
				tempItemList.Add(item);
			}

			//Обновляем Entity
			foreach(var tempItem in defectiveItemsList)
			{
				//валидация брака
				if(tempItem.TypeOfDefect == null)
				{
					MessageDialogHelper.RunWarningDialog("Для брака необходимо указать его вид");
					return false;
				}

				//проверка на дубли. если несколько одинаковых, то устанавливаем кол-во в 0 для последующего удаления из коллекции
				if(tempItem.Amount > 0 && defectiveItemsList.Count(i => i.EqualsToAnotherInternalItem(tempItem)) > 1)
				{
					tempItem.Amount = 0;
				}
			}

			foreach(var tempItem in defectiveItemsList)
			{
				var item = Entity.Items.FirstOrDefault(x => x.GoodsAccountingOperation.Id > 0 && x.GoodsAccountingOperation.Id == tempItem.MovementOperationId);
				if(item == null)
				{
					Entity.AddItem(
						tempItem.ReciveType,
						UoW.GetById<Nomenclature>(tempItem.NomenclatureId),
						null,
						tempItem.Amount,
						null,
						terminalId,
						null,
						tempItem.Source,
						tempItem.TypeOfDefect
					);
				}
				else
				{
					if(item.GoodsAccountingOperation.Amount != tempItem.Amount)
					{
						item.GoodsAccountingOperation.Amount = tempItem.Amount;
					}

					if(item.TypeOfDefect != tempItem.TypeOfDefect)
					{
						item.TypeOfDefect = tempItem.TypeOfDefect;
					}

					if(item.DefectSource != tempItem.Source)
					{
						item.DefectSource = tempItem.Source;
					}
				}
			}

			var nomenclatures = UoW.GetById<Nomenclature>(tempItemList.Select(x => x.NomenclatureId).ToArray());
			foreach(var tempItem in tempItemList)
			{
				var item = Entity.Items.FirstOrDefault(x => x.GoodsAccountingOperation.Nomenclature.Id == tempItem.NomenclatureId);
				if(item == null)
				{
					var nomenclature = nomenclatures.First(x => x.Id == tempItem.NomenclatureId);
					Entity.AddItem(
						tempItem.ReciveType,
						nomenclature,
						null,
						tempItem.Amount,
						null,
						terminalId,
						tempItem.Redhead
					);
				}
				else
				{
					if(item.GoodsAccountingOperation.Amount != tempItem.Amount)
					{
						item.GoodsAccountingOperation.Amount = tempItem.Amount;
					}

					if(item.EmployeeNomenclatureMovementOperation != null && item.EmployeeNomenclatureMovementOperation.Amount != -tempItem.Amount)
					{
						item.EmployeeNomenclatureMovementOperation.Amount = -tempItem.Amount;
					}

					if(item.Redhead != tempItem.Redhead)
					{
						item.Redhead = tempItem.Redhead;
					}

					item.CreateOrUpdateDeliveryFreeBalanceOperation(terminalId);
				}
			}

			foreach(var item in Entity.Items.ToList())
			{
				bool exist = true;
				if(item.ReciveType != ReciveTypes.Defective)
				{
					exist = tempItemList.Any(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature?.Id);
				}
				else
				{
					exist = defectiveItemsList.Any(x => x.MovementOperationId == item.GoodsAccountingOperation.Id && x.Amount > 0);
				}

				if(!exist)
				{
					var freeBalanceOperation = item.DeliveryFreeBalanceOperation;

					//т.к. при удалении строки разгрузки, пытается удалиться связанная строка свободных остатков,
					//то ее нужно убрать из всех связанных коллекций, чтобы она по каскаду не стала вновь сохраняться
					if(freeBalanceOperation != null)
					{
						Entity.RouteList.ObservableDeliveryFreeBalanceOperations.Remove(freeBalanceOperation);
					}
					
					UoW.Delete(item.GoodsAccountingOperation);
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
			{
				Save();
			}

			var reportInfo = _eventsQrPlacer.AddQrEventForPrintingDocument(
				UoW, Entity.Id, Entity.Title, EventQrDocumentType.CarUnloadDocument);

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}

		protected void OnWarehouseChanged()
		{
			UpdateWidgetsVisible();
			returnsreceptionview.Warehouse = Entity.Warehouse;
			FillOtherReturnsTable();
		}

		protected void OnYentryrefRouteListChanged(object sender, EventArgs e)
		{
			SetupForNewRouteList();
			FillOtherReturnsTable();
			if(Entity.RouteList != null)
			{
				Entity.ReturnedEmptyBottlesBefore(UoW, _routeListRepository);
			}
		}
		#endregion

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}

		private class InternalItem
		{
			public ReciveTypes ReciveType;
			public int NomenclatureId;

			public decimal Amount;
			public string Redhead;

			public DefectSource Source;
			public CullingCategory TypeOfDefect;

			public int MovementOperationId;

			public bool EqualsToAnotherInternalItem(InternalItem item)
			{
				if(item.TypeOfDefect == null || TypeOfDefect == null)
				{
					return false;
				}

				bool eq = item.ReciveType == ReciveType;
				eq &= item.Source == Source;
				eq &= item.NomenclatureId == NomenclatureId;
				eq &= item.TypeOfDefect.Id == TypeOfDefect.Id;
				return eq;
			}
		}
	}
}

