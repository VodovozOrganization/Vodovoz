using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Tdi;
using System;
using System.Linq;
using Vodovoz.Application.Orders.Services.OrderCancellation;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Warehouses.Documents;
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
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using Vodovoz.ViewModels.Warehouses;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsJournalNode, WarehouseDocumentsJournalFilterViewModel>
	{
		private readonly Type[] _documentTypesNotAvailableToCreate = new[]
		{
			typeof(DriverAttachedTerminalDocumentBase),
			typeof(DriverAttachedTerminalGiveoutDocument),
			typeof(DriverAttachedTerminalReturnDocument)
		};

		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IGenericRepository<CarEvent> _carEventRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly SelfdeliveryCancellationService _selfdeliveryCancellationService;

		public WarehouseDocumentsJournalViewModel(
			WarehouseDocumentsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IGtkTabsOpener gtkTabsOpener,
			IGenericRepository<CarEvent> carEventRepository,
			IInteractiveService interactiveService,
			ISelfDeliveryRepository selfDeliveryRepository,
			SelfdeliveryCancellationService selfdeliveryCancellationService,
			Action<WarehouseDocumentsJournalFilterViewModel> filterConfiguration = null
		)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigationManager)
		{
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_selfDeliveryRepository = selfDeliveryRepository ?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_selfdeliveryCancellationService = selfdeliveryCancellationService ?? throw new ArgumentNullException(nameof(selfdeliveryCancellationService));
			UseSlider = false;
			SearchEnabled = true;
			TabName = "Журнал складских документов";

			RegisterDocuments();

			var threadLoader = DataLoader as ThreadDataLoader<WarehouseDocumentsJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Date, true);

			filterViewModel.Journal = this;
			JournalFilter = filterViewModel;

			if(filterConfiguration != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfiguration);
			}

			UpdateOnChanges(
				typeof(IncomingInvoice),
				typeof(IncomingWater),
				typeof(MovementDocument),
				typeof(DriverAttachedTerminalDocumentBase),
				typeof(DriverAttachedTerminalGiveoutDocument),
				typeof(DriverAttachedTerminalReturnDocument),
				typeof(WriteOffDocument),
				typeof(InventoryDocument),
				typeof(ShiftChangeWarehouseDocument),
				typeof(RegradingOfGoodsDocument),
				typeof(SelfDeliveryDocument),
				typeof(CarLoadDocument),
				typeof(CarUnloadDocument));

			UpdateAllEntityPermissions();
			CreateNodeActions();
		}

		#region Documents registration and queries

		private void RegisterDocuments()
		{
			RegisterIncomingInvoiceDocuments();
			RegisterIncomingWaterDocuments();
			RegisterMovementDocuments();
			RegisterWriteOffDocuments();
			RegisterSelfDeliveryDocuments();
			RegisterCarLoadDocuments();
			RegisterCarUnloadDocuments();
			RegisterInventoryDocuments();
			RegisterShiftChangeWarehouseDocuments();
			RegisterRegradingOfGoodsDocuments();
			RegisterDriverAttachedTerminalGiveoutDocuments();
			RegisterDriverAttachedTerminalReturnDocuments();
		}

		private void RegisterIncomingInvoiceDocuments()
		{
			RegisterEntity(GetIncomingInvoiceDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => NavigationManager.OpenViewModel<IncomingInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<IncomingInvoiceViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(IncomingInvoice))
				.FinishConfiguration();
		}

		private void RegisterIncomingWaterDocuments()
		{
			RegisterEntity(GetIncomingWaterDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => _gtkTabsOpener.CreateWarehouseDocumentOrmMainDialog(TabParent, DocumentType.IncomingWater),
					(node) => _gtkTabsOpener.OpenIncomingWaterDlg(node.Id),
					(node) => node.EntityType == typeof(IncomingWater))
				.FinishConfiguration();
		}

		private void RegisterMovementDocuments()
		{
			RegisterEntity(GetMovementDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => NavigationManager.OpenViewModel<MovementDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<MovementDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(MovementDocument))
				.FinishConfiguration();
		}

		private void RegisterWriteOffDocuments()
		{
			RegisterEntity(GetWriteOffDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => NavigationManager.OpenViewModel<WriteOffDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<WriteOffDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(WriteOffDocument))
				.FinishConfiguration();
		}

		private void RegisterInventoryDocuments()
		{
			RegisterEntity(GetInventoryDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => NavigationManager.OpenViewModel<InventoryDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<InventoryDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(InventoryDocument))
				.FinishConfiguration();
		}

		private void RegisterShiftChangeWarehouseDocuments()
		{
			RegisterEntity(GetShiftChangeWarehouseDocumentsQuery)
				.AddDocumentConfiguration(
					() => NavigationManager.OpenViewModel<ShiftChangeResidueDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<ShiftChangeResidueDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(ShiftChangeWarehouseDocument))
				.FinishConfiguration();
		}

		private void RegisterRegradingOfGoodsDocuments()
		{
			RegisterEntity(GetRegradingOfGoodsDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => NavigationManager.OpenViewModel<RegradingOfGoodsDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForCreate()).ViewModel,
					(node) => NavigationManager.OpenViewModel<RegradingOfGoodsDocumentViewModel, IEntityUoWBuilder>(
						null, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(RegradingOfGoodsDocument))
				.FinishConfiguration();
		}

		private void RegisterSelfDeliveryDocuments()
		{
			RegisterEntity(GetSelfDeliveryDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => _gtkTabsOpener.CreateWarehouseDocumentOrmMainDialog(TabParent, DocumentType.SelfDeliveryDocument),
					(node) => _gtkTabsOpener.OpenSelfDeliveryDocumentDlg(node.Id),
					(node) => node.EntityType == typeof(SelfDeliveryDocument))
				.FinishConfiguration();
		}

		private void RegisterCarLoadDocuments()
		{
			RegisterEntity(GetCarLoadDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => _gtkTabsOpener.CreateWarehouseDocumentOrmMainDialog(TabParent, DocumentType.CarLoadDocument),
					(node) => _gtkTabsOpener.OpenCarLoadDocumentDlg(node.Id),
					(node) => node.EntityType == typeof(CarLoadDocument))
				.FinishConfiguration();
		}

		private void RegisterCarUnloadDocuments()
		{
			RegisterEntity(GetCarUnloadDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => _gtkTabsOpener.CreateWarehouseDocumentOrmMainDialog(TabParent, DocumentType.CarUnloadDocument),
					(node) => _gtkTabsOpener.OpenCarUnloadDocumentDlg(node.Id),
					(node) => node.EntityType == typeof(CarUnloadDocument))
				.FinishConfiguration();
		}

		private void RegisterDriverAttachedTerminalGiveoutDocuments()
		{
			RegisterEntity(GetDriverAttachedTerminalGiveoutDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => NavigationManager.OpenViewModel<DriverAttachedTerminalViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(DriverAttachedTerminalGiveoutDocument))
				.FinishConfiguration();
		}

		private void RegisterDriverAttachedTerminalReturnDocuments()
		{
			RegisterEntity(GetDriverAttachedTerminalReturnDocumentsQuery)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => NavigationManager.OpenViewModel<DriverAttachedTerminalViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					(node) => node.EntityType == typeof(DriverAttachedTerminalReturnDocument))
				.FinishConfiguration();
		}

		private IQueryOver<IncomingInvoice> GetIncomingInvoiceDocumentsQuery(IUnitOfWork uow)
		{
			IncomingInvoice invoiceAlias = null;
			Counterparty counterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => invoiceAlias)
				.Left.JoinAlias(() => invoiceAlias.Contractor, () => counterpartyAlias)
				.Left.JoinAlias(() => invoiceAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => invoiceAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => invoiceAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.IncomingInvoice)
				|| FilterViewModel.Driver != null
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => invoiceAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => invoiceAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => invoiceAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => invoiceAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			query.Where(GetSearchCriterion(
				() => invoiceAlias.Id,
				() => counterpartyAlias.Name,
				() => warehouseAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => invoiceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => invoiceAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => invoiceAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => DocumentType.IncomingInvoice).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name))).WithAlias(() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name))).WithAlias(() => resultAlias.ToWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => invoiceAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(() => invoiceAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<IncomingInvoice>>());

			return resultQuery;
		}

		private IQueryOver<IncomingWater> GetIncomingWaterDocumentsQuery(IUnitOfWork uow)
		{
			IncomingWater waterAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature productAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => waterAlias)
				.JoinEntityAlias(() => authorAlias, () => waterAlias.AuthorId ==authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => waterAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => waterAlias.IncomingWarehouse, () => warehouseAlias)
				.Left.JoinAlias(() => waterAlias.Product, () => productAlias);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.IncomingWater)
				|| FilterViewModel.Driver != null
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => waterAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => waterAlias.IncomingWarehouse.Id == FilterViewModel.Warehouse.Id
					|| waterAlias.WriteOffWarehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => waterAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => waterAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			query.Where(GetSearchCriterion(
				() => waterAlias.Id,
				() => warehouseAlias.Name,
				() => productAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => waterAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => waterAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.IncomingWater).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name))).WithAlias(() => resultAlias.ToWarehouse)
					.Select(() => productAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => waterAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => waterAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(() => waterAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<IncomingWater>>());

			return resultQuery;
		}

		private IQueryOver<MovementDocument> GetMovementDocumentsQuery(IUnitOfWork uow)
		{
			MovementDocument movementAlias = null;
			Warehouse warehouseAlias = null;
			Warehouse secondWarehouseAlias = null;
			MovementWagon wagonAlias = null;
			Car carStorageFromAlias = null;
			Car carStorageToAlias = null;
			CarModel carStorageModelFromAlias = null;
			CarModel carStorageModelToAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Employee employeeStorageFromAlias = null;
			Employee employeeStorageToAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => movementAlias)
				.Left.JoinAlias(() => movementAlias.FromWarehouse, () => warehouseAlias)
				.Left.JoinAlias(() => movementAlias.ToWarehouse, () => secondWarehouseAlias)
				.Left.JoinAlias(() => movementAlias.FromEmployee, () => employeeStorageFromAlias)
				.Left.JoinAlias(() => movementAlias.ToEmployee, () => employeeStorageToAlias)
				.Left.JoinAlias(() => movementAlias.FromCar, () => carStorageFromAlias)
				.Left.JoinAlias(() => carStorageFromAlias.CarModel, () => carStorageModelFromAlias)
				.Left.JoinAlias(() => movementAlias.ToCar, () => carStorageToAlias)
				.Left.JoinAlias(() => carStorageToAlias.CarModel, () => carStorageModelToAlias)
				.Left.JoinAlias(() => movementAlias.MovementWagon, () => wagonAlias)
				.JoinEntityAlias(() => authorAlias, () => movementAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => movementAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

			if(FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.MovementDocument)
			{
				query.Where(() => movementAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => movementAlias.FromWarehouse.Id == FilterViewModel.Warehouse.Id
					|| movementAlias.ToWarehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => movementAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => movementAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.MovementDocumentStatus.HasValue && FilterViewModel.DocumentType == DocumentType.MovementDocument)
			{
				query.Where(() => movementAlias.Status == FilterViewModel.MovementDocumentStatus.Value);
			}

			if(FilterViewModel.Employee != null)
			{
				query.Where(() =>
					(movementAlias.FromEmployee.Id == FilterViewModel.Employee.Id && movementAlias.StorageFrom == StorageType.Employee)
					|| (movementAlias.ToEmployee.Id == FilterViewModel.Employee.Id && movementAlias.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToEmployee));
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() =>
					(movementAlias.FromCar.Id == FilterViewModel.Car.Id && movementAlias.StorageFrom == StorageType.Car)
					|| (movementAlias.ToCar.Id == FilterViewModel.Car.Id && movementAlias.MovementDocumentTypeByStorage == MovementDocumentTypeByStorage.ToCar));
			}

			query.Where(GetSearchCriterion(
				() => movementAlias.Id,
				() => wagonAlias.Name,
				() => warehouseAlias.Name,
				() => secondWarehouseAlias.Name,
				() => employeeStorageFromAlias.LastName,
				() => employeeStorageToAlias.LastName,
				() => carStorageFromAlias.RegistrationNumber,
				() => carStorageToAlias.RegistrationNumber
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => movementAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => movementAlias.Status).WithAlias(() => resultAlias.MovementDocumentStatus)
					.Select(() => movementAlias.HasDiscrepancy).WithAlias(() => resultAlias.MovementDocumentDiscrepancy)
					.Select(() => movementAlias.StorageFrom).WithAlias(() => resultAlias.MovementDocumentStorageFrom)
					.Select(() => movementAlias.MovementDocumentTypeByStorage).WithAlias(() => resultAlias.MovementDocumentTypeByStorage)
					.Select(() => wagonAlias.Name).WithAlias(() => resultAlias.CarNumber)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name))).WithAlias(() => resultAlias.FromWarehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => secondWarehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => secondWarehouseAlias.Name))).WithAlias(() => resultAlias.ToWarehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => employeeStorageFromAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => employeeStorageFromAlias.LastName,
							() => employeeStorageFromAlias.Name,
							() => employeeStorageFromAlias.Patronymic))).WithAlias(() => resultAlias.FromEmployee)
					.Select(Projections.Conditional(
						Restrictions.Where(() => employeeStorageToAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => employeeStorageToAlias.LastName,
							() => employeeStorageToAlias.Name,
							() => employeeStorageToAlias.Patronymic))).WithAlias(() => resultAlias.ToEmployee)
					.Select(Projections.Conditional(
						Restrictions.Where(() => carStorageModelFromAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => carStorageModelFromAlias.Name,
							() => carStorageFromAlias.RegistrationNumber))).WithAlias(() => resultAlias.FromCar)
					.Select(Projections.Conditional(
						Restrictions.Where(() => carStorageModelToAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => carStorageModelToAlias.Name,
							() => carStorageToAlias.RegistrationNumber))).WithAlias(() => resultAlias.ToCar)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => movementAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => movementAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => movementAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<MovementDocument>>());

			return resultQuery;
		}

		private IQueryOver<WriteOffDocument> GetWriteOffDocumentsQuery(IUnitOfWork uow)
		{
			WriteOffDocument writeOffAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => writeOffAlias)
				.JoinEntityAlias(() => authorAlias, () => authorAlias.Id == writeOffAlias.AuthorId, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => lastEditorAlias.Id == writeOffAlias.LastEditorId, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => writeOffAlias.WriteOffFromWarehouse, () => warehouseAlias);

			if(FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.WriteoffDocument)
			{
				query.Where(() => writeOffAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => writeOffAlias.WriteOffFromWarehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => writeOffAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => writeOffAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Employee != null)
			{
				query.Where(() =>
					writeOffAlias.WriteOffFromEmployee.Id == FilterViewModel.Employee.Id && writeOffAlias.WriteOffType == WriteOffType.Employee);
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() =>
					writeOffAlias.WriteOffFromCar.Id == FilterViewModel.Car.Id && writeOffAlias.WriteOffType == WriteOffType.Car);
			}

			query.Where(GetSearchCriterion(
				() => writeOffAlias.Id,
				() => warehouseAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => writeOffAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => writeOffAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.WriteoffDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Constant(string.Empty, NHibernateUtil.String)).WithAlias(() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant(string.Empty, NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name))).WithAlias(() => resultAlias.FromWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => writeOffAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => writeOffAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => writeOffAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<WriteOffDocument>>());

			return resultQuery;
		}

		private IQueryOver<InventoryDocument> GetInventoryDocumentsQuery(IUnitOfWork uow)
		{
			InventoryDocument inventoryAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;
			Car carStorageFromAlias = null;
			CarModel carStorageModelFromAlias = null;
			Employee employeeStorageFromAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => inventoryAlias)
				.Left.JoinAlias(() => inventoryAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => inventoryAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => inventoryAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => inventoryAlias.Employee, () => employeeStorageFromAlias)
				.Left.JoinAlias(() => inventoryAlias.Car, () => carStorageFromAlias)
				.Left.JoinAlias(() => carStorageFromAlias.CarModel, () => carStorageModelFromAlias);

			if(FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.InventoryDocument)
			{
				query.Where(() => inventoryAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => inventoryAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => inventoryAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => inventoryAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Employee != null)
			{
				query.Where(() =>
					inventoryAlias.Employee.Id == FilterViewModel.Employee.Id && inventoryAlias.InventoryDocumentType == InventoryDocumentType.EmployeeInventory);
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() =>
					inventoryAlias.Car.Id == FilterViewModel.Car.Id && inventoryAlias.InventoryDocumentType == InventoryDocumentType.CarInventory);
			}

			query.Where(GetSearchCriterion(
				() => inventoryAlias.Id,
				() => warehouseAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => inventoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => inventoryAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.InventoryDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => inventoryAlias.InventoryDocumentType).WithAlias(() => resultAlias.InventoryDocumentType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => employeeStorageFromAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => employeeStorageFromAlias.LastName,
							() => employeeStorageFromAlias.Name,
							() => employeeStorageFromAlias.Patronymic))).WithAlias(() => resultAlias.FromEmployee)
					.Select(Projections.Conditional(
						Restrictions.Where(() => carStorageModelFromAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => carStorageModelFromAlias.Name,
							() => carStorageFromAlias.RegistrationNumber))).WithAlias(() => resultAlias.FromCar)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => inventoryAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => inventoryAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => inventoryAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<InventoryDocument>>());

			return resultQuery;
		}

		private IQueryOver<ShiftChangeWarehouseDocument> GetShiftChangeWarehouseDocumentsQuery(IUnitOfWork uow)
		{
			ShiftChangeWarehouseDocument shiftchangeAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;
			Car carStorageFromAlias = null;
			CarModel carStorageModelFromAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => shiftchangeAlias)
				.Left.JoinAlias(() => shiftchangeAlias.Warehouse, () => warehouseAlias)
				.JoinEntityAlias(() => authorAlias, () => shiftchangeAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => shiftchangeAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => shiftchangeAlias.Car, () => carStorageFromAlias)
				.Left.JoinAlias(() => carStorageFromAlias.CarModel, () => carStorageModelFromAlias);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.ShiftChangeDocument)
				|| FilterViewModel.Driver != null
				|| FilterViewModel.Employee != null)
			{
				query.Where(() => shiftchangeAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => shiftchangeAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => shiftchangeAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => shiftchangeAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() =>
					shiftchangeAlias.Car.Id == FilterViewModel.Car.Id && shiftchangeAlias.ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car);
			}

			query.Where(GetSearchCriterion(
				() => shiftchangeAlias.Id,
				() => warehouseAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => shiftchangeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => shiftchangeAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.ShiftChangeDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => shiftchangeAlias.ShiftChangeResidueDocumentType)
						.WithAlias(() => resultAlias.ShiftChangeResidueDocumentType)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => carStorageModelFromAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						CustomProjections.Concat_WS(" ",
							() => carStorageModelFromAlias.Name,
							() => carStorageFromAlias.RegistrationNumber))).WithAlias(() => resultAlias.FromCar)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => shiftchangeAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => shiftchangeAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => shiftchangeAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<ShiftChangeWarehouseDocument>>());

			return resultQuery;
		}

		private IQueryOver<RegradingOfGoodsDocument> GetRegradingOfGoodsDocumentsQuery(IUnitOfWork uow)
		{
			RegradingOfGoodsDocument regradingOfGoodsAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => regradingOfGoodsAlias)
				.JoinEntityAlias(() => authorAlias, () => regradingOfGoodsAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => regradingOfGoodsAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => regradingOfGoodsAlias.Warehouse, () => warehouseAlias);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.RegradingOfGoodsDocument)
				|| FilterViewModel.Driver != null
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => regradingOfGoodsAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => regradingOfGoodsAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}
			if(startDate.HasValue)
			{
				query.Where(() => regradingOfGoodsAlias.TimeStamp >= startDate.Value);
			}
			if(endDate.HasValue)
			{
				query.Where(() => regradingOfGoodsAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			query.Where(GetSearchCriterion(
				() => regradingOfGoodsAlias.Id,
				() => warehouseAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => regradingOfGoodsAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => regradingOfGoodsAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.RegradingOfGoodsDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => regradingOfGoodsAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => regradingOfGoodsAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => regradingOfGoodsAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<RegradingOfGoodsDocument>>());

			return resultQuery;
		}

		private IQueryOver<SelfDeliveryDocument> GetSelfDeliveryDocumentsQuery(IUnitOfWork uow)
		{
			SelfDeliveryDocument selfDeliveryAlias = null;
			Counterparty counterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Domain.Orders.Order orderAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => selfDeliveryAlias)
				.Left.JoinAlias(() => selfDeliveryAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => selfDeliveryAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(() => authorAlias, () => selfDeliveryAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => selfDeliveryAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.SelfDeliveryDocument)
				|| FilterViewModel.Driver != null
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => selfDeliveryAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => selfDeliveryAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => selfDeliveryAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => selfDeliveryAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			query.Where(GetSearchCriterion(
				() => selfDeliveryAlias.Id,
				() => warehouseAlias.Name,
				() => orderAlias.Id,
				() => counterpartyAlias.Name
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => selfDeliveryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => selfDeliveryAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.SelfDeliveryDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => selfDeliveryAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => selfDeliveryAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => selfDeliveryAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<SelfDeliveryDocument>>());

			return resultQuery;
		}

		private IQueryOver<CarLoadDocument> GetCarLoadDocumentsQuery(IUnitOfWork uow)
		{
			Warehouse warehouseAlias = null;
			CarLoadDocument loadCarAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => loadCarAlias)
				.Left.JoinAlias(() => loadCarAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => loadCarAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => authorAlias, () => loadCarAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => loadCarAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.CarLoadDocument)
				|| FilterViewModel.Employee != null)
			{
				query.Where(() => loadCarAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => loadCarAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => loadCarAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => loadCarAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Driver != null)
			{
				query.Where(() => routeListAlias.Driver.Id == FilterViewModel.Driver.Id);
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() => routeListAlias.Car.Id == FilterViewModel.Car.Id);
			}

			if(FilterViewModel.ShowOnlyQRScanRequiredCarLoadDocuments && FilterViewModel.OnlyQRScanRequiredCarLoadDocuments)
			{
				var paymentTypes = new PaymentType[]
				{
					PaymentType.Cashless
				};

				RouteListItem routeListItemAlias = null;
				Order orderAlias = null;
				Counterparty counterpartyAlias = null;

				var hasOrdersWithNetworkClientRequiredQRCodeScan = QueryOver.Of<RouteListItem>(() => routeListItemAlias)
					.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
					.Inner.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
					.Where(() => routeListItemAlias.RouteList.Id == routeListAlias.Id
						&& counterpartyAlias.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), paymentTypes))
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(hasOrdersWithNetworkClientRequiredQRCodeScan.DetachedCriteria));
			}

			query.Where(GetSearchCriterion(
				() => loadCarAlias.Id,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverAlias.LastName,
				() => routeListAlias.Id
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => loadCarAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => loadCarAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.CarLoadDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => loadCarAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => loadCarAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => loadCarAlias.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<CarLoadDocument>>());

			return resultQuery;
		}

		private IQueryOver<CarUnloadDocument> GetCarUnloadDocumentsQuery(IUnitOfWork uow)
		{
			Warehouse warehouseAlias = null;
			CarUnloadDocument unloadCarAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => unloadCarAlias)
				.Left.JoinAlias(() => unloadCarAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => unloadCarAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => authorAlias, () => unloadCarAlias.AuthorId == authorAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => lastEditorAlias, () => unloadCarAlias.LastEditorId == lastEditorAlias.Id, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null && FilterViewModel.DocumentType != DocumentType.CarUnloadDocument)
				|| FilterViewModel.Employee != null)
			{
				query.Where(() => unloadCarAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => unloadCarAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => unloadCarAlias.TimeStamp >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => unloadCarAlias.TimeStamp < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Driver != null)
			{
				query.Where(() => routeListAlias.Driver.Id == FilterViewModel.Driver.Id);
			}

			if(FilterViewModel.Car != null)
			{
				query.Where(() => routeListAlias.Car.Id == FilterViewModel.Car.Id);
			}

			query.Where(GetSearchCriterion(
				() => unloadCarAlias.Id,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverAlias.LastName,
				() => routeListAlias.Id
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => unloadCarAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => unloadCarAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.CarUnloadDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToWarehouse)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => unloadCarAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => unloadCarAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(() => unloadCarAlias.TimeStamp).Desc
			.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<CarUnloadDocument>>());

			return resultQuery;
		}

		private IQueryOver<DriverAttachedTerminalGiveoutDocument> GetDriverAttachedTerminalGiveoutDocumentsQuery(IUnitOfWork uow)
		{
			Warehouse warehouseAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			DriverAttachedTerminalGiveoutDocument terminalGiveoutAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => terminalGiveoutAlias)
				.Left.JoinAlias(() => terminalGiveoutAlias.GoodsAccountingOperation, () => operationAlias)
				.JoinEntityAlias(
					() => warehouseAlias,
					Restrictions.And(
						Restrictions.Lt(Projections.Property(() => operationAlias.Amount), 0),
						Restrictions.EqProperty(Projections.Property(() => operationAlias.Warehouse.Id), Projections.Property(() => warehouseAlias.Id))),
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => terminalGiveoutAlias.Driver, () => driverAlias)
				.JoinAlias(() => terminalGiveoutAlias.Author, () => authorAlias, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null
					&& FilterViewModel.DocumentType != DocumentType.DriverTerminalMovement
					&& FilterViewModel.DocumentType != DocumentType.DriverTerminalGiveout)
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => terminalGiveoutAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => operationAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => terminalGiveoutAlias.CreationDate >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => terminalGiveoutAlias.CreationDate < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Driver != null)
			{
				query.Where(() => terminalGiveoutAlias.Driver.Id == FilterViewModel.Driver.Id);
			}

			query.Where(GetSearchCriterion(
				() => terminalGiveoutAlias.Id,
				() => warehouseAlias.Name,
				() => driverAlias.LastName
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => terminalGiveoutAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => terminalGiveoutAlias.CreationDate).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.DriverTerminalGiveout).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic))
				.OrderBy(() => terminalGiveoutAlias.CreationDate).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<DriverAttachedTerminalGiveoutDocument>>());

			return resultQuery;
		}

		private IQueryOver<DriverAttachedTerminalReturnDocument> GetDriverAttachedTerminalReturnDocumentsQuery(IUnitOfWork uow)
		{
			Warehouse warehouseAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			DriverAttachedTerminalReturnDocument terminalReturnAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			WarehouseDocumentsJournalNode resultAlias = null;

			var startDate = FilterViewModel.StartDate;
			var endDate = FilterViewModel.EndDate;

			var query = uow.Session.QueryOver(() => terminalReturnAlias)
				.Left.JoinAlias(() => terminalReturnAlias.GoodsAccountingOperation, () => operationAlias)
				.JoinEntityAlias(() => warehouseAlias, Restrictions.And(
						Restrictions.Gt(Projections.Property(() => operationAlias.Amount), 0),
						Restrictions.EqProperty(Projections.Property(() => operationAlias.Warehouse.Id), Projections.Property(() => warehouseAlias.Id))),
						JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => terminalReturnAlias.Driver, () => driverAlias)
				.JoinAlias(() => terminalReturnAlias.Author, () => authorAlias, JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType != null
				&& FilterViewModel.DocumentType != DocumentType.DriverTerminalMovement
				&& FilterViewModel.DocumentType != DocumentType.DriverTerminalReturn)
				|| FilterViewModel.Employee != null
				|| FilterViewModel.Car != null)
			{
				query.Where(() => terminalReturnAlias.Id == -1);
			}

			if(FilterViewModel.Warehouse != null)
			{
				query.Where(() => operationAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			if(startDate.HasValue)
			{
				query.Where(() => terminalReturnAlias.CreationDate >= startDate.Value);
			}

			if(endDate.HasValue)
			{
				query.Where(() => terminalReturnAlias.CreationDate < endDate.Value.AddDays(1));
			}

			if(FilterViewModel.Driver != null)
			{
				query.Where(() => terminalReturnAlias.Driver.Id == FilterViewModel.Driver.Id);
			}

			query.Where(GetSearchCriterion(
				() => terminalReturnAlias.Id,
				() => warehouseAlias.Name,
				() => driverAlias.LastName
				));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => terminalReturnAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => terminalReturnAlias.CreationDate).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.DriverTerminalReturn).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.ToWarehouse)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic))
				.OrderBy(() => terminalReturnAlias.CreationDate).Desc
			.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsJournalNode<DriverAttachedTerminalReturnDocument>>());

			return resultQuery;
		}

		#endregion

		#region Node actions
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateCustomAddActions();
			CreateCustomEditAction();
			CreateCustomDeleteAction();
		}

		private void CreateCustomDeleteAction()
		{

			var deleteAction = new JournalAction("Удалить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<WarehouseDocumentsJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanDelete;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<WarehouseDocumentsJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					if(selectedNode.DocTypeEnum == DocumentType.WriteoffDocument)
					{
						var carEvent = _carEventRepository.Get(UoW, x => x.WriteOffDocument.Id == selectedNode.Id, 1).FirstOrDefault();

						if(carEvent != null)
						{
							_interactiveService.ShowMessage(
								ImportanceLevel.Warning,
								$"Данный акт списания привязан к событию ТС {carEvent.Id} {carEvent.CarEventType.Name}",
								"Невозможно удалить документ");

							return;
						}
					}

					if(selectedNode.DocTypeEnum == DocumentType.SelfDeliveryDocument)
					{
						var selfdeliveryDocument = UoW.Session.Get<SelfDeliveryDocument>(selectedNode.Id);
						var permit = _selfdeliveryCancellationService.CanDeleteSelfdeliveryDocument(UoW, selfdeliveryDocument);

						switch(permit.Type)
						{
							case OrderCancellationPermitType.AllowCancelDocflow:
								_selfdeliveryCancellationService.CancelDocflowByUser(
									$"Удаление отпуска самовывоза №{selectedNode.Id} по заказу №{selfdeliveryDocument.Order.Id}", 
									permit.EdoTaskToCancellationId.Value
								);
								return;
							case OrderCancellationPermitType.AllowCancelOrder:
								// Разрешение на удаление самовывоза
								break;
							case OrderCancellationPermitType.None:
							case OrderCancellationPermitType.Deny:
							default:
								// Запрет удаления
								return;
						}
					}

					var config = EntityConfigs[selectedNode.EntityType];

					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);

			NodeActionsList.Add(deleteAction);
		}

		protected void CreateCustomAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var addParentNodeAction = new JournalAction(
					"Добавить",
					(selected) => true,
					(selected) => true,
					(selected) => { });

			foreach(var entityConfig in EntityConfigs.Values)
			{
				foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
				{
					foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
					{
						var childNodeAction = new JournalAction(
							createDlgConfig.Title,
							(selected) =>
							{
								var isSensitive = entityConfig.PermissionResult.CanCreate
									&& !_documentTypesNotAvailableToCreate.Contains(entityConfig.EntityType);

								return isSensitive;
							},
							(selected) => true,
							(selected) =>
							{
								createDlgConfig.OpenEntityDialogFunction.Invoke();

								if(documentConfig.JournalParameters.HideJournalForCreateDialog)
								{
									HideJournal(TabParent);
								}
							}
						);
						addParentNodeAction.ChildActionsList.Add(childNodeAction);
					}
				}
			}
			NodeActionsList.Add(addParentNodeAction);
		}

		protected void CreateCustomEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<WarehouseDocumentsJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate || config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<WarehouseDocumentsJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
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

		#endregion
	}
}
