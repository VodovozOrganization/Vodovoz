using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class TerminalManagementViewModel : UoWWidgetViewModelBase
	{
		private readonly Employee _driver;
		private readonly Employee _author;
		private readonly ITdiTab _parentTab;
		private readonly bool _canManageTerminal;
		private readonly Warehouse _defaultWarehouse;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly Vodovoz.Settings.Nomenclature.INomenclatureSettings _nomenclatureSettings;
		private string _title;
		private int _terminalId;
		private DriverAttachedTerminalDocumentBase _entity;
		private AttachedTerminalDocumentType _documentTypeToCreate;

		public TerminalManagementViewModel(
			Warehouse defaultWarehouse,
			Employee driver,
			ITdiTab parentTab,
			IEmployeeRepository employeeRepository,
			IWarehouseRepository warehouseRepository,
			IRouteListRepository routeListRepository,
			ICommonServices commonServices,
			IUnitOfWork uow,
			IUnitOfWorkFactory unitOfWorkFactory,
			Vodovoz.Settings.Nomenclature.INomenclatureSettings nomenclatureSettings)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			_driver = driver ?? throw new ArgumentNullException(nameof(driver));
			_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_defaultWarehouse = defaultWarehouse;

			_author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			UpdateEntityAndRelatedProperties(
				_driver.Id > 0
				? _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, _driver)
				: null, false);
			_canManageTerminal = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);
		}

		#region Свойства
		public bool HasChanges { get; private set; }

		public string Title
		{
			get => _title;
			private set
			{
				if(value != null)
				{
					SetField(ref _title, value);
					return;
				}
				SetField(ref _title, "Терминал не привязан");
			}
		}

		/// <summary>
		/// Видимость кнопок управления документами (выдача/возврат)
		/// </summary>
		public AttachedTerminalDocumentType DocumentTypeToCreate
		{
			get => _documentTypeToCreate;
			private set => SetField(ref _documentTypeToCreate, value);
		}
		#endregion

		#region Методы
		/// <summary>
		/// Сохраняет сущности этой viewModel
		/// </summary>
		public void SaveChanges()
		{
			if(!HasChanges)
			{
				return;
			}
			UoW.Save(_entity);
		}

		public void ReturnTerminal()
		{
			if(!RunChecksBeforeExecution()) {return;}
			if(HasChanges && _entity is DriverAttachedTerminalGiveoutDocument)
			{
				UpdateEntityAndRelatedProperties(_routeListRepository.GetLastTerminalDocumentForEmployee(UoW, _driver), false);
			}
			else
			{
				var income = _entity.GoodsAccountingOperation.Warehouse;
				var returnDocument = new DriverAttachedTerminalReturnDocument
				{
					CreationDate = DateTime.Now,
					Author = _author,
					Driver = _driver
				};
				_terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
				returnDocument.CreateMovementOperations(income, UoW.GetById<Nomenclature>(_terminalId));

				UpdateEntityAndRelatedProperties(returnDocument, true);
			}
		}

		public void GiveoutTerminal()
		{
			if(!RunChecksBeforeExecution())
			{
				return;
			}
			_terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
			var terminal = UoW.GetById<Nomenclature>(_terminalId);
			var filter = new NomenclatureBalanceByStockFilterViewModel(_warehouseRepository)
			{
				Warehouse = _defaultWarehouse,
				CanChangeWarehouse = true,
				Nomenclature = terminal
			};
			var writeoffWarehouseJournal =
				new NomenclatureBalanceByStockJournalViewModel(filter, _unitOfWorkFactory, _commonServices)
				{
					TabName = "Выбор склада для списания терминала",
					SelectionMode = JournalSelectionMode.Single
				};
			writeoffWarehouseJournal.OnEntitySelectedResult += OnWriteoffWarehouseSelected;
			_parentTab.TabParent.AddSlaveTab(_parentTab, writeoffWarehouseJournal);
		}

		private void OnWriteoffWarehouseSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			if(e.SelectedNodes.FirstOrDefault() is NomenclatureBalanceByStockJournalNode node)
			{
				var creationDate = DateTime.Now;
				var writeoff = UoW.GetById<Warehouse>(node.Id);
				var terminal = UoW.GetById<Nomenclature>(_terminalId);
				var giveoutDocument = new DriverAttachedTerminalGiveoutDocument
				{
					CreationDate = creationDate,
					Author = _author,
					Driver = _driver
				};
				_terminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
				giveoutDocument.CreateMovementOperations(writeoff, terminal);

				UpdateEntityAndRelatedProperties(giveoutDocument, true);
			}
		}

		private void UpdateEntityAndRelatedProperties(DriverAttachedTerminalDocumentBase doc, bool hasChanges)
		{
			_entity = doc;
			DocumentTypeToCreate = _entity is DriverAttachedTerminalReturnDocument || _entity == null
				? AttachedTerminalDocumentType.Giveout
				: AttachedTerminalDocumentType.Return;
			Title = _entity?.ToString();
			HasChanges = hasChanges;
		}

		private bool RunChecksBeforeExecution()
		{
			if(!_canManageTerminal)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Нет прав кассира для управления терминалом водителя");
				return false;
			}

			if(_driver.Id == 0)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Необходимо сохранить нового сотрудника перед проведением операций по выдаче/возврату терминала");
				return false;
			}

			if(HasChanges && _entity is DriverAttachedTerminalReturnDocument)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Имеется несохраненная операция возврата терминала. " +
					"Необходимо сохранить сотрудника перед проведением следующей операции.");
				return false;
			}

			return true;
		}
		#endregion
	}
}
