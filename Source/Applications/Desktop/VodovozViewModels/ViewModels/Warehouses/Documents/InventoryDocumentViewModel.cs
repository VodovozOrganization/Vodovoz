using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Warehouses.Documents
{
	public class InventoryDocumentViewModel : EntityTabViewModelBase<InventoryDocument>
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private IEnumerable<Nomenclature> _nomenclaturesWithDiscrepancies = new List<Nomenclature>();
		private bool _sortByNomenclatureTitle;
		private SelectableParametersReportFilter _filter;
		private InventoryDocumentItem _selectedInventoryDocumentItem;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IStoreDocumentHelper _storeDocumentHelper;
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
			IStoreDocumentHelper storeDocumentHelper,
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
			_sortByNomenclatureTitle = false;

			SubscribeOnEntityChanges();
		}

		public override string Title => Entity.Id == 0 ? "Новая инвентаризация" : Entity.Title;

		private void ValidateNomenclatures()
		{
			int wrongNomenclatures = 0;

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(
				  "Не установлены единицы измерения у следующих номенклатур:");

			foreach(var item in UoWGeneric.Root.Items)
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

		private void SubscribeOnEntityChanges()
		{
			SetPropertyChangeRelation(
				iDoc => iDoc.Id,
				() => Title);

			SetPropertyChangeRelation(
				iDoc => iDoc.Warehouse,
				() => CanFillItems);

			SetPropertyChangeRelation(
				iDoc => iDoc.Items,
				() => FillItemsButtonTitle);

			Entity.Items.ListChanged += (list) => OnPropertyChanged(nameof(FillItemsButtonTitle));

			SetPropertyChangeRelation(
				iDoc => iDoc.CanEdit,
				() => CanFillItems,
				() => CanAddFine,
				() => CanAddNomenclature,
				() => CanDeleteFine,
				() => CanSave);
		}

		public bool SortByNomenclatureTitle
		{
			get => _sortByNomenclatureTitle;
			set
			{
				if(SetField(ref _sortByNomenclatureTitle, value))
				{
					SortDocumentItems();
				}
			}
		}

		public bool CanSave => Entity.CanEdit;

		public bool CanAddNomenclature => Entity.CanEdit;

		public bool CanFillItems => Entity.CanEdit && Entity.Warehouse != null;

		public string FillItemsButtonTitle => Entity.Items.Count > 0
			? "Обновить остатки"
			: "Заполнить по складу";

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

		public bool CanAddFine => Entity.CanEdit && SelectedInventoryDocumentItem != null;

		public string AddFineButtonTitle =>
			SelectedInventoryDocumentItem?.Fine != null
			? "Изменить штраф"
			: "Добавить штраф";

		public bool CanDeleteFine => Entity.CanEdit
			&& SelectedInventoryDocumentItem != null
			&& SelectedInventoryDocumentItem.Fine != null;

		public void DeleteFine()
		{
			UoW.Delete(SelectedInventoryDocumentItem.Fine);
			SelectedInventoryDocumentItem.Fine = null;
			OnPropertyChanged(nameof(CanAddFine));
			OnPropertyChanged(nameof(CanDeleteFine));
		}

		public SelectableParameterReportFilterViewModel FilterViewModel { get; }

		public IEnumerable<Nomenclature> NomenclaturesWithDiscrepancies
		{
			get => _nomenclaturesWithDiscrepancies;
			set => _nomenclaturesWithDiscrepancies = value;
		}

		public SelectableParametersReportFilter Filter { get => _filter; set => _filter = value; }

		public INomenclatureJournalFactory NomenclatureJournalFactory => _nomenclatureJournalFactory;

		public QueryOver<Domain.Store.Warehouse> GetRestrictedWarehouseQuery()
		{
			return _storeDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissionsType.InventoryEdit);
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

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			filter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) =>
				{
					var query = UoW.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null);

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

		public void FillDiscrepancies()
		{
			if(Entity.Warehouse != null && Entity.Warehouse.Id > 0)
			{
				_nomenclaturesWithDiscrepancies = _warehouseRepository
					.GetDiscrepancyNomenclatures(UoW, Entity.Warehouse.Id);
			}
		}

		public void SortDocumentItems()
		{
			if(SortByNomenclatureTitle)
			{
				Entity.SortItems(x => x.Nomenclature.OfficialName);
			}
			else
			{
				Entity.SortItems(x => x.Nomenclature.Id);
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
			return true;
		}

		public void Print()
		{
			if(UoWGeneric.HasChanges
				&& CommonServices.InteractiveService.Question(
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
					{ "inventory_id",  Entity.Id }
				}
			};

			_reportViewOpener.OpenReport(TabParent, reportInfo);
		}

		public void FillItems(
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			string[] nomenclatureCategoryToInclude,
			string[] nomenclatureCategoryToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			if(Entity.Items.Count == 0)
			{
				Entity.FillItemsFromStock(
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

			SortDocumentItems();
		}

		public void FillByAccounting()
		{
			foreach(var inventoryDocumentItem in Entity.Items)
			{
				inventoryDocumentItem.AmountInFact = inventoryDocumentItem.AmountInDB;
			}
		}

		public void AddOrEditFine()
		{
			EntityUoWBuilder entityUoWBuilder;
			IPage fineView;

			if(SelectedInventoryDocumentItem.Fine != null)
			{
				entityUoWBuilder = EntityUoWBuilder.ForOpen(SelectedInventoryDocumentItem.Fine.Id);
				fineView = NavigationManager.OpenViewModel<FineViewModel>(this, addingRegistrations: (cb) =>
					cb.RegisterInstance(entityUoWBuilder).As<IEntityUoWBuilder>() );

				(fineView.ViewModel as FineViewModel).EntitySaved += ExistingFineEntitySaved;
			}
			else
			{
				entityUoWBuilder = EntityUoWBuilder.ForCreate();
				fineView = NavigationManager.OpenViewModel<FineViewModel>(this, addingRegistrations: (cb) =>
					cb.RegisterInstance(entityUoWBuilder).As<IEntityUoWBuilder>());

				(fineView.ViewModel as FineViewModel).Entity.FineReasonString = "Недостача";
				(fineView.ViewModel as FineViewModel).EntitySaved += NewFineSaved;
			}

			(fineView.ViewModel as FineViewModel).Entity.TotalMoney = SelectedInventoryDocumentItem.SumOfDamage;
		}

		private void NewFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedInventoryDocumentItem.Fine = e.Entity as Fine;
			SelectedInventoryDocumentItem = null;
		}

		private void ExistingFineEntitySaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = SelectedInventoryDocumentItem.Fine.Id;
			UoW.Session.Evict(SelectedInventoryDocumentItem.Fine);
			SelectedInventoryDocumentItem.Fine = UoW.GetById<Fine>(id);
		}
	}
}
