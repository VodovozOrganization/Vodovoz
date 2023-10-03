using ClosedXML.Report;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.SqlCommand;
using QS.Project.DB;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.NHibernateProjections.Documents;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Presentation.ViewModels.Common;
using QS.DomainModel.Entity;
using Vodovoz.Domain;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsItemsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsItemsJournalNode, WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private const string _journalReportTemplatePath = @".\Reports\Store\WarehouseDocumentsItemsJournalReport.xlsx";
		private const string _warehouseAccountingCardTemplatePath = @".\Reports\Store\WarehouseAccountingCard.xlsx";
		private const string _notSpecified = "Не указан";
		private readonly IFileDialogService _fileDialogService;

		private readonly Type[] _documentItemsTypes;
		private readonly Type[] _documentTypes;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private WarehouseDocumentsItemsJournalReport _warehouseDocumentsItemsJournalReport;
		private WarehouseAccountingCard _warehouseAccountingCard;
		private CancellationTokenSource _cancellationTokenSource;

		public WarehouseDocumentsItemsJournalViewModel(
			WarehouseDocumentsItemsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IGtkTabsOpener gtkTabsOpener,
			IFileDialogService fileDialogService)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			NavigationManager = navigationManager;
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			FilterViewModel.JournalViewModel = this;

			TabName = "Журнал строк складских документов";

			_documentItemsTypes = new[]
			{
				typeof(IncomingInvoiceItem),
				typeof(IncomingWaterMaterial),
				typeof(MovementDocumentItem),
				typeof(WriteOffDocumentItem),
				typeof(SelfDeliveryDocumentItem),
				typeof(CarLoadDocumentItem),
				typeof(CarUnloadDocumentItem),
				typeof(InventoryDocumentItem),
				typeof(ShiftChangeWarehouseDocumentItem),
				typeof(RegradingOfGoodsDocumentItem),
			};

			_documentTypes = new[]
			{
				typeof(IncomingInvoice),
				typeof(IncomingWater),
				typeof(MovementDocument),
				typeof(WriteOffDocument),
				typeof(SelfDeliveryDocument),
				typeof(CarLoadDocument),
				typeof(CarUnloadDocument),
				typeof(InventoryDocument),
				typeof(ShiftChangeWarehouseDocument),
				typeof(RegradingOfGoodsDocument),
				typeof(DriverAttachedTerminalDocumentBase),
				typeof(DriverAttachedTerminalGiveoutDocument),
				typeof(DriverAttachedTerminalReturnDocument),
			};

			SearchEnabled = false;

			UseSlider = false;

			RegisterDocumentItems(_documentItemsTypes);

			UpdateOnChanges(_documentItemsTypes.Concat(_documentTypes).ToArray());

			UpdateAllEntityPermissions();
			CreateNodeActions();
			CreatePopupActions();
			_cancellationTokenSource = new CancellationTokenSource();
			CreateJournalReportCommand = new DelegateCommand(async () => await CreateReport(_cancellationTokenSource.Token));
			ExportJournalReportCommand = new DelegateCommand(ExportJournalReport);
			CreateWarehouseAccountingCardCommand = new DelegateCommand(async () => await CreateWarehouseAccountingCard(_cancellationTokenSource.Token));
			ExportWarehouseAccountingCardCommand = new DelegateCommand(ExportWarehouseAccountingCard);

			FilterViewModel.PropertyChanged += OnFilterViewModelPropertyChanged;
		}

		private void OnFilterViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var canExportWarehouseAccountingCardPropertiesAffected = new string[]
			{
				nameof(FilterViewModel.Nomenclature),
				nameof(FilterViewModel.WarehouseIds),
				nameof(FilterViewModel.TargetSource)
			};

			if(canExportWarehouseAccountingCardPropertiesAffected.Contains(e.PropertyName))
			{
				OnPropertyChanged(nameof(CanExportWarehouseAccountingCard));
			}
		}

		public DelegateCommand CreateJournalReportCommand { get; }
		public DelegateCommand ExportJournalReportCommand { get; }
		public DelegateCommand CreateWarehouseAccountingCardCommand { get; }
		public DelegateCommand ExportWarehouseAccountingCardCommand { get; }

		public bool CanExportWarehouseAccountingCard =>
			FilterViewModel.Nomenclature != null
			&& FilterViewModel.WarehouseIds.Count == 1
			&& FilterViewModel.TargetSource == TargetSource.Both;

		protected override void CreatePopupActions()
		{
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateOpenDocumentAction();
			CreateExportJournalReportAction();
			CreateExportWarhouseAccountingCardAction();
		}

		protected void CreateOpenDocumentAction()
		{
			var editAction = new JournalAction("Открыть документ",
				(selected) => {
					var selectedNodes = selected.OfType<WarehouseDocumentsItemsJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					WarehouseDocumentsItemsJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<WarehouseDocumentsItemsJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					WarehouseDocumentsItemsJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void RegisterDocumentItems(Type[] types)
		{
			RegisterEntity(GetQueryIncomingInvoiceItem)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<IncomingInvoiceViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(IncomingInvoiceItem))
					.FinishConfiguration();

			RegisterEntity(new Func<IUnitOfWork, IQueryOver<IncomingWaterMaterial>>[]
				{
					GetQueryIncomingWaterFromMaterial,
					GetQueryIncomingWaterToMaterial
				}.AsEnumerable())
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenIncomingWaterDlg(node.DocumentId),
					(node) => node.EntityType == typeof(IncomingWaterMaterial))
					.FinishConfiguration();

			RegisterEntity(new Func<IUnitOfWork, IQueryOver<MovementDocumentItem>>[]
				{
					GetQueryMovementFromDocumentItem,
					GetQueryMovementToDocumentItem
				}.AsEnumerable())
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<MovementDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(MovementDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryWriteOffDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => NavigationManager.OpenViewModel<WriteOffDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(WriteOffDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQuerySelfDeliveryDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenSelfDeliveryDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(SelfDeliveryDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryCarLoadDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenCarLoadDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(CarLoadDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryCarUnloadDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenCarUnloadDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(CarUnloadDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryInventoryDocumentItem)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<InventoryDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(InventoryDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryShiftChangeWarehouseDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenShiftChangeWarehouseDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(ShiftChangeWarehouseDocumentItem))
					.FinishConfiguration();

			RegisterEntity<RegradingOfGoodsDocumentItem>(
				new Func<IUnitOfWork, IQueryOver<RegradingOfGoodsDocumentItem>>[]
				{
					GetQueryRegradingOfGoodsWriteoffDocumentItem,
					GetQueryRegradingOfGoodsIncomeDocumentItem
				}.AsEnumerable())
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenRegradingOfGoodsDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(RegradingOfGoodsDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryDriverAttachedTerminalGiveoutDocument)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<DriverAttachedTerminalViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(DriverAttachedTerminalGiveoutDocument))
					.FinishConfiguration();

			RegisterEntity(GetQueryDriverAttachedTerminalReturnDocument)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<DriverAttachedTerminalViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(DriverAttachedTerminalReturnDocument))
					.FinishConfiguration();

			var dataLoader = DataLoader as ThreadDataLoader<WarehouseDocumentsItemsJournalNode>;
			dataLoader.MergeInOrderBy(node => node.Date, true);
		}

		#region IQueryOver<Document> Functions

		private IQueryOver<IncomingInvoiceItem> GetQueryIncomingInvoiceItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;
			IncomingInvoice invoiceAlias = null;
			IncomingInvoiceItem incomingInvoiceItemAlias = null;
			Counterparty counterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var invoiceQuery = unitOfWork.Session.QueryOver(() => incomingInvoiceItemAlias)
				.Left.JoinQueryOver(() => incomingInvoiceItemAlias.Document, () => invoiceAlias);

			if((FilterViewModel.DocumentType is null
				|| FilterViewModel.DocumentType == DocumentType.IncomingInvoice)
				&& FilterViewModel.Driver is null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source))
			{
				if(FilterViewModel.DocumentId != null)
				{
					invoiceQuery.Where(() => invoiceAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					invoiceQuery.Where(() => invoiceAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					invoiceQuery.Where(() => invoiceAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.CounterpartyIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var countrpartyRestriction = Restrictions.In(Projections.Property(() => counterpartyAlias.Id), FilterViewModel.CounterpartyIds);
					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						invoiceQuery.Where(countrpartyRestriction);
					}
					else
					{
						invoiceQuery.Where(Restrictions.Not(countrpartyRestriction));
					}
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var warehouseRestriction = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);
					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						invoiceQuery.Where(warehouseRestriction);
					}
					else
					{
						invoiceQuery.Where(Restrictions.Not(warehouseRestriction));
					}
				}

				if(FilterViewModel.Author != null)
				{
					invoiceQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					invoiceQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				invoiceQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				invoiceQuery.Where(() => invoiceAlias.Id == -1);
			}

			return invoiceQuery
				.Left.JoinAlias(() => incomingInvoiceItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => invoiceAlias.Contractor, () => counterpartyAlias)
				.Left.JoinAlias(() => invoiceAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => invoiceAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => invoiceAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => incomingInvoiceItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => invoiceAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => invoiceAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => invoiceAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.IncomingInvoice).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(IncomingInvoiceItem)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias(() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => incomingInvoiceItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => invoiceAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<IncomingWaterMaterial> GetQueryIncomingWaterFromMaterial(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;
			IncomingWaterMaterial incomingWaterMaterialAlias = null;
			IncomingWater incomingWaterAlias = null;

			Warehouse fromWarehouseAlias = null;
			Warehouse toWarehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var waterQuery = unitOfWork.Session.QueryOver(() => incomingWaterMaterialAlias)
				.JoinQueryOver(() => incomingWaterMaterialAlias.Document, () => incomingWaterAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.IncomingWater)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Target)
			{
				if(FilterViewModel.DocumentId != null)
				{
					waterQuery.Where(() => incomingWaterAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					waterQuery.Where(() => incomingWaterAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					waterQuery.Where(() => incomingWaterAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Target)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => fromWarehouseAlias.Id), FilterViewModel.WarehouseIds);
					}

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						waterQuery.Where(warehouseCriterion);
					}
					else
					{
						waterQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					waterQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					waterQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				waterQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				waterQuery.Where(() => incomingWaterAlias.Id == -1);
			}

			return waterQuery
				.Left.JoinAlias(() => incomingWaterAlias.IncomingWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.WriteOffWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => incomingWaterAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => incomingWaterMaterialAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList(list => list
					.Select(() => incomingWaterMaterialAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => incomingWaterAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => incomingWaterAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => fromWarehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.IncomingWater).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(IncomingWaterMaterial)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => fromWarehouseAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => fromWarehouseAlias.Name)))
					.WithAlias(() => resultAlias.FromStorage)
					.Select(() => fromWarehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(Projections.Conditional(
						Restrictions.Where(() => toWarehouseAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => toWarehouseAlias.Name)))
					.WithAlias(() => resultAlias.ToStorage)
					.Select(() => toWarehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => incomingWaterMaterialAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => incomingWaterAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<IncomingWaterMaterial> GetQueryIncomingWaterToMaterial(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;
			IncomingWaterMaterial incomingWaterMaterialAlias = null;
			IncomingWater incomingWaterAlias = null;

			Warehouse fromWarehouseAlias = null;
			Warehouse toWarehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var waterQuery = unitOfWork.Session.QueryOver(() => incomingWaterMaterialAlias)
				.JoinQueryOver(() => incomingWaterMaterialAlias.Document, () => incomingWaterAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.IncomingWater)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Source)
			{
				if(FilterViewModel.DocumentId != null)
				{
					waterQuery.Where(() => incomingWaterAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					waterQuery.Where(() => incomingWaterAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					waterQuery.Where(() => incomingWaterAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Source)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => incomingWaterAlias.IncomingWarehouse.Id), FilterViewModel.WarehouseIds);
					}

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						waterQuery.Where(warehouseCriterion);
					}
					else
					{
						waterQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					waterQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					waterQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				waterQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				waterQuery.Where(() => incomingWaterAlias.Id == -1);
			}

			return waterQuery
				.Left.JoinAlias(() => incomingWaterAlias.IncomingWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.WriteOffWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => incomingWaterAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => incomingWaterAlias.Product, () => nomenclatureAlias)
				.SelectList(list => list.SelectGroup(() => incomingWaterAlias.Id)
					.Select(() => incomingWaterMaterialAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => incomingWaterAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => incomingWaterAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => toWarehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.IncomingWater).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(IncomingWaterMaterial)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.Where(() => fromWarehouseAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => fromWarehouseAlias.Name)))
					.WithAlias(() => resultAlias.FromStorage)
					.Select(() => fromWarehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(Projections.Conditional(
						Restrictions.Where(() => toWarehouseAlias.Name == null),
						Projections.Constant(_notSpecified, NHibernateUtil.String),
						Projections.Property(() => toWarehouseAlias.Name)))
					.WithAlias(() => resultAlias.ToStorage)
					.Select(() => toWarehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(Projections.Cast(NHibernateUtil.Decimal, Projections.Property(() => incomingWaterAlias.Amount))).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => incomingWaterAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<MovementDocumentItem> GetQueryMovementFromDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			MovementDocumentItem movementDocumentItemAlias = null;
			MovementDocument movementDocumentAlias = null;
			Warehouse fromWarehouseAlias = null;
			Warehouse toWarehouseAlias = null;
			MovementWagon movementWagonAlias = null;
			Employee fromEmployeeAlias = null;
			Employee toEmployeeAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Car fromCarAlias = null;
			CarModel fromCarModelAlias = null;
			Car toCarAlias = null;
			CarModel toCarModelAlias = null;
			Nomenclature nomenclatureAlias = null;

			var movementQuery = unitOfWork.Session.QueryOver(() => movementDocumentItemAlias)
				.JoinQueryOver(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToEmployee, () => toEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToCar, () => toCarAlias)
				.Left.JoinAlias(() => toCarAlias.CarModel, () => toCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.MovementWagon, () => movementWagonAlias)
				.Left.JoinAlias(() => movementDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => movementDocumentAlias.LastEditor, () => lastEditorAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.MovementDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Target)
			{
				if(FilterViewModel.DocumentId != null)
				{
					movementQuery.Where(() => movementDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(!FilterViewModel.ShowNotAffectedBalance)
				{
					movementQuery.Where(() => movementDocumentAlias.Status != MovementDocumentStatus.New);
				}

				if(FilterViewModel.MovementDocumentStatus != null)
				{
					movementQuery.Where(() => movementDocumentAlias.Status == FilterViewModel.MovementDocumentStatus);
				}

				if(FilterViewModel.StartDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Target)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => fromWarehouseAlias.Id), FilterViewModel.WarehouseIds);
					}

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						movementQuery.Where(warehouseCriterion);
					}
					else
					{
						movementQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					movementQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					movementQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				movementQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				movementQuery.Where(() => movementDocumentAlias.Id == -1);
			}

			return movementQuery
				.SelectList(list => list
					.Select(() => movementDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => movementDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(WarehouseDocumentsProjections.GetFromStorageProjection(_notSpecified)).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(MovementDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => movementDocumentAlias.Status).WithAlias(() => resultAlias.MovementDocumentStatus)
					.Select(() => movementDocumentAlias.HasDiscrepancy).WithAlias(() => resultAlias.MovementDocumentDiscrepancy)
					.Select(() => movementWagonAlias.Name).WithAlias(() => resultAlias.CarNumber)
					.Select(WarehouseDocumentsProjections.GetFromStorageProjection(_notSpecified)).WithAlias(() => resultAlias.FromStorage)
					.Select(WarehouseDocumentsProjections.GetFromStorageIdProjection()).WithAlias(() => resultAlias.FromStorageId)
					.Select(WarehouseDocumentsProjections.GetToStorageProjection(_notSpecified)).WithAlias(() => resultAlias.ToStorage)
					.Select(WarehouseDocumentsProjections.GetToStorageIdProjection()).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(Projections.Conditional(
						Restrictions.Eq(
							Projections.Property(() => movementDocumentAlias.Status), MovementDocumentStatus.Accepted),
							Projections.Property(() => movementDocumentItemAlias.ReceivedAmount),
							Projections.Property(() => movementDocumentItemAlias.SentAmount)))
					.WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => movementDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => movementDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<MovementDocumentItem> GetQueryMovementToDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			MovementDocumentItem movementDocumentItemAlias = null;
			MovementDocument movementDocumentAlias = null;
			Warehouse fromWarehouseAlias = null;
			Warehouse toWarehouseAlias = null;
			MovementWagon movementWagonAlias = null;
			Employee fromEmployeeAlias = null;
			Employee toEmployeeAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Car fromCarAlias = null;
			CarModel fromCarModelAlias = null;
			Car toCarAlias = null;
			CarModel toCarModelAlias = null;
			Nomenclature nomenclatureAlias = null;

			var movementQuery = unitOfWork.Session.QueryOver(() => movementDocumentItemAlias)
				.JoinQueryOver(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToEmployee, () => toEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToCar, () => toCarAlias)
				.Left.JoinAlias(() => toCarAlias.CarModel, () => toCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.MovementWagon, () => movementWagonAlias)
				.Left.JoinAlias(() => movementDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => movementDocumentAlias.LastEditor, () => lastEditorAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.MovementDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Source)
			{
				movementQuery.Where(Restrictions.In(
					Projections.Property(() => movementDocumentAlias.Status),
					new[] { MovementDocumentStatus.Accepted, MovementDocumentStatus.Discrepancy }));

				if(FilterViewModel.DocumentId != null)
				{
					movementQuery.Where(() => movementDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.MovementDocumentStatus != null)
				{
					movementQuery.Where(() => movementDocumentAlias.Status == FilterViewModel.MovementDocumentStatus);
				}

				if(FilterViewModel.StartDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Source)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => toWarehouseAlias.Id), FilterViewModel.WarehouseIds);
					}

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						movementQuery.Where(warehouseCriterion);
					}
					else
					{
						movementQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					movementQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					movementQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				movementQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				movementQuery.Where(() => movementDocumentAlias.Id == -1);
			}

			return movementQuery
				.SelectList(list => list
					.Select(() => movementDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => movementDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(WarehouseDocumentsProjections.GetToStorageProjection(_notSpecified)).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(MovementDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => movementDocumentAlias.Status).WithAlias(() => resultAlias.MovementDocumentStatus)
					.Select(() => movementDocumentAlias.HasDiscrepancy).WithAlias(() => resultAlias.MovementDocumentDiscrepancy)
					.Select(() => movementWagonAlias.Name).WithAlias(() => resultAlias.CarNumber)
					.Select(WarehouseDocumentsProjections.GetFromStorageProjection(_notSpecified)).WithAlias(() => resultAlias.FromStorage)
					.Select(WarehouseDocumentsProjections.GetFromStorageIdProjection()).WithAlias(() => resultAlias.FromStorageId)
					.Select(WarehouseDocumentsProjections.GetToStorageProjection(_notSpecified)).WithAlias(() => resultAlias.ToStorage)
					.Select(WarehouseDocumentsProjections.GetToStorageIdProjection()).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => movementDocumentItemAlias.ReceivedAmount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => movementDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => movementDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		
		private IQueryOver<WriteOffDocumentItem> GetQueryWriteOffDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			WriteOffDocumentItem writeOffDocumentItemAlias = null;
			WriteOffDocument writeOffDocumentAlias = null;
			Warehouse fromWarehouseAlias = null;
			Employee fromEmployeeAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Car fromCarAlias = null;
			CarModel fromCarModelAlias = null;
			Nomenclature nomenclatureAlias = null;

			var writeoffQuery = unitOfWork.Session.QueryOver(() => writeOffDocumentItemAlias)
				.JoinQueryOver(() => writeOffDocumentItemAlias.Document, () => writeOffDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.WriteoffDocument)
				&& FilterViewModel.Driver == null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target))
			{
				if(FilterViewModel.DocumentId != null)
				{
					writeoffQuery.Where(() => writeOffDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					writeoffQuery.Where(() => writeOffDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					writeoffQuery.Where(() => writeOffDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any()
					&& (FilterViewModel.TargetSource == TargetSource.Source || FilterViewModel.TargetSource == TargetSource.Both))
				{
					var  warehouseCriterion = Restrictions.In(Projections.Property(() => fromWarehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						writeoffQuery.Where(warehouseCriterion);
					}
					else
					{
						writeoffQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					writeoffQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					writeoffQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				writeoffQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				writeoffQuery.Where(() => writeOffDocumentAlias.Id == -1);
			}

			return writeoffQuery
				.Left.JoinAlias(() => writeOffDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => writeOffDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => writeOffDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => writeOffDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(WarehouseDocumentsProjections.GetFromStorageProjection(string.Empty)).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.WriteoffDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(WriteOffDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(WarehouseDocumentsProjections.GetFromStorageProjection(string.Empty)).WithAlias(() => resultAlias.FromStorage)
					.Select(WarehouseDocumentsProjections.GetFromStorageIdProjection()).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => writeOffDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => writeOffDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => writeOffDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<SelfDeliveryDocumentItem> GetQuerySelfDeliveryDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			SelfDeliveryDocument selfDeliveryDocumentAlias = null;
			SelfDeliveryDocumentItem selfDeliveryDocumentItemAlias = null;

			Warehouse warehouseAlias = null;
			Domain.Orders.Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var selfDeliveryQuery = unitOfWork.Session.QueryOver(() => selfDeliveryDocumentItemAlias)
				.JoinQueryOver(() => selfDeliveryDocumentItemAlias.Document, () => selfDeliveryDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.SelfDeliveryDocument)
				&& FilterViewModel.Driver == null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target))
			{
				if(FilterViewModel.DocumentId != null)
				{
					selfDeliveryQuery.Where(() => selfDeliveryDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					selfDeliveryQuery.Where(() => selfDeliveryDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					selfDeliveryQuery.Where(() => selfDeliveryDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.CounterpartyIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var counterpartyCriterion = Restrictions.In(Projections.Property(() => counterpartyAlias.Id), FilterViewModel.CounterpartyIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						selfDeliveryQuery.Where(counterpartyCriterion);
					}
					else
					{
						selfDeliveryQuery.Where(Restrictions.Not(counterpartyCriterion));
					}
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						selfDeliveryQuery.Where(warehouseCriterion);
					}
					else
					{
						selfDeliveryQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					selfDeliveryQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					selfDeliveryQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				selfDeliveryQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				selfDeliveryQuery.Where(() => selfDeliveryDocumentAlias.Id == -1);
			}

			return selfDeliveryQuery
				.Left.JoinAlias(() => selfDeliveryDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => selfDeliveryDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => selfDeliveryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => selfDeliveryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.SelfDeliveryDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(SelfDeliveryDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => selfDeliveryDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => selfDeliveryDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => selfDeliveryDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<CarLoadDocumentItem> GetQueryCarLoadDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			CarLoadDocumentItem carLoadDocumentItemAlias = null;
			CarLoadDocument carLoadDocumentAlias = null;
			Warehouse warehouseAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var carLoadQuery = unitOfWork.Session.QueryOver(() => carLoadDocumentItemAlias)
				.JoinQueryOver(() => carLoadDocumentItemAlias.Document, () => carLoadDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarLoadDocument)
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Target)
			{
				if(FilterViewModel.DocumentId != null)
				{
					carLoadQuery.Where(() => carLoadDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					carLoadQuery.Where(() => carLoadDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					carLoadQuery.Where(() => carLoadDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.Driver != null)
				{
					carLoadQuery.Where(() => driverAlias.Id == FilterViewModel.Driver.Id);
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						carLoadQuery.Where(warehouseCriterion);
					}
					else
					{
						carLoadQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					carLoadQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					carLoadQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				carLoadQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				carLoadQuery.Where(() => carLoadDocumentAlias.Id == -1);
			}

			return carLoadQuery
				.Left.JoinAlias(() => carLoadDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => carLoadDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => carLoadDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => carLoadDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.CarLoadDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(CarLoadDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => carLoadDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => carLoadDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => carLoadDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private Conjunction GetIncludeExcludeRestriction( Nomenclature nomenclatureAlias)
		{
			var restriction = Restrictions.Conjunction();

			var nomenclatureFilter = FilterViewModel.IncludeExcludeFilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();
			var includedNomenclatures = nomenclatureFilter.GetIncluded().ToArray();

			if(includedNomenclatures.Any())
			{
				restriction.Add(Restrictions.In(Projections.Property(() => nomenclatureAlias.Id), includedNomenclatures));
			}

			var excludedNomenclatures = nomenclatureFilter.GetExcluded().ToArray();

			if(excludedNomenclatures.Any())
			{
				restriction.Add(Restrictions.Not(Restrictions.In(Projections.Property(() => nomenclatureAlias.Id), excludedNomenclatures)));
			}

			var productGroupFilter = FilterViewModel.IncludeExcludeFilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();

			var includedProductGroups = productGroupFilter.GetIncluded().ToArray();

			if(includedProductGroups.Any())
			{
				restriction.Add(Restrictions.In(Projections.Property(() => nomenclatureAlias.Id), includedProductGroups));
			}

			var excludedProductGroups = productGroupFilter.GetExcluded().ToArray();

			if(excludedProductGroups.Any())
			{
				restriction.Add(Restrictions.Not(Restrictions.In(Projections.Property(() => nomenclatureAlias.Id), excludedProductGroups)));
			}

			return restriction;
		}


		private IQueryOver<CarUnloadDocumentItem> GetQueryCarUnloadDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			CarUnloadDocumentItem carUnLoadDocumentItemAlias = null;
			CarUnloadDocument carUnLoadDocumentAlias = null;
			Warehouse warehouseAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;
			GoodsAccountingOperation goodsAccountingOperationAlias = null;

			var carUnloadQuery = unitOfWork.Session.QueryOver(() => carUnLoadDocumentItemAlias)
					.JoinQueryOver(() => carUnLoadDocumentItemAlias.Document, () => carUnLoadDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarUnloadDocument)
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Source)
			{
				if(FilterViewModel.DocumentId != null)
				{
					carUnloadQuery.Where(() => carUnLoadDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					carUnloadQuery.Where(() => carUnLoadDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					carUnloadQuery.Where(() => carUnLoadDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.Driver != null)
				{
					carUnloadQuery.Where(() => driverAlias.Id == FilterViewModel.Driver.Id);
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						carUnloadQuery.Where(warehouseCriterion);
					}
					else
					{
						carUnloadQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					carUnloadQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					carUnloadQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				carUnloadQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				carUnloadQuery.Where(() => carUnLoadDocumentAlias.Id == -1);
			}

			return carUnloadQuery
				.Left.JoinAlias(() => carUnLoadDocumentItemAlias.GoodsAccountingOperation, () => goodsAccountingOperationAlias)
				.Left.JoinAlias(() => goodsAccountingOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => carUnLoadDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => carUnLoadDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => carUnLoadDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.CarUnloadDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(CarUnloadDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => goodsAccountingOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => carUnLoadDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => carUnLoadDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<InventoryDocumentItem> GetQueryInventoryDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			InventoryDocumentItem inventoryDocumentItemAlias = null;
			InventoryDocument inventoryDocumentAlias = null;
			Warehouse warehouseAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var inventoryQuery = unitOfWork.Session.QueryOver(() => inventoryDocumentItemAlias)
				.JoinQueryOver(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.InventoryDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any())
			{
				if(FilterViewModel.DocumentId != null)
				{
					inventoryQuery.Where(() => inventoryDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(!FilterViewModel.ShowNotAffectedBalance)
				{
					inventoryQuery.Where(Restrictions.NotEqProperty(
						Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
						Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)));
				}

				if(FilterViewModel.StartDate != null)
				{
					inventoryQuery.Where(() => inventoryDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					inventoryQuery.Where(() => inventoryDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						inventoryQuery.Where(warehouseCriterion);
					}
					else
					{
						inventoryQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.TargetSource != TargetSource.Both)
				{
					if(FilterViewModel.TargetSource == TargetSource.Target)
					{
						inventoryQuery.Where(() => inventoryDocumentItemAlias.AmountInFact - inventoryDocumentItemAlias.AmountInDB >= 0);
					}

					if(FilterViewModel.TargetSource == TargetSource.Source)
					{
						inventoryQuery.Where(() => inventoryDocumentItemAlias.AmountInFact - inventoryDocumentItemAlias.AmountInDB < 0);
					}
				}

				if(FilterViewModel.Author != null)
				{
					inventoryQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					inventoryQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				inventoryQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				inventoryQuery.Where(() => inventoryDocumentAlias.Id == -1);
			}

			return inventoryQuery
				.Left.JoinAlias(() => inventoryDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.SelectList(list => list
					.Select(() => inventoryDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => inventoryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => inventoryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(
						Projections.Conditional(
							Restrictions.GeProperty(
								Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
								Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
						WarehouseDocumentsProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.Source)
					.Select(
						Projections.Conditional(
							Restrictions.LtProperty(
								Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
								Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
							WarehouseDocumentsProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.InventoryDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(InventoryDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(Projections.Conditional(
						Restrictions.GeProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
						WarehouseDocumentsProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.FromStorage)
					.Select(Projections.Conditional(
						Restrictions.GeProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
						WarehouseDocumentsProjections.GetStorageIdProjection(),
						Projections.Constant(0))).WithAlias(() => resultAlias.FromStorageId)
					.Select(Projections.Conditional(
						Restrictions.LtProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
						WarehouseDocumentsProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.ToStorage)
					.Select(Projections.Conditional(
						Restrictions.LtProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB)),
						WarehouseDocumentsProjections.GetStorageIdProjection(),
						Projections.Constant(0))).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => inventoryDocumentItemAlias.AmountInFact - inventoryDocumentItemAlias.AmountInDB)
						.WithAlias(() => resultAlias.Amount)
					.Select(() => inventoryDocumentAlias.InventoryDocumentType).WithAlias(() => resultAlias.InventoryDocumentType)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => inventoryDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => inventoryDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<ShiftChangeWarehouseDocumentItem> GetQueryShiftChangeWarehouseDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			ShiftChangeWarehouseDocumentItem shiftChangeWarehouseDocumentItemAlias = null;
			ShiftChangeWarehouseDocument shiftChangeWarehouseDocumentAlias = null;

			Warehouse warehouseAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var shiftchangeQuery = unitOfWork.Session.QueryOver(() => shiftChangeWarehouseDocumentItemAlias)
				.JoinQueryOver(() => shiftChangeWarehouseDocumentItemAlias.Document, () => shiftChangeWarehouseDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.ShiftChangeDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource == TargetSource.Both)
				&& FilterViewModel.ShowNotAffectedBalance)
			{
				if(FilterViewModel.DocumentId != null)
				{
					shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						shiftchangeQuery.Where(warehouseCriterion);
					}
					else
					{
						shiftchangeQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.TargetSource != TargetSource.Both)
				{
					if(FilterViewModel.TargetSource == TargetSource.Target)
					{
						shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact - shiftChangeWarehouseDocumentItemAlias.AmountInDB >= 0);
					}

					if(FilterViewModel.TargetSource == TargetSource.Source)
					{
						shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact - shiftChangeWarehouseDocumentItemAlias.AmountInDB < 0);
					}
				}

				if(FilterViewModel.Author != null)
				{
					shiftchangeQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					shiftchangeQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				shiftchangeQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.Id == -1);
			}

			return shiftchangeQuery
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => shiftChangeWarehouseDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => shiftChangeWarehouseDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => shiftChangeWarehouseDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(
						Projections.Conditional(
							Restrictions.GeProperty(
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
							ShiftChangeDocumentProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.Source)
					.Select(
						Projections.Conditional(
							Restrictions.LtProperty(
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
						ShiftChangeDocumentProjections.GetStorageProjection(),
						Projections.Constant(string.Empty))).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.ShiftChangeDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(ShiftChangeWarehouseDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => shiftChangeWarehouseDocumentAlias.ShiftChangeResidueDocumentType)
					.WithAlias(() => resultAlias.ShiftChangeResidueDocumentType)
					.Select(
						Projections.Conditional(
							Restrictions.GeProperty(
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
							ShiftChangeDocumentProjections.GetStorageProjection(),
							Projections.Constant(string.Empty))).WithAlias(() => resultAlias.FromStorage)
					.Select(Projections.Conditional(
						Restrictions.GeProperty(
							Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
							Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
						ShiftChangeDocumentProjections.GetStorageIdProjection(),
						Projections.Constant(0))).WithAlias(() => resultAlias.FromStorageId)
					.Select(
						Projections.Conditional(
							Restrictions.LtProperty(
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
							ShiftChangeDocumentProjections.GetStorageProjection(),
							Projections.Constant(string.Empty))).WithAlias(() => resultAlias.ToStorage)
					.Select(
						Projections.Conditional(
							Restrictions.LtProperty(
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact),
								Projections.Property(() => shiftChangeWarehouseDocumentItemAlias.AmountInDB)),
							ShiftChangeDocumentProjections.GetStorageIdProjection(),
							Projections.Constant(0))).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact - shiftChangeWarehouseDocumentItemAlias.AmountInDB)
						.WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => shiftChangeWarehouseDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => shiftChangeWarehouseDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<RegradingOfGoodsDocumentItem> GetQueryRegradingOfGoodsWriteoffDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			RegradingOfGoodsDocument regradingOfGoodsDocumentAlias = null;

			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			Fine fineAlias = null;
			CullingCategory cullingCategoryAlias = null;
			RegradingOfGoodsReason regradingOfGoodsReasonAlias = null;

			var regrandingQuery = unitOfWork.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.JoinQueryOver(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.RegradingOfGoodsDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target))
			{
				if(FilterViewModel.DocumentId != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						regrandingQuery.Where(warehouseCriterion);
					}
					else
					{
						regrandingQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					regrandingQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					regrandingQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				regrandingQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == -1);
			}

			return regrandingQuery
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.NomenclatureOld, () => nomenclatureAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.Fine, () => fineAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.TypeOfDefect, () => cullingCategoryAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.RegradingOfGoodsReason, () => regradingOfGoodsReasonAlias)
				.SelectList(list => list
					.Select(() => regradingOfGoodsDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => regradingOfGoodsDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => regradingOfGoodsDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.RegradingOfGoodsDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(RegradingOfGoodsDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => -regradingOfGoodsDocumentItemAlias.Amount)
						.WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => regradingOfGoodsDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => regradingOfGoodsDocumentAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.Fine)
					.Select(() => cullingCategoryAlias.Name).WithAlias(() => resultAlias.TypeOfDefect)
					.Select(() => regradingOfGoodsDocumentItemAlias.Source).WithAlias(() => resultAlias.DefectSource)
					.Select(() => regradingOfGoodsReasonAlias.Name).WithAlias(() => resultAlias.RegradingOfGoodsReason)
					)
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<RegradingOfGoodsDocumentItem> GetQueryRegradingOfGoodsIncomeDocumentItem(IUnitOfWork unitOfWork)
		{
			
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			RegradingOfGoodsDocument regradingOfGoodsDocumentAlias = null;

			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			Fine fineAlias = null;
			CullingCategory cullingCategoryAlias = null;
			RegradingOfGoodsReason regradingOfGoodsReasonAlias = null;

			var regrandingQuery = unitOfWork.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.JoinQueryOver(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.RegradingOfGoodsDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source))
			{
				if(FilterViewModel.DocumentId != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						regrandingQuery.Where(warehouseCriterion);
					}
					else
					{
						regrandingQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					regrandingQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					regrandingQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				regrandingQuery.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == -1);
			}

			return regrandingQuery
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.NomenclatureNew, () => nomenclatureAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.Fine, () => fineAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.TypeOfDefect, () => cullingCategoryAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.RegradingOfGoodsReason, () => regradingOfGoodsReasonAlias)
				.SelectList(list => list
					.Select(() => regradingOfGoodsDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => regradingOfGoodsDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => regradingOfGoodsDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.RegradingOfGoodsDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(RegradingOfGoodsDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => regradingOfGoodsDocumentItemAlias.Amount)
						.WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => regradingOfGoodsDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => regradingOfGoodsDocumentAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.Fine)
					.Select(() => cullingCategoryAlias.Name).WithAlias(() => resultAlias.TypeOfDefect)
					.Select(() => regradingOfGoodsDocumentItemAlias.Source).WithAlias(() => resultAlias.DefectSource)
					.Select(() => regradingOfGoodsReasonAlias.Name).WithAlias(() => resultAlias.RegradingOfGoodsReason))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<DriverAttachedTerminalGiveoutDocument> GetQueryDriverAttachedTerminalGiveoutDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalGiveoutDocument driverAttachedTerminalGiveoutDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			
			var queryDriverAttachedTerminalGiveout = unitOfWork.Session.QueryOver(() => driverAttachedTerminalGiveoutDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalGiveout
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target))
			{
				if(FilterViewModel.DocumentId != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.CreationDate >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.CreationDate <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						queryDriverAttachedTerminalGiveout.Where(warehouseCriterion);
					}
					else
					{
						queryDriverAttachedTerminalGiveout.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				queryDriverAttachedTerminalGiveout.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalGiveout
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.Author, () => lastEditorAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Warehouse, () => warehouseAlias)
				.SelectList(list => list
					.Select(() => driverAttachedTerminalGiveoutDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAttachedTerminalGiveoutDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => warehouseBulkOperationAlias.OperationTime).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.DriverTerminalGiveout).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(DriverAttachedTerminalGiveoutDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => driverAttachedTerminalGiveoutDocumentAlias.CreationDate).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.CreationDate).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<DriverAttachedTerminalReturnDocument> GetQueryDriverAttachedTerminalReturnDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalReturnDocument driverAttachedTerminalReturnDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;

			var queryDriverAttachedTerminalReturn = unitOfWork.Session.QueryOver(() => driverAttachedTerminalReturnDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalReturn
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source))
			{
				if(FilterViewModel.DocumentId != null)
				{
					queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.CreationDate >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.CreationDate <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						queryDriverAttachedTerminalReturn.Where(warehouseCriterion);
					}
					else
					{
						queryDriverAttachedTerminalReturn.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					queryDriverAttachedTerminalReturn.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					queryDriverAttachedTerminalReturn.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				queryDriverAttachedTerminalReturn.Where(GetIncludeExcludeRestriction(nomenclatureAlias));
			}
			else
			{
				queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalReturn
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.Author, () => lastEditorAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Warehouse, () => warehouseAlias)
				.SelectList(list => list
					.Select(() => driverAttachedTerminalReturnDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAttachedTerminalReturnDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => warehouseBulkOperationAlias.OperationTime).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.DriverTerminalReturn).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(DriverAttachedTerminalReturnDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => driverAttachedTerminalReturnDocumentAlias.CreationDate).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.CreationDate).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		#endregion

		#region ExportReport

		protected void CreateExportJournalReportAction()
		{
			var exportJournalReportAction = new JournalAction("Выгрузка в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => {
					try
					{
						CreateJournalReportCommand?.Execute();
						ExportJournalReportCommand?.Execute();
					}
					catch(Exception)
					{
						throw;
					}
				}
			);

			NodeActionsList.Add(exportJournalReportAction);
		}

		public async Task CreateReport(CancellationToken cancellationToken)
		{
			List<WarehouseDocumentsItemsJournalNode> lines = new List<WarehouseDocumentsItemsJournalNode>();

			lines.AddRange(GetQueryIncomingInvoiceItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryIncomingWaterFromMaterial(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryIncomingWaterToMaterial(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryMovementFromDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryMovementToDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryWriteOffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQuerySelfDeliveryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarLoadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarUnloadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryInventoryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryShiftChangeWarehouseDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsWriteoffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsIncomeDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());

			_warehouseDocumentsItemsJournalReport = WarehouseDocumentsItemsJournalReport.Create(
				FilterViewModel.StartDate,
				FilterViewModel.EndDate,
				FilterViewModel.DocumentType,
				FilterViewModel.MovementDocumentStatus,
				FilterViewModel.Author?.FullName ?? string.Empty,
				FilterViewModel.LastEditor?.FullName ?? string.Empty,
				FilterViewModel.Driver?.FullName ?? string.Empty,
				FilterViewModel.Nomenclature?.Name ?? string.Empty,
				FilterViewModel.ShowNotAffectedBalance,
				FilterViewModel.TargetSource,
				FilterViewModel.CounterpartiesNames,
				FilterViewModel.WarehousesNames,
				lines.OrderByDescending(x => x.Date));

			await Task.CompletedTask;
		}

		public void ExportJournalReport()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			var template = new XLTemplate(_journalReportTemplatePath);
			template.AddVariable(_warehouseDocumentsItemsJournalReport);
			template.Generate();
			template.SaveAs(path);
		}

		#endregion ExportReport

		#region WarehouseAccountingCard

		protected void CreateExportWarhouseAccountingCardAction()
		{
			var exportJournalReportAction = new JournalAction("Выгрузить карточку складского учета",
				(selected) => FilterViewModel.Nomenclature != null
					&& FilterViewModel.WarehouseIds.Count == 1
					&& FilterViewModel.TargetSource == TargetSource.Both,
				(selected) => true,
				(selected) => {
					try
					{
						CreateWarehouseAccountingCardCommand?.Execute();
						ExportWarehouseAccountingCardCommand?.Execute();
					}
					catch(Exception)
					{
						throw;
					}
				}
			);

			NodeActionsList.Add(exportJournalReportAction);
		}

		private async Task CreateWarehouseAccountingCard(CancellationToken token)
		{
			List<WarehouseDocumentsItemsJournalNode> lines = new List<WarehouseDocumentsItemsJournalNode>();

			lines.AddRange(GetQueryIncomingInvoiceItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryIncomingWaterFromMaterial(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryIncomingWaterToMaterial(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryMovementFromDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryMovementToDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryWriteOffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQuerySelfDeliveryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarLoadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarUnloadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryInventoryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsWriteoffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsIncomeDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());

			lines.OrderByDescending(x => x.Date);

			var warehouseId = FilterViewModel.WarehouseIds.SingleOrDefault();

			var warehouseName = UoW.Query<Warehouse>().Where(x => x.Id == warehouseId)
				.SingleOrDefault().Name;

			_warehouseAccountingCard = WarehouseAccountingCard.Create(
				FilterViewModel.StartDate.Value,
				FilterViewModel.EndDate.Value,
				warehouseId,
				warehouseName,
				FilterViewModel.Nomenclature.Id,
				FilterViewModel.Nomenclature.Name,
				lines,
				GetWarehouseBalance);

			await Task.CompletedTask;
		}

		private void ExportWarehouseAccountingCard()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить карточку складского учета",
				DefaultFileExtention = ".xlsx",
				FileName = $"Карточка складского учета {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveWarehouseAccountingCard(result.Path);
			}
		}

		private void SaveWarehouseAccountingCard(string path)
		{
			var template = new XLTemplate(_warehouseAccountingCardTemplatePath);
			template.AddVariable(_warehouseAccountingCard);
			template.Generate();
			template.SaveAs(path);
		}

		#endregion WarehouseAccountingCard

		private decimal GetWarehouseBalance(int nomenclatureId, int warehouseId, DateTime upToDateTime)
		{
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclatureStockNode resultAlias = null;

			var balanceProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.Sum(() => warehouseBulkOperationAlias.Amount));

			var result = UoW.Session.QueryOver(() => nomenclatureAlias)
				.JoinEntityAlias(() => warehouseBulkOperationAlias,
					() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
						&& warehouseBulkOperationAlias.Warehouse.Id == warehouseId,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id == nomenclatureId)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(balanceProjection).WithAlias(() => resultAlias.Stock))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.SingleOrDefault<NomenclatureStockNode>();

			return result.Stock;
		}
	}
}
