using ClosedXML.Report;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain;
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
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.Warehouses;
using VodovozBusiness.Domain.Documents;

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
				typeof(SelfDeliveryDocumentItemEntity),
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

			UoW.Session.DefaultReadOnly = true;

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
		}

		public DelegateCommand CreateJournalReportCommand { get; }
		public DelegateCommand ExportJournalReportCommand { get; }
		public DelegateCommand CreateWarehouseAccountingCardCommand { get; }
		public DelegateCommand ExportWarehouseAccountingCardCommand { get; }

		private bool _isIncludeFilterType =>
					FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include;

		private bool _isFilteringBySourceSelected =>
			FilterViewModel.TargetSource == TargetSource.Source || FilterViewModel.TargetSource == TargetSource.Both;

		private bool _isFilteringByTargetSelected =>
			FilterViewModel.TargetSource == TargetSource.Target || FilterViewModel.TargetSource == TargetSource.Both;

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
					(node) => node.EntityType == typeof(SelfDeliveryDocumentItemEntity))
					.FinishConfiguration();

			RegisterEntity(GetQuerySelfDeliveryReturnedDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenSelfDeliveryDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(SelfDeliveryDocumentReturned))
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
					(node) =>
						NavigationManager.OpenViewModel<RegradingOfGoodsDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(RegradingOfGoodsDocumentItem))
					.FinishConfiguration();

			RegisterEntity<DriverAttachedTerminalGiveoutDocument>(
				new Func<IUnitOfWork, IQueryOver<DriverAttachedTerminalGiveoutDocument>>[]
				{
					GetQueryDriverAttachedTerminalGiveoutFromDocument,
					GetQueryDriverAttachedTerminalGiveoutToDocument
				}.AsEnumerable())
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<DriverAttachedTerminalViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(DriverAttachedTerminalGiveoutDocument))
					.FinishConfiguration();

			RegisterEntity(
				new Func<IUnitOfWork, IQueryOver<DriverAttachedTerminalReturnDocument>>[]
				{
					GetQueryDriverAttachedTerminalReturnFromDocument,
					GetQueryDriverAttachedTerminalReturnToDocument
				}.AsEnumerable())
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
			ProductGroup productGroupAlias = null;

			var invoiceQuery = unitOfWork.Session.QueryOver(() => incomingInvoiceItemAlias)
				.Left.JoinAlias(() => incomingInvoiceItemAlias.Document, () => invoiceAlias);

			if((FilterViewModel.DocumentType is null
				|| FilterViewModel.DocumentType == DocumentType.IncomingInvoice)
				&& FilterViewModel.Driver is null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				invoiceQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				invoiceQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				invoiceQuery.Where(() => invoiceAlias.Id == -1);
			}

			return invoiceQuery
				.Left.JoinAlias(() => incomingInvoiceItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => invoiceAlias.Contractor, () => counterpartyAlias)
				.Left.JoinAlias(() => invoiceAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => invoiceAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => invoiceAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
				.OrderBy(() => invoiceAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var waterQuery = unitOfWork.Session.QueryOver(() => incomingWaterMaterialAlias)
				.JoinAlias(() => incomingWaterMaterialAlias.Document, () => incomingWaterAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.IncomingWater)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Target
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				waterQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				waterQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				waterQuery.Where(() => incomingWaterAlias.Id == -1);
			}

			return waterQuery
				.Left.JoinAlias(() => incomingWaterAlias.IncomingWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.WriteOffWarehouse, () => fromWarehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => incomingWaterAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => incomingWaterAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => incomingWaterMaterialAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
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
				.OrderBy(() => incomingWaterAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var waterQuery = unitOfWork.Session.QueryOver(() => incomingWaterMaterialAlias)
				.JoinAlias(() => incomingWaterMaterialAlias.Document, () => incomingWaterAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.IncomingWater)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Source
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				waterQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				waterQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				waterQuery.Where(() => incomingWaterAlias.Id == -1);
			}

			return waterQuery
				.Left.JoinAlias(() => incomingWaterAlias.IncomingWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.WriteOffWarehouse, () => fromWarehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => incomingWaterAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => incomingWaterAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => incomingWaterAlias.Product, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)

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
				.OrderBy(() => incomingWaterAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var movementQuery = unitOfWork.Session.QueryOver(() => movementDocumentItemAlias)
				.JoinAlias(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToEmployee, () => toEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToCar, () => toCarAlias)
				.Left.JoinAlias(() => toCarAlias.CarModel, () => toCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.MovementWagon, () => movementWagonAlias)
				.JoinEntityAlias(() => authorAlias, () => movementDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => movementDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

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
					movementQuery.Where(() => movementDocumentAlias.SendTime >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.SendTime <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Target)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => fromWarehouseAlias.Id), FilterViewModel.WarehouseIds);
					}

					if(_isIncludeFilterType)
					{
						movementQuery.Where(warehouseCriterion);
					}
					else
					{
						movementQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.EmployeeIds.Any())
				{
					var employeeCriterion = GetEmployeeCriterion(
						_isIncludeFilterType,
						() => movementDocumentAlias.FromEmployee.Id,
						() => movementDocumentAlias.StorageFrom == StorageType.Employee);

					movementQuery.Where(employeeCriterion);
				}

				if(FilterViewModel.CarIds.Any())
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => movementDocumentAlias.FromCar.Id,
						() => movementDocumentAlias.StorageFrom == StorageType.Car);

					movementQuery.Where(carCriterion);
				}

				if(FilterViewModel.Author != null)
				{
					movementQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					movementQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				movementQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				movementQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				movementQuery.Where(() => movementDocumentAlias.Id == -1);
			}

			return movementQuery
				.SelectList(list => list
					.Select(() => movementDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => movementDocumentAlias.SendTime).WithAlias(() => resultAlias.Date)
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
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "- ?1"),
								NHibernateUtil.Decimal,
								Projections.Property(() => movementDocumentItemAlias.ReceivedAmount)),
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "- ?1"),
								NHibernateUtil.Decimal,
								Projections.Property(() => movementDocumentItemAlias.SentAmount))))
					.WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => movementDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => movementDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => movementDocumentAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var movementQuery = unitOfWork.Session.QueryOver(() => movementDocumentItemAlias)
				.JoinAlias(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToWarehouse, () => toWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToEmployee, () => toEmployeeAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToCar, () => toCarAlias)
				.Left.JoinAlias(() => toCarAlias.CarModel, () => toCarModelAlias)
				.Left.JoinAlias(() => movementDocumentAlias.MovementWagon, () => movementWagonAlias)
				.JoinEntityAlias(() => authorAlias, () => movementDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => movementDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

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
					movementQuery.Where(() => movementDocumentAlias.ReceiveTime >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					movementQuery.Where(() => movementDocumentAlias.ReceiveTime <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.WarehouseIds.Any())
				{
					ICriterion warehouseCriterion = null;

					if(FilterViewModel.TargetSource != TargetSource.Source)
					{
						warehouseCriterion = Restrictions.In(Projections.Property(() => toWarehouseAlias.Id), FilterViewModel.WarehouseIds);
					}

					if(_isIncludeFilterType)
					{
						movementQuery.Where(warehouseCriterion);
					}
					else
					{
						movementQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.EmployeeIds.Any())
				{
					var employeeCriterion = GetEmployeeCriterion(
						_isIncludeFilterType,
						() => movementDocumentAlias.ToEmployee.Id,
						() => movementDocumentAlias.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToEmployee);

					movementQuery.Where(employeeCriterion);
				}

				if(FilterViewModel.CarIds.Any())
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => movementDocumentAlias.ToCar.Id,
						() => movementDocumentAlias.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToCar);

					movementQuery.Where(carCriterion);
				}

				if(FilterViewModel.Author != null)
				{
					movementQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					movementQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				movementQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				movementQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				movementQuery.Where(() => movementDocumentAlias.Id == -1);
			}

			return movementQuery
				.SelectList(list => list
					.Select(() => movementDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => movementDocumentAlias.ReceiveTime).WithAlias(() => resultAlias.Date)
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
				.OrderBy(() => movementDocumentAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var writeoffQuery = unitOfWork.Session.QueryOver(() => writeOffDocumentItemAlias)
				.JoinAlias(() => writeOffDocumentItemAlias.Document, () => writeOffDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.WriteoffDocument)
				&& FilterViewModel.Driver == null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& (!FilterViewModel.EmployeeIds.Any() || _isFilteringBySourceSelected)
				&& (!FilterViewModel.CarIds.Any() || _isFilteringBySourceSelected))
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

				var isIncludeFilterType =
					FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include;

				var isSourceFilterTarget =
					FilterViewModel.TargetSource == TargetSource.Source || FilterViewModel.TargetSource == TargetSource.Both;

				if(FilterViewModel.WarehouseIds.Any()
					&& (FilterViewModel.TargetSource == TargetSource.Source || FilterViewModel.TargetSource == TargetSource.Both))
				{
					var  warehouseCriterion = Restrictions.In(Projections.Property(() => fromWarehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(isIncludeFilterType)
					{
						writeoffQuery.Where(warehouseCriterion);
					}
					else
					{
						writeoffQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.EmployeeIds.Any() && _isFilteringBySourceSelected)
				{
					var employeeCriterion = GetEmployeeCriterion(
						isIncludeFilterType,
						() => writeOffDocumentAlias.WriteOffFromEmployee.Id,
						() => writeOffDocumentAlias.WriteOffType == WriteOffType.Employee);

					writeoffQuery.Where(employeeCriterion);
				}

				if(FilterViewModel.CarIds.Any() && _isFilteringBySourceSelected)
				{
					var carCriterion = GetCarCriterion(
						isIncludeFilterType,
						() => writeOffDocumentAlias.WriteOffFromCar.Id,
						() => writeOffDocumentAlias.WriteOffType == WriteOffType.Car);

					writeoffQuery.Where(carCriterion);
				}

				if(FilterViewModel.Author != null)
				{
					writeoffQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					writeoffQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				writeoffQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				writeoffQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				writeoffQuery.Where(() => writeOffDocumentAlias.Id == -1);
			}

			return writeoffQuery
				.Left.JoinAlias(() => writeOffDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromWarehouse, () => fromWarehouseAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromEmployee, () => fromEmployeeAlias)
				.Left.JoinAlias(() => writeOffDocumentAlias.WriteOffFromCar, () => fromCarAlias)
				.Left.JoinAlias(() => fromCarAlias.CarModel, () => fromCarModelAlias)
				.JoinEntityAlias(() => authorAlias, () => writeOffDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => writeOffDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
					.Select(() => -writeOffDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => writeOffDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => writeOffDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => writeOffDocumentAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<SelfDeliveryDocumentItemEntity> GetQuerySelfDeliveryDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			SelfDeliveryDocument selfDeliveryDocumentAlias = null;
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemAlias = null;

			Warehouse warehouseAlias = null;
			Domain.Orders.Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;

			var selfDeliveryQuery = unitOfWork.Session.QueryOver(() => selfDeliveryDocumentItemAlias)
				.JoinAlias(() => selfDeliveryDocumentItemAlias.Document, () => selfDeliveryDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.SelfDeliveryDocument)
				&& FilterViewModel.Driver == null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				selfDeliveryQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				selfDeliveryQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				selfDeliveryQuery.Where(() => selfDeliveryDocumentAlias.Id == -1);
			}

			return selfDeliveryQuery
				.Left.JoinAlias(() => selfDeliveryDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(() => authorAlias, () => selfDeliveryDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => selfDeliveryDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.SelectList(list => list
					.Select(() => selfDeliveryDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => selfDeliveryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => selfDeliveryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.SelfDeliveryDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(SelfDeliveryDocumentItemEntity)).WithAlias(() => resultAlias.EntityType)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => -selfDeliveryDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => selfDeliveryDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => selfDeliveryDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => selfDeliveryDocumentAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}


		private IQueryOver<SelfDeliveryDocumentReturned> GetQuerySelfDeliveryReturnedDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			SelfDeliveryDocument selfDeliveryDocumentAlias = null;
			SelfDeliveryDocumentReturned selfDeliveryDocumentReturnedAlias = null;

			Warehouse warehouseAlias = null;
			Domain.Orders.Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			NomenclatureEntity nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;

			var selfDeliveryReturnedQuery = unitOfWork.Session.QueryOver(() => selfDeliveryDocumentReturnedAlias)
				.JoinAlias(() => selfDeliveryDocumentReturnedAlias.Document, () => selfDeliveryDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.SelfDeliveryDocument)
				&& FilterViewModel.Driver == null
				&& (!FilterViewModel.CounterpartyIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
			{
				if(FilterViewModel.DocumentId != null)
				{
					selfDeliveryReturnedQuery.Where(() => selfDeliveryDocumentAlias.Id == FilterViewModel.DocumentId);
				}

				if(FilterViewModel.StartDate != null)
				{
					selfDeliveryReturnedQuery.Where(() => selfDeliveryDocumentAlias.TimeStamp >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate != null)
				{
					selfDeliveryReturnedQuery.Where(() => selfDeliveryDocumentAlias.TimeStamp <= FilterViewModel.EndDate.Value.LatestDayTime());
				}

				if(FilterViewModel.CounterpartyIds.Any() && FilterViewModel.TargetSource != TargetSource.Target)
				{
					var counterpartyCriterion = Restrictions.In(Projections.Property(() => counterpartyAlias.Id), FilterViewModel.CounterpartyIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						selfDeliveryReturnedQuery.Where(counterpartyCriterion);
					}
					else
					{
						selfDeliveryReturnedQuery.Where(Restrictions.Not(counterpartyCriterion));
					}
				}

				if(FilterViewModel.WarehouseIds.Any() && FilterViewModel.TargetSource != TargetSource.Source)
				{
					var warehouseCriterion = Restrictions.In(Projections.Property(() => warehouseAlias.Id), FilterViewModel.WarehouseIds);

					if(FilterViewModel.FilterType == Vodovoz.Infrastructure.Report.SelectableParametersFilter.SelectableFilterType.Include)
					{
						selfDeliveryReturnedQuery.Where(warehouseCriterion);
					}
					else
					{
						selfDeliveryReturnedQuery.Where(Restrictions.Not(warehouseCriterion));
					}
				}

				if(FilterViewModel.Author != null)
				{
					selfDeliveryReturnedQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					selfDeliveryReturnedQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				selfDeliveryReturnedQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				selfDeliveryReturnedQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				selfDeliveryReturnedQuery.Where(() => selfDeliveryDocumentAlias.Id == -1);
			}

			return selfDeliveryReturnedQuery
				.Left.JoinAlias(() => selfDeliveryDocumentReturnedAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => selfDeliveryDocumentAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(() => authorAlias, () => selfDeliveryDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => selfDeliveryDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.SelectList(list => list
					.Select(() => selfDeliveryDocumentReturnedAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => selfDeliveryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => selfDeliveryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.SelfDeliveryDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(SelfDeliveryDocumentReturned)).WithAlias(() => resultAlias.EntityType)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => selfDeliveryDocumentReturnedAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => selfDeliveryDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => selfDeliveryDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => selfDeliveryDocumentAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var carLoadQuery = unitOfWork.Session.QueryOver(() => carLoadDocumentItemAlias)
				.JoinAlias(() => carLoadDocumentItemAlias.Document, () => carLoadDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarLoadDocument)
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Target
				&& !FilterViewModel.EmployeeIds.Any())
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

				if(FilterViewModel.CarIds.Any() && _isFilteringBySourceSelected)
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => carAlias.Id);

					carLoadQuery.Where(carCriterion);
				}

				if(FilterViewModel.Author != null)
				{
					carLoadQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					carLoadQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				carLoadQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				carLoadQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				carLoadQuery.Where(() => carLoadDocumentAlias.Id == -1);
			}

			return carLoadQuery
				.Left.JoinAlias(() => carLoadDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => carLoadDocumentAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => authorAlias, () => carLoadDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => carLoadDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
					.Select(() => -carLoadDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => carLoadDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => carLoadDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => carLoadDocumentAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
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
			ProductGroup productGroupAlias = null;
			GoodsAccountingOperation goodsAccountingOperationAlias = null;

			var carUnloadQuery = unitOfWork.Session.QueryOver(() => carUnLoadDocumentItemAlias)
				.JoinAlias(() => carUnLoadDocumentItemAlias.Document, () => carUnLoadDocumentAlias);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarUnloadDocument)
				&& !FilterViewModel.CounterpartyIds.Any()
				&& FilterViewModel.TargetSource != TargetSource.Source
				&& !FilterViewModel.EmployeeIds.Any())
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

				if(FilterViewModel.CarIds.Any() && _isFilteringByTargetSelected)
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => carAlias.Id);

					carUnloadQuery.Where(carCriterion);
				}

				if(FilterViewModel.Author != null)
				{
					carUnloadQuery.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					carUnloadQuery.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				carUnloadQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				carUnloadQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				carUnloadQuery.Where(() => carUnLoadDocumentAlias.Id == -1);
			}

			return carUnloadQuery
				.Left.JoinAlias(() => carUnLoadDocumentItemAlias.GoodsAccountingOperation, () => goodsAccountingOperationAlias)
				.Left.JoinAlias(() => goodsAccountingOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => carUnLoadDocumentAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => authorAlias, () => carUnLoadDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => carUnLoadDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
				.OrderBy(() => carUnLoadDocumentAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		/// <summary>
		/// Запрос строк документа инвентаризации
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
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
			ProductGroup productGroupAlias = null;

			var inventoryQuery = unitOfWork.Session.QueryOver(() => inventoryDocumentItemAlias)
				.JoinAlias(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias);

			var restrictionIncome = Restrictions.GtProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB));

			var restrictionWriteoff = Restrictions.LtProperty(
							Projections.Property(() => inventoryDocumentItemAlias.AmountInFact),
							Projections.Property(() => inventoryDocumentItemAlias.AmountInDB));

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

				if(FilterViewModel.EmployeeIds.Any())
				{
					var employeeCriterion = GetEmployeeCriterion(
						_isIncludeFilterType,
						() => inventoryDocumentAlias.Employee.Id,
						() => inventoryDocumentAlias.InventoryDocumentType == InventoryDocumentType.EmployeeInventory);

					inventoryQuery.Where(employeeCriterion);
				}

				if(FilterViewModel.CarIds.Any())
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => inventoryDocumentAlias.Car.Id,
						() => inventoryDocumentAlias.InventoryDocumentType == InventoryDocumentType.CarInventory);

					inventoryQuery.Where(carCriterion);
				}

				if(FilterViewModel.TargetSource != TargetSource.Both)
				{
					if(FilterViewModel.TargetSource == TargetSource.Target)
					{
						inventoryQuery.Where(restrictionIncome);
					}

					if(FilterViewModel.TargetSource == TargetSource.Source)
					{
						inventoryQuery.Where(restrictionWriteoff);
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

				inventoryQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				inventoryQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				inventoryQuery.Where(() => inventoryDocumentAlias.Id == -1);
			}

			return inventoryQuery
				.Left.JoinAlias(() => inventoryDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => inventoryDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => inventoryDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => inventoryDocumentAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => inventoryDocumentAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.SelectList(list => list
					.Select(() => inventoryDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => inventoryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => inventoryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(
						Projections.Conditional(restrictionWriteoff,
							WarehouseDocumentsProjections.GetStorageProjection(),
							Projections.Constant(string.Empty)))
					.WithAlias(() => resultAlias.Source)
					.Select(
						Projections.Conditional(restrictionIncome,
							WarehouseDocumentsProjections.GetStorageProjection(),
							Projections.Constant(string.Empty)))
					.WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.InventoryDocument).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(InventoryDocumentItem)).WithAlias(() => resultAlias.EntityType)
					.Select(
						Projections.Conditional(restrictionWriteoff,
							WarehouseDocumentsProjections.GetStorageProjection(),
							Projections.Constant(string.Empty)))
					.WithAlias(() => resultAlias.FromStorage)
					.Select(
						Projections.Conditional(restrictionWriteoff,
							WarehouseDocumentsProjections.GetStorageIdProjection(),
							Projections.Constant(0)))
					.WithAlias(() => resultAlias.FromStorageId)
					.Select(
						Projections.Conditional(restrictionIncome,
							WarehouseDocumentsProjections.GetStorageProjection(),
							Projections.Constant(string.Empty)))
					.WithAlias(() => resultAlias.ToStorage)
					.Select(
						Projections.Conditional(restrictionIncome,
							WarehouseDocumentsProjections.GetStorageIdProjection(),
							Projections.Constant(0)))
					.WithAlias(() => resultAlias.ToStorageId)
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
				.OrderBy(() => inventoryDocumentAlias.TimeStamp).Desc
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
			ProductGroup productGroupAlias = null;

			var shiftchangeQuery = unitOfWork.Session.QueryOver(() => shiftChangeWarehouseDocumentItemAlias)
				.JoinAlias(() => shiftChangeWarehouseDocumentItemAlias.Document, () => shiftChangeWarehouseDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.ShiftChangeDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource == TargetSource.Both)
				&& FilterViewModel.ShowNotAffectedBalance
				&& !FilterViewModel.EmployeeIds.Any())
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

				if(FilterViewModel.CarIds.Any())
				{
					var carCriterion = GetCarCriterion(
						_isIncludeFilterType,
						() => shiftChangeWarehouseDocumentAlias.Car.Id,
						() => shiftChangeWarehouseDocumentAlias.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car);

					shiftchangeQuery.Where(carCriterion);
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

				shiftchangeQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				shiftchangeQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.Id == -1);
			}

			return shiftchangeQuery
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => authorAlias, () => shiftChangeWarehouseDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => shiftChangeWarehouseDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
				.OrderBy(() => shiftChangeWarehouseDocumentAlias.TimeStamp).Desc
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
			Employee fineEmployeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;

			Fine fineAlias = null;
			CullingCategory cullingCategoryAlias = null;
			RegradingOfGoodsReason regradingOfGoodsReasonAlias = null;

			var regrandingQuery = unitOfWork.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.JoinAlias(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias);

			var finesSubquery = QueryOver.Of<FineItem>()
				.Where(x => x.Fine.Id == fineAlias.Id)
				.JoinAlias(x => x.Employee, () => fineEmployeeAlias)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT(?1, ' ', LEFT(?2, 1), '.', LEFT(?3, 1), '.') SEPARATOR ', ')"),
						NHibernateUtil.String,
						Projections.Property(() => fineEmployeeAlias.LastName),
						Projections.Property(() => fineEmployeeAlias.Name),
						Projections.Property(() => fineEmployeeAlias.Patronymic)
					));

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.RegradingOfGoodsDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				regrandingQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				regrandingQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == -1);
			}

			return regrandingQuery
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.NomenclatureOld, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => regradingOfGoodsDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => regradingOfGoodsDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
					.SelectSubQuery(finesSubquery).WithAlias(() => resultAlias.FineEmployees)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineTotalMoney)
					.Select(() => cullingCategoryAlias.Name).WithAlias(() => resultAlias.TypeOfDefect)
					.Select(() => regradingOfGoodsDocumentItemAlias.Source).WithAlias(() => resultAlias.DefectSource)
					.Select(() => regradingOfGoodsReasonAlias.Name).WithAlias(() => resultAlias.RegradingOfGoodsReason)
					)
				.OrderBy(() => regradingOfGoodsDocumentAlias.TimeStamp).Desc
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
			Employee fineEmployeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;

			Fine fineAlias = null;
			CullingCategory cullingCategoryAlias = null;
			RegradingOfGoodsReason regradingOfGoodsReasonAlias = null;

			var regrandingQuery = unitOfWork.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.JoinAlias(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias);

			var finesSubquery = QueryOver.Of<FineItem>()
				.Where(x => x.Fine.Id == fineAlias.Id)
				.JoinAlias(x => x.Employee, () => fineEmployeeAlias)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT(?1, ' ', LEFT(?2, 1), '.', LEFT(?3, 1), '.') SEPARATOR ', ')"),
						NHibernateUtil.String,
						Projections.Property(() => fineEmployeeAlias.LastName),
						Projections.Property(() => fineEmployeeAlias.Name),
						Projections.Property(() => fineEmployeeAlias.Patronymic)
					));

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.RegradingOfGoodsDocument)
				&& FilterViewModel.Driver == null
				&& !FilterViewModel.CounterpartyIds.Any()
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				regrandingQuery.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				regrandingQuery.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == -1);
			}

			return regrandingQuery
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.NomenclatureNew, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => regradingOfGoodsDocumentAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => regradingOfGoodsDocumentAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
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
					.SelectSubQuery(finesSubquery).WithAlias(() => resultAlias.FineEmployees)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineTotalMoney)
					.Select(() => cullingCategoryAlias.Name).WithAlias(() => resultAlias.TypeOfDefect)
					.Select(() => regradingOfGoodsDocumentItemAlias.Source).WithAlias(() => resultAlias.DefectSource)
					.Select(() => regradingOfGoodsReasonAlias.Name).WithAlias(() => resultAlias.RegradingOfGoodsReason))
				.OrderBy(() => regradingOfGoodsDocumentAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode>());
		}

		private IQueryOver<DriverAttachedTerminalGiveoutDocument> GetQueryDriverAttachedTerminalGiveoutFromDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalGiveoutDocument driverAttachedTerminalGiveoutDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			
			var queryDriverAttachedTerminalGiveout = unitOfWork.Session.QueryOver(() => driverAttachedTerminalGiveoutDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalGiveout
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Target)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				queryDriverAttachedTerminalGiveout.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				queryDriverAttachedTerminalGiveout.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalGiveout
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
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

		private IQueryOver<DriverAttachedTerminalGiveoutDocument> GetQueryDriverAttachedTerminalGiveoutToDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalGiveoutDocument driverAttachedTerminalGiveoutDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperationAlias = null;
			Warehouse warehouseAlias = null;
			Employee driverAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;

			var queryDriverAttachedTerminalGiveout =
				unitOfWork.Session.QueryOver(() => driverAttachedTerminalGiveoutDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalGiveout
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& _isFilteringByTargetSelected
				&& !FilterViewModel.WarehouseIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				if(FilterViewModel.Author != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => authorAlias.Id == FilterViewModel.Author.Id);
				}

				if(FilterViewModel.LastEditor != null)
				{
					queryDriverAttachedTerminalGiveout.Where(() => lastEditorAlias.Id == FilterViewModel.LastEditor.Id);
				}

				if(FilterViewModel.EmployeeIds.Any())
				{
					var employeeCriterion = GetEmployeeCriterion(
						_isIncludeFilterType,
						() => driverAlias.Id);

					queryDriverAttachedTerminalGiveout.Where(employeeCriterion);
				}

				queryDriverAttachedTerminalGiveout.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				queryDriverAttachedTerminalGiveout.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				queryDriverAttachedTerminalGiveout.Where(() => driverAttachedTerminalGiveoutDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalGiveout
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.EmployeeNomenclatureMovementOperation, () => employeeNomenclatureMovementOperationAlias)
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => employeeNomenclatureMovementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => driverAttachedTerminalGiveoutDocumentAlias.Author, () => lastEditorAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => employeeNomenclatureMovementOperationAlias.Employee, () => driverAlias)
				.SelectList(list => list
					.Select(() => driverAttachedTerminalGiveoutDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAttachedTerminalGiveoutDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => employeeNomenclatureMovementOperationAlias.OperationTime).WithAlias(() => resultAlias.Date)
					.Select(EmployeeProjections.DriverLastNameWithInitials).WithAlias(() => resultAlias.Target)
					.Select(() => DocumentType.DriverTerminalGiveout).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(DriverAttachedTerminalGiveoutDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.FromStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => employeeNomenclatureMovementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
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

		private IQueryOver<DriverAttachedTerminalReturnDocument> GetQueryDriverAttachedTerminalReturnFromDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalReturnDocument driverAttachedTerminalReturnDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperationAlias = null;
			Warehouse warehouseAlias = null;
			Employee driverAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;

			var queryDriverAttachedTerminalReturn = unitOfWork.Session.QueryOver(() => driverAttachedTerminalReturnDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalReturn
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& _isFilteringBySourceSelected
				&& !FilterViewModel.WarehouseIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				if(FilterViewModel.EmployeeIds.Any())
				{
					var employeeCriterion = GetEmployeeCriterion(
						_isIncludeFilterType,
						() => driverAlias.Id);

					queryDriverAttachedTerminalReturn.Where(employeeCriterion);
				}

				queryDriverAttachedTerminalReturn.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				queryDriverAttachedTerminalReturn.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalReturn
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.EmployeeNomenclatureMovementOperation, () => employeeNomenclatureMovementOperationAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.Author, () => lastEditorAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => employeeNomenclatureMovementOperationAlias.Employee, () => driverAlias)
				.SelectList(list => list
					.Select(() => driverAttachedTerminalReturnDocumentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAttachedTerminalReturnDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => employeeNomenclatureMovementOperationAlias.OperationTime).WithAlias(() => resultAlias.Date)
					.Select(EmployeeProjections.DriverLastNameWithInitials).WithAlias(() => resultAlias.Source)
					.Select(() => DocumentType.DriverTerminalReturn).WithAlias(() => resultAlias.DocumentTypeEnum)
					.Select(() => typeof(DriverAttachedTerminalReturnDocument)).WithAlias(() => resultAlias.EntityType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToStorage)
					.Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.ToStorageId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => employeeNomenclatureMovementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
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

		private IQueryOver<DriverAttachedTerminalReturnDocument> GetQueryDriverAttachedTerminalReturnToDocument(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode resultAlias = null;

			DriverAttachedTerminalReturnDocument driverAttachedTerminalReturnDocumentAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;

			var queryDriverAttachedTerminalReturn = unitOfWork.Session.QueryOver(() => driverAttachedTerminalReturnDocumentAlias);

			if((FilterViewModel.DocumentType == null
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalReturn
					|| FilterViewModel.DocumentType == DocumentType.DriverTerminalMovement)
				&& (!FilterViewModel.WarehouseIds.Any() || FilterViewModel.TargetSource != TargetSource.Source)
				&& !FilterViewModel.EmployeeIds.Any()
				&& !FilterViewModel.CarIds.Any())
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

				queryDriverAttachedTerminalReturn.Where(GetIncludeExcludeNomenclatureRestriction(nomenclatureAlias));
				queryDriverAttachedTerminalReturn.Where(GetIncludeExcludeProductGroupRestriction(productGroupAlias));
			}
			else
			{
				queryDriverAttachedTerminalReturn.Where(() => driverAttachedTerminalReturnDocumentAlias.Id == -1);
			}

			return queryDriverAttachedTerminalReturn
				.Left.JoinAlias(() => driverAttachedTerminalReturnDocumentAlias.GoodsAccountingOperation, () => warehouseBulkOperationAlias)
				.Left.JoinAlias(() => warehouseBulkOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
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

		private Conjunction GetIncludeExcludeNomenclatureRestriction(NomenclatureEntity nomenclatureAlias)
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

			return restriction;
		}

		private Conjunction GetIncludeExcludeProductGroupRestriction(ProductGroup productGroupAlias)
		{
			var restriction = Restrictions.Conjunction();

			var productGroupFilter = FilterViewModel.IncludeExcludeFilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();

			var includedProductGroups = productGroupFilter.GetIncluded().ToArray();

			if(includedProductGroups.Any())
			{
				restriction.Add(Restrictions.In(Projections.Property(() => productGroupAlias.Id), includedProductGroups));
			}

			var excludedProductGroups = productGroupFilter.GetExcluded().ToArray();

			if(excludedProductGroups.Any())
			{
				restriction.Add(Restrictions.Not(Restrictions.In(Projections.Property(() => productGroupAlias.Id), excludedProductGroups)));
			}

			return restriction;
		}

		private ICriterion GetEmployeeCriterion(
			bool isIncludeFilterType,
			Expression<Func<object>> employeeIdProjectionPropertyExpression,
			params Expression<Func<bool>>[] includeFilterAdditionalRestrictionExpressions)
		{
			if(isIncludeFilterType)
			{
				return GetIncludeFilterEmployeeCriterion(
					employeeIdProjectionPropertyExpression,
					includeFilterAdditionalRestrictionExpressions);
			}

			return GetExcludeFilterEmployeeCriterion(employeeIdProjectionPropertyExpression);
		}

		private ICriterion GetCarCriterion(
			bool isIncludeFilterType,
			Expression<Func<object>> carIdProjectionPropertyExpression,
			params Expression<Func<bool>>[] includeFilterAdditionalRestrictionExpressions)
		{
			if(isIncludeFilterType)
			{
				return GetIncludeFilterCarCriterion(
					carIdProjectionPropertyExpression,
					includeFilterAdditionalRestrictionExpressions);
			}

			return GetExcludeFilterCarCriterion(carIdProjectionPropertyExpression);
		}

		private ICriterion GetIncludeFilterEmployeeCriterion(
			Expression<Func<object>> employeeIdProjectionPropertyExpression, params Expression<Func<bool>>[] additionAndRestrictionExpressions) =>
			GetIncludeFilterConjunctionCriterion(
				EmployeeIdSelectedInFilterCriterion(employeeIdProjectionPropertyExpression),
				additionAndRestrictionExpressions);

		private ICriterion GetExcludeFilterEmployeeCriterion(Expression<Func<object>> employeeIdProjectionPropertyExpression) =>
			GetExcludeFilterDisjunctionCriterion(
				EmployeeIdSelectedInFilterCriterion(employeeIdProjectionPropertyExpression),
				PropertyIsNullCriterion(employeeIdProjectionPropertyExpression));

		private ICriterion GetIncludeFilterCarCriterion(
			Expression<Func<object>> carIdProjectionPropertyExpression, params Expression<Func<bool>>[] additionAndRestrictionExpressions) =>
			GetIncludeFilterConjunctionCriterion(
				CarIdSelectedInFilterCriterion(carIdProjectionPropertyExpression),
				additionAndRestrictionExpressions);

		private ICriterion GetExcludeFilterCarCriterion(Expression<Func<object>> carIdProjectionPropertyExpression) =>
			GetExcludeFilterDisjunctionCriterion(
				CarIdSelectedInFilterCriterion(carIdProjectionPropertyExpression),
				PropertyIsNullCriterion(carIdProjectionPropertyExpression));

		private ICriterion GetIncludeFilterConjunctionCriterion(
			ICriterion propertySelectedInFilterCriterion,
			params Expression<Func<bool>>[] additionConjunctions)
		{
			var conjunction = Restrictions.Conjunction().Add(propertySelectedInFilterCriterion);

			foreach(var restriction in additionConjunctions)
			{
				conjunction.Add(restriction);
			}

			return conjunction;
		}

		private ICriterion GetExcludeFilterDisjunctionCriterion(
			ICriterion propertySelectedInFilterCriterion,
			params ICriterion[] additionalDisjunctions)
		{
			var disjunction = Restrictions.Disjunction().Add(Restrictions.Not(propertySelectedInFilterCriterion));

			foreach(var additionalDisjunction in additionalDisjunctions)
			{
				disjunction.Add(additionalDisjunction);
			}

			return disjunction;
		}

		private ICriterion PropertyIsNullCriterion(Expression<Func<object>> propertyExpression) =>
			Restrictions.IsNull(Projections.Property(propertyExpression));

		private ICriterion EmployeeIdSelectedInFilterCriterion(Expression<Func<object>> employeeIdExpression) =>
			Restrictions.In(Projections.Property(employeeIdExpression), FilterViewModel.EmployeeIds);

		private ICriterion CarIdSelectedInFilterCriterion(Expression<Func<object>> carIdExpression) =>
			Restrictions.In(Projections.Property(carIdExpression), FilterViewModel.CarIds);

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
			lines.AddRange(GetQuerySelfDeliveryReturnedDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarLoadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarUnloadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryInventoryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryShiftChangeWarehouseDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsWriteoffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsIncomeDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutFromDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutToDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnFromDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnToDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());

			_warehouseDocumentsItemsJournalReport = WarehouseDocumentsItemsJournalReport.Create(
				FilterViewModel.StartDate,
				FilterViewModel.EndDate,
				FilterViewModel.DocumentType,
				FilterViewModel.MovementDocumentStatus,
				FilterViewModel.Author?.FullName ?? string.Empty,
				FilterViewModel.LastEditor?.FullName ?? string.Empty,
				FilterViewModel.Driver?.FullName ?? string.Empty,
				FilterViewModel.SelectedNomenclatureElement?.Title ?? string.Empty,
				FilterViewModel.ShowNotAffectedBalance,
				FilterViewModel.TargetSource,
				FilterViewModel.CounterpartiesNames,
				FilterViewModel.WarehousesNames,
				lines.OrderByDescending(x => x.Date),
				FilterViewModel.IncludeExcludeFilterViewModel);

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
				(selected) => FilterViewModel.SelectedNomenclatureElement != null
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
			lines.AddRange(GetQuerySelfDeliveryReturnedDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarLoadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryCarUnloadDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryInventoryDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsWriteoffDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryRegradingOfGoodsIncomeDocumentItem(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutFromDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalGiveoutToDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnFromDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());
			lines.AddRange(GetQueryDriverAttachedTerminalReturnToDocument(UoW).List<WarehouseDocumentsItemsJournalNode>());

			lines.OrderByDescending(x => x.Date);

			var warehouseId = FilterViewModel.WarehouseIds.SingleOrDefault();

			var warehouseName = UoW.Query<Warehouse>().Where(x => x.Id == warehouseId)
				.SingleOrDefault().Name;

			_warehouseAccountingCard = WarehouseAccountingCard.Create(
				FilterViewModel.StartDate.Value,
				FilterViewModel.EndDate.Value,
				warehouseId,
				warehouseName,
				int.TryParse(FilterViewModel.SelectedNomenclatureElement.Number, out var id) ? id : 0,
				FilterViewModel.SelectedNomenclatureElement.Title,
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
				.And(() => warehouseBulkOperationAlias.OperationTime <= upToDateTime)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(balanceProjection).WithAlias(() => resultAlias.Stock))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.SingleOrDefault<NomenclatureStockNode>();

			return result.Stock;
		}
	}
}
