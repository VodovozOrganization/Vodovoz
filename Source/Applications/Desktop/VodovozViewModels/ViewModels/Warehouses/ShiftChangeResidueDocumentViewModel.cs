using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class ShiftChangeResidueDocumentViewModel : EntityTabViewModelBase<ShiftChangeWarehouseDocument>
	{
		private const string _updateResidues = "Обновить остатки";
		private const string _fillByWarehouse = "Заполнить по складу";
		private const string _fillByCar = "Заполнить по автомобилю";
		private readonly CommonMessages _commonMessages;
		private readonly IStoreDocumentHelper _documentHelper;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly ILifetimeScope _scope;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IEntityExtendedPermissionValidator _extendedPermissionValidator;
		private bool _isInstanceAccountingActive;
		private bool _isBulkAccountingActive;
		private int _activeAccounting;
		private string _instancesDiscrepanciesString;
		private Employee _currentEmployee;
		private SelectableParametersReportFilter _selectableFilter;

		private DelegateCommand _printCommand;
		private DelegateCommand _fillNomenclatureItemsByStorageCommand;
		private DelegateCommand _addMissingNomenclatureCommand;
		private DelegateCommand _fillNomenclatureInstanceItemsCommand;
		private DelegateCommand _addMissingNomenclatureInstanceCommand;
		private Dictionary<int, string> _instancesDiscrepancies = new Dictionary<int, string>();
		private IEnumerable<int> _availableWarehousesIdsForCreate;
		private IEnumerable<int> _availableWarehousesIdsForEdit;

		public ShiftChangeResidueDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IStoreDocumentHelper documentHelper,
			IStockRepository stockRepository,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			IReportViewOpener reportViewOpener,
			IEntityExtendedPermissionValidator extendedPermissionValidator,
			ILifetimeScope scope,
			IReportInfoFactory reportInfoFactory
			)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_documentHelper = documentHelper ?? throw new ArgumentNullException(nameof(documentHelper));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureInstanceRepository =
				nomenclatureInstanceRepository ?? throw new ArgumentNullException(nameof(nomenclatureInstanceRepository));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_extendedPermissionValidator =
				extendedPermissionValidator ?? throw new ArgumentNullException(nameof(extendedPermissionValidator));

			Init(employeeService ?? throw new ArgumentNullException(nameof(employeeService)));
		}

		public bool IsBulkAccountingActive
		{
			get => _isBulkAccountingActive;
			set
			{
				if(SetField(ref _isBulkAccountingActive, value))
				{
					if(value)
					{
						ActiveAccounting = 0;
					}
				}
			}
		}

		public bool IsInstanceAccountingActive
		{
			get => _isInstanceAccountingActive;
			set
			{
				if(SetField(ref _isInstanceAccountingActive, value))
				{
					if(value)
					{
						ActiveAccounting = 1;
					}
				}
			}
		}

		public int ActiveAccounting
		{
			get => _activeAccounting;
			set => SetField(ref _activeAccounting, value);
		}

		public string InstancesDiscrepanciesString
		{
			get => _instancesDiscrepanciesString;
			set => SetField(ref _instancesDiscrepanciesString, value);
		}

		public bool CanCreate => Entity.Id == 0 && CheckCanCreateDocument();
		public bool CanEdit => Entity.Id > 0 && CheckCanEditDocument();
		public bool HasAccessToCarStorages { get; private set; }
		public bool CanSave => CanCreate || CanEdit;
		public bool CanChangeShiftChangeResidueDocumentType => !Entity.StorageIsNotEmpty();
		public bool CanHandleDocumentItems => Entity.Warehouse != null || Entity.Car != null;
		public bool CanShowWarehouseStorage => Entity.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Warehouse;
		public bool CanShowCarStorage => Entity.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car;
		public SelectableParameterReportFilterViewModel SelectableFilterViewModel { get; private set; }
		public EntityEntryViewModel<Warehouse> WarehouseStorageEntryViewModel { get; private set; }
		public EntityEntryViewModel<Car> CarStorageEntryViewModel { get; private set; }
		public EntityEntryViewModel<Employee> EmployeeSenderEntryViewModel { get; private set; }
		public EntityEntryViewModel<Employee> EmployeeReceiverEntryViewModel { get; private set; }

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(ShiftChangeWarehouseDocument), "акта передачи остатков"))
				{
					if(!Save())
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не удалось сохранить документ, попробуйте еще раз");
						return;
					}
				}

				var reportInfo = _reportInfoFactory.Create();

				var carTypeOfUsesForDefectionAct = new List<CarTypeOfUse> { CarTypeOfUse.Largus, CarTypeOfUse.Minivan, CarTypeOfUse.GAZelle };

				if(Entity.Car != null
					&& Entity.Car.CarModel != null
					&& carTypeOfUsesForDefectionAct.Contains(Entity.Car.CarModel.CarTypeOfUse))
				{
					reportInfo.Title = $"Акт передачи остатков №{Entity.Id} от {Entity.TimeStamp:d}";
					reportInfo.Identifier = "Store.ShiftChangeWarehouseWithCarDefectionAct";
					reportInfo.Parameters = new Dictionary<string, object>
					{
						{ "document_id", Entity.Id },
						{ "car_id", Entity.Car?.Id },
						{ "include_largus_defects_act", Entity.Car.CarModel?.CarTypeOfUse == CarTypeOfUse.Largus || Entity.Car.CarModel?.CarTypeOfUse == CarTypeOfUse.Minivan },
						{ "include_GAZelle_defects_act", Entity.Car.CarModel?.CarTypeOfUse == CarTypeOfUse.GAZelle },
						{ "order_by_nomenclature_name", Entity.SortedByNomenclatureName },
						{ "sender_fio", Entity.Sender.FullName },
						{ "receiver_fio", Entity.Receiver.FullName },
					};
				}
				else
				{
					reportInfo.Title = $"Акт передачи остатков №{Entity.Id} от {Entity.TimeStamp:d}";
					reportInfo.Identifier = "Store.ShiftChangeWarehouse";
					reportInfo.Parameters = new Dictionary<string, object>
					{
						{ "document_id", Entity.Id },
						{ "order_by_nomenclature_name", Entity.SortedByNomenclatureName}
					};
				}

				_reportViewOpener.OpenReport(this, reportInfo);
			}
		));

		#region Объемный учет

		public string FillNomenclaturesByStorageTitle
		{
			get
			{
				switch(Entity.ShiftChangeResidueDocumentType)
				{
					case ShiftChangeResidueDocumentType.Warehouse:
						return Entity.ObservableNomenclatureItems.Any() ? _updateResidues : _fillByWarehouse;
					case ShiftChangeResidueDocumentType.Car:
						return Entity.ObservableNomenclatureItems.Any() ? _updateResidues : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}

		public DelegateCommand FillNomenclatureItemsByStorageCommand => _fillNomenclatureItemsByStorageCommand ?? (
			_fillNomenclatureItemsByStorageCommand = new DelegateCommand(
				() =>
				{
					// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
					var nomenclaturesToInclude = new List<int>();
					var nomenclaturesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					var productGroupToInclude = new List<int>();
					var productGroupToExclude = new List<int>();

					foreach(var parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case nameof(Nomenclature):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										nomenclaturesToExclude.Add(value.EntityId);
									}
								}
								break;
							case nameof(NomenclatureCategory):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								}
								else
								{
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
									}
								}
								break;
							case nameof(ProductGroup):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										productGroupToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										productGroupToExclude.Add(value.EntityId);
									}
								}
								break;
						}
					}

					if(Entity.NomenclatureItems.Count == 0)
					{
						Entity.FillNomenclatureItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude,
							nomenclaturesToExclude: nomenclaturesToExclude,
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude,
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude,
							productGroupToInclude: productGroupToInclude,
							productGroupToExclude: productGroupToExclude);
					}
					else
					{
						Entity.UpdateItemsFromStock(
							UoW,
							_stockRepository,
							nomenclaturesToInclude: nomenclaturesToInclude,
							nomenclaturesToExclude: nomenclaturesToExclude,
							nomenclatureTypeToInclude: nomenclatureCategoryToInclude,
							nomenclatureTypeToExclude: nomenclatureCategoryToExclude,
							productGroupToInclude: productGroupToInclude,
							productGroupToExclude: productGroupToExclude);
					}

					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));

		public DelegateCommand AddMissingNomenclatureCommand => _addMissingNomenclatureCommand ?? (
			_addMissingNomenclatureCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
						this,
						filter => filter.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
						OpenPageOptions.AsSlave);

					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnMissingNomenclatureSelectedResult;
				}
			));

		#endregion

		#region Экземплярный учет

		public string FillNomenclatureInstancesByStorageTitle
		{
			get
			{
				switch(Entity.ShiftChangeResidueDocumentType)
				{
					case ShiftChangeResidueDocumentType.Warehouse:
						return Entity.ObservableInstanceItems.Any() ? _updateResidues : _fillByWarehouse;
					case ShiftChangeResidueDocumentType.Car:
						return Entity.ObservableInstanceItems.Any() ? _updateResidues : _fillByCar;
					default:
						throw new InvalidOperationException("Выбран неверный тип документа");
				}
			}
		}

		public DelegateCommand FillNomenclatureInstanceItemsCommand => _fillNomenclatureInstanceItemsCommand ?? (
			_fillNomenclatureInstanceItemsCommand = new DelegateCommand(
				() =>
				{
					var instancesToInclude = new List<int>();
					var instancesToExclude = new List<int>();
					var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
					var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
					var productGroupToInclude = new List<int>();
					var productGroupToExclude = new List<int>();

					foreach(var parameterSet in _selectableFilter.ParameterSets)
					{
						switch(parameterSet.ParameterName)
						{
							case nameof(Nomenclature):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<InventoryNomenclatureInstance> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										instancesToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<InventoryNomenclatureInstance> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										instancesToExclude.Add(value.EntityId);
									}
								}
								break;
							case nameof(NomenclatureCategory):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
									}
								}
								else
								{
									foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
										nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
									}
								}
								break;
							case nameof(ProductGroup):
								if(parameterSet.FilterType == SelectableFilterType.Include)
								{
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										productGroupToInclude.Add(value.EntityId);
									}
								}
								else
								{
									foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
									{
										productGroupToExclude.Add(value.EntityId);
									}
								}
								break;
						}
					}

					Entity.FillInstanceItemsFromStock(
						UoW,
						_nomenclatureInstanceRepository,
						instancesToInclude,
						instancesToExclude,
						nomenclatureCategoryToInclude,
						nomenclatureCategoryToExclude,
						productGroupToInclude,
						productGroupToExclude);

					OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
				}
			));

		public DelegateCommand AddMissingNomenclatureInstanceCommand => _addMissingNomenclatureInstanceCommand ?? (
			_addMissingNomenclatureInstanceCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(this, OpenPageOptions.AsSlave);
					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += OnMissingInstanceSelectResult;
				}
			));

		#endregion

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.SortedByNomenclatureName))
			{
				SortDocumentItems();
			}
		}

		private void SortDocumentItems()
		{
			Entity.SortItems(Entity.SortedByNomenclatureName);
		}

		protected override bool BeforeValidation() => CanSave;

		protected override bool BeforeSave()
		{
			Entity.LastEditorId = _currentEmployee?.Id;
			Entity.LastEditedTime = DateTime.Now;
			UpdateInstanceDiscrepancies();

			return true;
		}

		private void Init(IEmployeeService employeeService)
		{
			_currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			SetPermissions();

			if(_currentEmployee is null)
			{
				ShowErrorMessage(
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете работать с данным диалогом," +
					" так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			if(!CanCreate && UoW.IsNew)
			{
				FailInitialize = true;
				return;
			}

			if(!CanEdit && !UoW.IsNew)
			{
				ShowWarningMessage("У вас нет прав на изменение этого документа.");
			}

			if(Entity.Id == 0)
			{
				Entity.AuthorId = _currentEmployee?.Id;
				Entity.Warehouse = _documentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.ShiftChangeCreate);
			}
			else
			{
				if(CheckNomenclaturesWithoutUnits(out var errorMessage))
				{
					ShowErrorMessage(errorMessage);
					FailInitialize = true;
					return;
				}
			}

			ConfigureSelectableFilter();
			SetEntriesViewModels();
			SetPropertyChangeRelations();

			if(Entity.ObservableInstanceItems.Count > 0)
			{
				UpdateInstanceDiscrepancies();
			}

			if(Entity.SortedByNomenclatureName)
			{
				SortDocumentItems();
			}
		}

		private bool CheckNomenclaturesWithoutUnits(out string errorMessage)
		{
			errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			var wrongNomenclatures = 0;
			foreach(var item in Entity.NomenclatureItems)
			{
				if(item.Nomenclature.Unit == null)
				{
					errorMessage += $"Номер: {item.Nomenclature.Id}. Название: {item.Nomenclature.Name}{Environment.NewLine}";
					wrongNomenclatures++;
				}
			}

			return wrongNomenclatures > 0;
		}

		private void ConfigureSelectableFilter()
		{
			_selectableFilter = _scope.Resolve<SelectableParametersReportFilter>(new TypedParameter(typeof(IUnitOfWork), UoW));

			var nomenclatureParam = _selectableFilter.CreateParameterSet(
				"Номенклатуры",
				nameof(Nomenclature),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;

					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			var nomenclatureTypeParam = _selectableFilter.CreateParameterSet(
				"Типы номенклатур",
				nameof(NomenclatureCategory),
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any())
					{
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			_selectableFilter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(UoW,
					(filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null)
							.And(p => !p.IsArchive);

						if(filters != null && filters.Any())
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}
						return query.List();
					},
				x => x.Name,
				x => x.Childs)
			);

			Nomenclature nomenclatureAlias = null;
			var instancesParam = _selectableFilter.CreateParameterSet("Экземпляры",
				nameof(InventoryNomenclatureInstance),
				new ParametersFactory(UoW, (filters) =>
				{
					InventoryNomenclatureInstance instanceAlias = null;
					SelectableEntityParameter<InventoryNomenclatureInstance> resultAlias = null;

					var query = UoW.Session.QueryOver(() => instanceAlias)
						.JoinAlias(i => i.Nomenclature, () => nomenclatureAlias)
						.Where(i => !i.IsArchive);

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					var customName = CustomProjections.Concat(
						Projections.Property(() => nomenclatureAlias.OfficialName),
						Projections.Constant(" "),
						InventoryNomenclatureInstanceProjections.InventoryNumberProjection());

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(customName).WithAlias(() => resultAlias.EntityTitle)
					).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<InventoryNomenclatureInstance>>());
					return query.List<SelectableParameter>();
				}));

			instancesParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedTypes = nomenclatureTypeParam.GetSelectedValues().ToArray();
					if(!selectedTypes.Any())
					{
						return null;
					}
					return Restrictions.On(() => nomenclatureAlias.Category).IsIn(selectedTypes);
				});

			SelectableFilterViewModel = _scope.Resolve<SelectableParameterReportFilterViewModel>(
				new TypedParameter(typeof(SelectableParametersReportFilter), _selectableFilter));
		}

		private void SetPermissions()
		{
			HasAccessToCarStorages =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_car_storage_in_warehouse_documents");
		}

		private void SetEntriesViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<ShiftChangeWarehouseDocument>(this, Entity, UoW, NavigationManager, _scope);

			_availableWarehousesIdsForCreate = _documentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.ShiftChangeCreate);
			_availableWarehousesIdsForEdit = _documentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.ShiftChangeEdit);

			EmployeeSenderEntryViewModel = builder.ForProperty(x => x.Sender)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			EmployeeSenderEntryViewModel.CanViewEntity = false;

			EmployeeReceiverEntryViewModel = builder.ForProperty(x => x.Receiver)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			EmployeeReceiverEntryViewModel.CanViewEntity = false;

			WarehouseStorageEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(GetWarehouseFilterParams)
				.Finish();
			WarehouseStorageEntryViewModel.CanViewEntity = false;
			WarehouseStorageEntryViewModel.BeforeChangeByUser += OnWarehouseBeforeChangeByUser;
			WarehouseStorageEntryViewModel.ChangedByUser += OnWarehouseChangedByUser;

			CarStorageEntryViewModel = builder.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
			CarStorageEntryViewModel.CanViewEntity = false;
			CarStorageEntryViewModel.BeforeChangeByUser += OnCarBeforeChangeByUser;
			CarStorageEntryViewModel.ChangedByUser += OnCarChangedByUser;
		}

		private void OnWarehouseBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Entity.StorageIsNotEmpty() && Entity.ItemsNotEmpty())
			{
				if(AskQuestion("При изменении склада табличная часть документа будет очищена. Продолжить?"))
				{
					ClearItems();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}

		private void OnCarBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Entity.StorageIsNotEmpty() && Entity.ItemsNotEmpty())
			{
				if(AskQuestion("При изменении автомобиля табличная часть документа будет очищена. Продолжить?"))
				{
					ClearItems();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}

		private void OnWarehouseChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Warehouse is null)
			{
				ClearItems();
			}
		}

		private void OnCarChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Car is null)
			{
				ClearItems();
			}
		}

		private void ClearItems()
		{
			Entity.ObservableNomenclatureItems.Clear();
			Entity.ObservableInstanceItems.Clear();
			_instancesDiscrepancies.Clear();
			InstancesDiscrepanciesString = string.Empty;
			OnPropertyChanged(nameof(FillNomenclaturesByStorageTitle));
			OnPropertyChanged(nameof(FillNomenclatureInstancesByStorageTitle));
		}

		private void GetWarehouseFilterParams(WarehouseJournalFilterViewModel filter)
		{
			filter.IncludeWarehouseIds = UoW.IsNew ? _availableWarehousesIdsForCreate : _availableWarehousesIdsForEdit;
		}

		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.ShiftChangeResidueDocumentType,
				() => CanShowWarehouseStorage,
				() => CanShowCarStorage);

			SetPropertyChangeRelation(
				x => x.ShiftChangeResidueDocumentType,
				() => FillNomenclaturesByStorageTitle,
				() => FillNomenclatureInstancesByStorageTitle);

			SetStoragePropertiesChangeRelation();

			Entity.PropertyChanged += EntityPropertyChanged;
		}

		private void SetStoragePropertiesChangeRelation()
		{
			SetPropertyChangeRelation(x => x.Warehouse,
				() => CanHandleDocumentItems,
				() => CanChangeShiftChangeResidueDocumentType);
			SetPropertyChangeRelation(x => x.Car,
				() => CanHandleDocumentItems,
				() => CanChangeShiftChangeResidueDocumentType);
		}

		private void OnMissingNomenclatureSelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var node in selectedNodes)
			{
				if(Entity.NomenclatureItems.Any(x => x.Nomenclature.Id == node.Id))
				{
					continue;
				}

				var nomenclature = UoW.GetById<Nomenclature>(node.Id);
				Entity.AddItem(nomenclature, 0, 0);
			}
		}

		private void OnMissingInstanceSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.GetSelectedObjects<InventoryInstancesJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var node in selectedNodes)
			{
				if(Entity.InstanceItems.Any(x => x.InventoryNomenclatureInstance.Id == node.Id))
				{
					continue;
				}

				var instance = UoW.GetById<InventoryNomenclatureInstance>(node.Id);
				Entity.AddInstanceItem(instance, 0, true);
			}
		}

		private void UpdateInstanceDiscrepancies()
		{
			var storageId = Entity.GetStorageId();
			var storageType = Entity.GetStorageType();
			var currentInstancesIds =
				Entity.ObservableInstanceItems.Select(x => x.InventoryNomenclatureInstance.Id).ToArray();

			var instancesOnStorageBalance =
				_nomenclatureInstanceRepository.GetOtherInstancesOnStorageBalance(
					UoW, storageType, storageId ?? 0, currentInstancesIds, Entity.TimeStamp);

			foreach(var instanceData in instancesOnStorageBalance)
			{
				if(_instancesDiscrepancies.ContainsKey(instanceData.Id))
				{
					continue;
				}
				
				_instancesDiscrepancies.Add(
					instanceData.Id,
					$"{instanceData.Name} {instanceData.GetInventoryNumber} числится на этом складе");
			}

			var currentInstancesOnOtherStorages =
				_nomenclatureInstanceRepository.GetCurrentInstancesOnOtherStorages(
					UoW, storageType, storageId ?? 0, currentInstancesIds, Entity.TimeStamp);

			if(currentInstancesOnOtherStorages.Any())
			{
				foreach(var groupInstanceData in currentInstancesOnOtherStorages)
				{
					var key = groupInstanceData.Key;
					var instanceData = groupInstanceData.First();
					var storages = string.Join(",", groupInstanceData.Select(x => x.StorageName));

					if(!_instancesDiscrepancies.ContainsKey(key))
					{
						_instancesDiscrepancies.Add(key, $"{instanceData.Name} {instanceData.GetInventoryNumber} числится на: {storages}");
					}
					else
					{
						_instancesDiscrepancies[key] =
							$"{instanceData.Name} {instanceData.GetInventoryNumber} числится на: {storages} помимо выбранного склада";
					}
				}
			}

			InstancesDiscrepanciesString = _instancesDiscrepancies.Any() ? string.Join("\n", _instancesDiscrepancies.Values) : string.Empty;
		}

		private bool CheckCanCreateDocument()
		{
			switch(Entity.ShiftChangeResidueDocumentType)
			{
				case ShiftChangeResidueDocumentType.Warehouse:
					return !_documentHelper.CheckCreateDocument(WarehousePermissionsType.ShiftChangeCreate, Entity.Warehouse);
				case ShiftChangeResidueDocumentType.Car:
					return HasAccessToCarStorages;
			}

			return false;
		}

		private bool CheckCanEditDocument()
		{
			var canEdit = false;

			switch(Entity.ShiftChangeResidueDocumentType)
			{
				case ShiftChangeResidueDocumentType.Warehouse:
					canEdit = _documentHelper.CanEditDocument(WarehousePermissionsType.ShiftChangeEdit, Entity.Warehouse);
					break;
				case ShiftChangeResidueDocumentType.Car:
					canEdit = HasAccessToCarStorages;
					break;
			}

			return Entity.TimeStamp < DateTime.Today
				? canEdit && _extendedPermissionValidator.Validate(
					typeof(ShiftChangeWarehouseDocument),
					UserService.CurrentUserId,
					nameof(RetroactivelyClosePermission))
				: canEdit;
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= EntityPropertyChanged;
			WarehouseStorageEntryViewModel.BeforeChangeByUser -= OnWarehouseBeforeChangeByUser;
			WarehouseStorageEntryViewModel.ChangedByUser -= OnWarehouseChangedByUser;
			CarStorageEntryViewModel.BeforeChangeByUser -= OnCarBeforeChangeByUser;
			CarStorageEntryViewModel.ChangedByUser -= OnCarChangedByUser;
			base.Dispose();
		}
	}
}
