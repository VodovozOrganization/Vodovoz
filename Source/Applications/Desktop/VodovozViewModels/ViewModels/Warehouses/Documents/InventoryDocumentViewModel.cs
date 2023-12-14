using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Autofac;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Warehouses.Documents
{
	[Obsolete("Снести после обновления 29.05.23")]
	public class InventoryDocumentViewModel : EntityTabViewModelBase<InventoryDocument>
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private IEnumerable<Nomenclature> _nomenclaturesWithDiscrepancies = new List<Nomenclature>();
		private SelectableParametersReportFilter _filter;
		private InventoryDocumentItem _selectedInventoryDocumentItem;
		private ILifetimeScope _lifetimeScope;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly StoreDocumentHelper _storeDocumentHelper;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
		private readonly IReportViewOpener _reportViewOpener;

		public InventoryDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			IWarehouseRepository warehouseRepository,
			StoreDocumentHelper storeDocumentHelper,
			IStockRepository stockRepository,
			INomenclatureJournalFactory nomenclatureJournalFactory,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IReportViewOpener reportViewOpener,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_warehouseRepository = warehouseRepository
				?? throw new ArgumentNullException(nameof(warehouseRepository));
			_storeDocumentHelper = storeDocumentHelper
				?? throw new ArgumentNullException(nameof(storeDocumentHelper));
			_stockRepository = stockRepository
				?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureJournalFactory = nomenclatureJournalFactory
				?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			_entityExtendedPermissionValidator = entityExtendedPermissionValidator;
			_reportViewOpener = reportViewOpener
				?? throw new ArgumentNullException(nameof(reportViewOpener));
			NomenclaturesWithDiscrepancies = new List<Nomenclature>();

			ConfigureEntity();

			FilterViewModel = ConfigureFilter();

			PrintCommand = new DelegateCommand(Print);

			FillDiscrepanciesCommand = new DelegateCommand(
				FillDiscrepancies,
				() => CanFillDiscrepancies);

			FillItemsCommand = new DelegateCommand(
				FillItems,
				() => CanFillItems);

			AddItemCommand = new DelegateCommand(
				AddItem,
				() => CanAddItem);

			AddOrEditFineCommand = new DelegateCommand(
				AddOrEditFine,
				() => CanAddFine);

			DeleteFineCommand = new DelegateCommand(
				DeleteFine,
				() => CanDeleteFine);

			ClearItemsCommand = new DelegateCommand(ClearItems);

			FillFactByAccountingCommand = new DelegateCommand(FillFactByAccounting);

			SubscribeOnEntityChanges();
			SortDocumentItems();
			Entity.ObservableNomenclatureItems.Reconnect();
		}

		private void ClearItems()
		{
			Entity.ObservableNomenclatureItems.Clear();
		}

		private void ValidateNomenclatures()
		{
			int wrongNomenclatures = 0;

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(
				  "Не установлены единицы измерения у следующих номенклатур:");

			foreach(var item in Entity.ObservableNomenclatureItems)
			{
				if(item.Nomenclature.Unit == null)
				{
					stringBuilder.AppendLine(
						$"Номер: {item.Nomenclature.Id}." +
						$" Название: {item.Nomenclature.Name}");
					wrongNomenclatures++;
				}
			}

			if(wrongNomenclatures > 0)
			{
				CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						stringBuilder.ToString());

				Close(false, CloseSource.Self);
			}
		}

		private void ConfigureEntity()
		{
			if(Entity.Id == 0)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(currentEmployee is null)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"Ваш пользователь не привязан к действующему сотруднику," +
						" вы не можете создавать складские документы," +
						" так как некого указывать в качестве кладовщика.");

					Close(false, CloseSource.Self);
				}

				Entity.Author = currentEmployee;

				Entity.Warehouse = _storeDocumentHelper
					.GetDefaultWarehouse(UoW, WarehousePermissionsType.InventoryEdit);
			}

			Entity.CanEdit = _storeDocumentHelper
				.CanEditDocument(WarehousePermissionsType.InventoryEdit, Entity.Warehouse);

			ValidateNomenclatures();

			Entity.CanEdit =
				_entityExtendedPermissionValidator.Validate(
					typeof(InventoryDocument),
					ServicesConfig.UserService.CurrentUserId,
					nameof(RetroactivelyClosePermission));

			if(Entity.CanEdit || Entity.TimeStamp.Date == DateTime.Now.Date)
			{
				Entity.CanEdit = true;
			}
		}

		public InventoryDocumentItem SelectedInventoryDocumentItem
		{
			get => _selectedInventoryDocumentItem;
			set
			{
				if(SetField(ref _selectedInventoryDocumentItem, value))
				{
					OnPropertyChanged(nameof(CanAddFine));
					OnPropertyChanged(nameof(CanDeleteFine));
				}
			}
		}

		public override string Title => Entity.Id == 0 ? "Новая инвентаризация" : Entity.Title;

		public bool CanAddFine => Entity.CanEdit && SelectedInventoryDocumentItem != null;

		public string AddFineButtonTitle =>
			SelectedInventoryDocumentItem?.Fine != null
			? "Изменить штраф"
			: "Добавить штраф";

		public bool CanDeleteFine => Entity.CanEdit
			&& SelectedInventoryDocumentItem != null
			&& SelectedInventoryDocumentItem.Fine != null;

		public bool CanSave => Entity.CanEdit;

		public bool CanAddItem => Entity.CanEdit;

		public bool CanFillItems => Entity.CanEdit && Entity.Warehouse != null;

		public string FillItemsButtonTitle => Entity.ObservableNomenclatureItems.Count > 0
			? "Обновить остатки"
			: "Заполнить по складу";

		public SelectableParametersReportFilter Filter { get => _filter; set => _filter = value; }

		public IEnumerable<Nomenclature> NomenclaturesWithDiscrepancies
		{
			get => _nomenclaturesWithDiscrepancies;
			set => _nomenclaturesWithDiscrepancies = value;
		}

		public INomenclatureJournalFactory NomenclatureJournalFactory => _nomenclatureJournalFactory;

		#region Commands

		public DelegateCommand PrintCommand { get; }
		
		public DelegateCommand FillDiscrepanciesCommand { get; }

		public DelegateCommand FillItemsCommand { get; }

		public DelegateCommand FillFactByAccountingCommand { get; }

		public DelegateCommand AddItemCommand { get; }

		public DelegateCommand AddOrEditFineCommand { get; }

		public DelegateCommand DeleteFineCommand { get; }

		public DelegateCommand ClearItemsCommand { get; }

		public SelectableParameterReportFilterViewModel FilterViewModel { get; }

		#endregion

		public QueryOver<Warehouse> GetRestrictedWarehouseQuery()
		{
			return _storeDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissionsType.InventoryEdit);
		}

		private void SubscribeOnEntityChanges()
		{
			SetPropertyChangeRelation(
				iDoc => iDoc.Id,
				() => Title);

			SetPropertyChangeRelation(
				iDoc => iDoc.Warehouse,
				() => CanFillItems);

			SetPropertyChangeRelation(
				iDoc => iDoc.ObservableNomenclatureItems,
				() => FillItemsButtonTitle);

			Entity.ObservableNomenclatureItems.ListChanged += (list) => OnPropertyChanged(nameof(FillItemsButtonTitle));

			SetPropertyChangeRelation(
				iDoc => iDoc.CanEdit,
				() => CanFillItems,
				() => CanAddFine,
				() => CanAddItem,
				() => CanDeleteFine,
				() => CanSave);

			Entity.PropertyChanged += EntityPropertyChanged;
		}

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.SortedByNomenclatureName))
			{
				SortDocumentItems();
			}
		}

		private SelectableParameterReportFilterViewModel ConfigureFilter()
		{
			var filter = new SelectableParametersReportFilter(UoW);

			var nomenclatureParam = filter.CreateParameterSet(
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
						.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle));

					query.TransformUsing(Transformers
						.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				}));

			var nomenclatureTypeParam = filter.CreateParameterSet(
				"Типы номенклатур",
				nameof(NomenclatureCategory),
				new ParametersEnumFactory<NomenclatureCategory>());

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any())
					{
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category)
						.IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				});

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			filter.CreateParameterSet(
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
				x => x.Childs));

			Filter = filter;

			return new SelectableParameterReportFilterViewModel(Filter);
		}

		private void FillDiscrepancies()
		{
			_nomenclaturesWithDiscrepancies = _warehouseRepository
				.GetDiscrepancyNomenclatures(UoW, Entity.Warehouse.Id);
		}

		public bool CanFillDiscrepancies => Entity.Warehouse != null && Entity.Warehouse.Id > 0;

		private void AddItem()
		{
			var nomenclatureSelector = NomenclatureJournalFactory.CreateNomenclatureSelector(_lifetimeScope);
			var nomenclatureJournalViewModel = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, IEntitySelector>(this, nomenclatureSelector).ViewModel;
			nomenclatureJournalViewModel.SelectionMode = JournalSelectionMode.Single;
			nomenclatureJournalViewModel.OnEntitySelectedResult += NomenclatureSelectorOnEntitySelectedResult;
		}

		private void NomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			if(e.SelectedNodes.Any())
			{
				foreach(var node in e.SelectedNodes)
				{
					if(Entity.ObservableNomenclatureItems.Any(x => x.Nomenclature.Id == node.Id))
					{
						continue;
					}

					var nomenclature = UoW.GetById<Nomenclature>(node.Id);
					Entity.AddNomenclatureItem(nomenclature, 0, 0);
				}

				SortDocumentItems();
			}
		}

		private void SortDocumentItems()
		{
			if(Entity.SortedByNomenclatureName)
			{
				Entity.SortItems(true);
			}
			else
			{
				Entity.SortItems();
			}
		}

		public bool AskQuestion(string question)
		{
			return CommonServices.InteractiveService.Question(question);
		}

		public override bool Save(bool close)
		{
			if(!Entity.CanEdit)
			{
				return false;
			}

			var errors = Entity.Validate(new ValidationContext(Entity));

			if(errors.Any())
			{
				string errorsString;

				StringBuilder stringBuilder = new StringBuilder();

				foreach(var error in errors)
				{
					stringBuilder.AppendLine(error.ErrorMessage);
				}

				errorsString = stringBuilder.ToString();

				CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						errorsString,
						"Ошибка валидации");

				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Ваш пользователь не привязан к действующему сотруднику," +
					" вы не можете изменять складские документы," +
					" так как некого указывать в качестве кладовщика."
					);

				return false;
			}

			Entity.UpdateOperations(UoW);

			_logger.Info("Сохраняем акт списания...");
			UoWGeneric.Save();
			_logger.Info("Ok.");

			if(close)
			{
				Close(false, CloseSource.Save);
			}
			return true;
		}

		private void Print()
		{
			if(UoWGeneric.HasChanges && CommonServices.InteractiveService.Question(
					"Для печати необходимо сохранить документ." +
					" Сохранить акт инвентаризации?"))
			{
				Save();
			}

			var reportInfo = new QS.Report.ReportInfo
			{
				Title = $"Акт инвентаризации №{Entity.Id} от {Entity.TimeStamp:d}",
				Identifier = "Store.InventoryDoc",
				Parameters = new Dictionary<string, object>
				{
					{ "inventory_id",  Entity.Id },
					{ "sorted_by_nomenclature_name", Entity.SortedByNomenclatureName }
				}
			};

			_reportViewOpener.OpenReport(TabParent, reportInfo);
		}

		private void FillItems()
		{
			// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
			var nomenclaturesToInclude = new List<int>();
			var nomenclaturesToExclude = new List<int>();
			var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
			var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
			var productGroupToInclude = new List<int>();
			var productGroupToExclude = new List<int>();

			foreach(SelectableParameterSet parameterSet in Filter.ParameterSets)
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
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
							}
						}
						else
						{
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
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

			FillDiscrepancies();

			if(Entity.ObservableNomenclatureItems.Count == 0)
			{
				Entity.FillNomenclatureItemsFromStock(
					UoW,
					_stockRepository,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			}
			else
			{
				Entity.UpdateNomenclatureItemsFromStock(
					UoW,
					_stockRepository,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			}

			SortDocumentItems();
		}

		private void FillFactByAccounting()
		{
			for(int i = 0; i < Entity.ObservableNomenclatureItems.Count; i++)
			{
				if(Entity.ObservableNomenclatureItems[i].AmountInFact != Entity.ObservableNomenclatureItems[i].AmountInDB)
				{
					Entity.ObservableNomenclatureItems[i].AmountInFact = Entity.ObservableNomenclatureItems[i].AmountInDB;
					Entity.ObservableNomenclatureItems.OnPropertyChanged(nameof(Entity.ObservableNomenclatureItems));
				}
			}
		}

		private void AddOrEditFine()
		{
			EntityUoWBuilder entityUoWBuilder;
			FineViewModel fineViewModel;

			if(SelectedInventoryDocumentItem.Fine != null)
			{
				entityUoWBuilder = EntityUoWBuilder.ForOpen(SelectedInventoryDocumentItem.Fine.Id);
				fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, entityUoWBuilder).ViewModel;

				fineViewModel.EntitySaved += ExistingFineEntitySaved;
			}
			else
			{
				entityUoWBuilder = EntityUoWBuilder.ForCreate();
				fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, entityUoWBuilder).ViewModel;

				fineViewModel.Entity.FineReasonString = "Недостача";

				fineViewModel.EntitySaved += NewFineSaved;
			}

			fineViewModel.Entity.TotalMoney = SelectedInventoryDocumentItem.SumOfDamage;
		}

		private void DeleteFine()
		{
			UoW.Delete(SelectedInventoryDocumentItem.Fine);
			SelectedInventoryDocumentItem.Fine = null;
			OnPropertyChanged(nameof(CanAddFine));
			OnPropertyChanged(nameof(CanDeleteFine));
		}

		private void NewFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedInventoryDocumentItem.Fine = e.Entity as Fine;
		}

		private void ExistingFineEntitySaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedInventoryDocumentItem.Fine.Id;
			UoW.Session.Evict(SelectedInventoryDocumentItem.Fine);
			SelectedInventoryDocumentItem.Fine = UoW.GetById<Fine>(id);
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= EntityPropertyChanged;

			base.Dispose();
		}
	}
}
