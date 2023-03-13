using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Tdi;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Vodovoz.ViewModels.ViewModels.Warehouses.Documents;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsItemsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsItemsJournalNode, WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private Type[] _documentItemsTypes;
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public WarehouseDocumentsItemsJournalViewModel(
			WarehouseDocumentsItemsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IGtkTabsOpener gtkTabsOpener)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			NavigationManager = navigationManager;

			TabName = "Журнал строк складских документов";

			_documentItemsTypes = new[]
			{
				typeof(IncomingInvoiceItem),
				typeof(IncomingWaterMaterial),
				typeof(MovementDocumentItem),
				typeof(WriteoffDocumentItem),
				typeof(SelfDeliveryDocumentItem),
				typeof(CarLoadDocumentItem),
				typeof(CarUnloadDocumentItem),
				typeof(InventoryDocumentItem),
				typeof(ShiftChangeWarehouseDocumentItem),
				typeof(RegradingOfGoodsDocumentItem),
			};

			SearchEnabled = false;

			UseSlider = false;

			RegisterDocumentItems(_documentItemsTypes);

			UpdateOnChanges(_documentItemsTypes);

			UpdateAllEntityPermissions();
			CreateNodeActions();
			CreatePopupActions();
			_gtkTabsOpener = gtkTabsOpener;
		}

		protected override void CreatePopupActions()
		{
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateOpenDocumentAction();
		}

		protected void CreateOpenDocumentAction()
		{
			var editAction = new JournalAction("Изменить",
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
					(node) => NavigationManager.OpenViewModel<IncomingInvoiceViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(IncomingInvoiceItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryIncomingWaterMaterial)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenIncomingWaterDlg(node.DocumentId),
					(node) => node.EntityType == typeof(IncomingWaterMaterial))
					.FinishConfiguration();

			RegisterEntity(GetQueryMovementDocumentItem)
				.AddDocumentConfiguration(
					() => null,
					(node) => NavigationManager.OpenViewModel<MovementDocumentViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(MovementDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryWriteoffDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenWriteoffDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(WriteoffDocumentItem))
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
					(node) => NavigationManager.OpenViewModel<InventoryDocumentViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.DocumentId)).ViewModel,
					(node) => node.EntityType == typeof(InventoryDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryShiftChangeWarehouseDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenShiftChangeWarehouseDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(ShiftChangeWarehouseDocumentItem))
					.FinishConfiguration();

			RegisterEntity(GetQueryRegradingOfGoodsDocumentItem)
				.AddDocumentConfiguration<ITdiTab>(
					() => null,
					(node) => _gtkTabsOpener.OpenRegradingOfGoodsDocumentDlg(node.DocumentId),
					(node) => node.EntityType == typeof(RegradingOfGoodsDocumentItem))
					.FinishConfiguration();

			var dataLoader = DataLoader as ThreadDataLoader<WarehouseDocumentsItemsJournalNode>;
			dataLoader.MergeInOrderBy(node => node.Date, true);
		}

		private IQueryOver<IncomingInvoiceItem> GetQueryIncomingInvoiceItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<IncomingInvoiceItem> resultAlias = null;
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
				&& FilterViewModel.Driver is null)
			{
				invoiceQuery.Where(FilterViewModel.GetWarehouseSpecification<IncomingInvoice>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<IncomingInvoice>().IsSatisfiedBy());
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
					.Select(() => DocumentType.IncomingInvoice).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias(() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => incomingInvoiceItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => invoiceAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<IncomingInvoiceItem>>());
		}

		private IQueryOver<IncomingWaterMaterial> GetQueryIncomingWaterMaterial(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<IncomingWaterMaterial> resultAlias = null;
			IncomingWaterMaterial incomingWaterMaterialAlias = null;
			IncomingWater incomingWaterAlias = null;

			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var waterQuery = unitOfWork.Session.QueryOver(() => incomingWaterMaterialAlias)
				.JoinQueryOver(() => incomingWaterMaterialAlias.Document, () => incomingWaterAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.IncomingWater)
				&& FilterViewModel.Driver == null)
			{
				waterQuery.Where(FilterViewModel.GetTwoWarhousesSpecification<IncomingWater>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<IncomingWater>().IsSatisfiedBy());
			}
			else
			{
				waterQuery.Where(() => incomingWaterAlias.Id == -1);
			}

			return waterQuery
				.Left.JoinAlias(() => incomingWaterAlias.ToWarehouse, () => warehouseAlias)
				.Left.JoinAlias(() => incomingWaterAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => incomingWaterAlias.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => incomingWaterAlias.Product, () => nomenclatureAlias)
				.SelectList(list => list
					.Select(() => incomingWaterMaterialAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => incomingWaterAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => incomingWaterAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.IncomingWater).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => incomingWaterMaterialAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => incomingWaterAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<IncomingWaterMaterial>>());
		}

		private IQueryOver<MovementDocumentItem> GetQueryMovementDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<MovementDocumentItem> resultAlias = null;

			MovementDocumentItem movementDocumentItemAlias = null;
			MovementDocument movementDocumentAlias = null;
			Warehouse warehouseAlias = null;
			Warehouse secondWarehouseAlias = null;
			MovementWagon movementWagonAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var movementQuery = unitOfWork.Session.QueryOver(() => movementDocumentItemAlias)
				.JoinQueryOver(() => movementDocumentItemAlias.Document, () => movementDocumentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.MovementDocument)
				&& FilterViewModel.Driver == null)
			{
				movementQuery.Where(FilterViewModel.GetTwoWarhousesSpecification<MovementDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<MovementDocument>().IsSatisfiedBy());
			}
			else
			{
				movementQuery.Where(() => movementDocumentAlias.Id == -1);
			}

			return movementQuery
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => movementDocumentAlias.FromWarehouse, () => warehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.ToWarehouse, () => secondWarehouseAlias)
				.Left.JoinAlias(() => movementDocumentAlias.MovementWagon, () => movementWagonAlias)
				.Left.JoinAlias(() => movementDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => movementDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => movementDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => movementDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => movementDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.MovementDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => movementDocumentAlias.Status).WithAlias(() => resultAlias.MovementDocumentStatus)
					.Select(() => movementDocumentAlias.HasDiscrepancy).WithAlias(() => resultAlias.MovementDocumentDiscrepancy)
					.Select(() => movementWagonAlias.Name).WithAlias(() => resultAlias.CarNumber)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias(() => resultAlias.Warehouse)
					.Select(Projections.Conditional(
						Restrictions.Where(() => secondWarehouseAlias.Name == null),
						Projections.Constant("Не указан", NHibernateUtil.String),
						Projections.Property(() => secondWarehouseAlias.Name)))
					.WithAlias(() => resultAlias.SecondWarehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => movementDocumentItemAlias.SendedAmount).WithAlias(() => resultAlias.Amount) // Нужно проверять (RecievedAmount не совпадает в части записей)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => movementDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => movementDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<MovementDocumentItem>>());
		}

		private IQueryOver<WriteoffDocumentItem> GetQueryWriteoffDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<WriteoffDocumentItem> resultAlias = null;

			WriteoffDocumentItem writeoffDocumentItemAlias = null;
			WriteoffDocument writeoffDocumentAlias = null;
			Counterparty counterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var writeoffQuery = unitOfWork.Session.QueryOver(() => writeoffDocumentItemAlias)
				.JoinQueryOver(() => writeoffDocumentItemAlias.Document, () => writeoffDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.WriteoffDocument) &&
				 FilterViewModel.Driver == null)
			{
				writeoffQuery.Where(FilterViewModel.GetWarehouseSpecification<WriteoffDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<WriteoffDocument>().IsSatisfiedBy());
			}
			else
			{
				writeoffQuery.Where(() => writeoffDocumentAlias.Id == -1);
			}

			return writeoffQuery
				.Left.JoinAlias(() => writeoffDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => writeoffDocumentAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => writeoffDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => writeoffDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => writeoffDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => writeoffDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => writeoffDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => writeoffDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.WriteoffDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(Projections.Conditional(
						Restrictions.Where(() => counterpartyAlias.Name == null),
						Projections.Constant(string.Empty, NHibernateUtil.String),
						Projections.Property(() => counterpartyAlias.Name)))
					.WithAlias(() => resultAlias.Counterparty)
					.Select(Projections.Conditional(
						Restrictions.Where(() => warehouseAlias.Name == null),
						Projections.Constant(string.Empty, NHibernateUtil.String),
						Projections.Property(() => warehouseAlias.Name)))
					.WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => writeoffDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => writeoffDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => writeoffDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<WriteoffDocumentItem>>());
		}

		private IQueryOver<SelfDeliveryDocumentItem> GetQuerySelfDeliveryDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<SelfDeliveryDocumentItem> resultAlias = null;

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
				&& FilterViewModel.Driver == null)
			{
				selfDeliveryQuery.Where(FilterViewModel.GetWarehouseSpecification<SelfDeliveryDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<SelfDeliveryDocument>().IsSatisfiedBy());
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
					.Select(() => DocumentType.SelfDeliveryDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
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
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<SelfDeliveryDocumentItem>>());
		}

		private IQueryOver<CarLoadDocumentItem> GetQueryCarLoadDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<CarLoadDocumentItem> resultAlias = null;

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
				.JoinQueryOver(() => carLoadDocumentItemAlias.Document, () => carLoadDocumentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarLoadDocument)
			{
				if(FilterViewModel.Driver != null)
				{
					carLoadQuery.Where(() => routeListAlias.Driver.Id == FilterViewModel.Driver.Id);
				}

				carLoadQuery.Where(FilterViewModel.GetWarehouseSpecification<CarLoadDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<CarLoadDocument>().IsSatisfiedBy());
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
					.Select(() => DocumentType.CarLoadDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
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
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<CarLoadDocumentItem>>());
		}

		private IQueryOver<CarUnloadDocumentItem> GetQueryCarUnloadDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<CarUnloadDocumentItem> resultAlias = null;

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
			WarehouseMovementOperation warehouseMovementOperationAlias = null;

			var carUnloadQuery = unitOfWork.Session.QueryOver(() => carUnLoadDocumentItemAlias)
					.JoinQueryOver(() => carUnLoadDocumentItemAlias.Document, () => carUnLoadDocumentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(FilterViewModel.DocumentType == null
				|| FilterViewModel.DocumentType == DocumentType.CarUnloadDocument)
			{
				carUnloadQuery.Where(FilterViewModel.GetWarehouseSpecification<CarUnloadDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<CarUnloadDocument>().IsSatisfiedBy());

				if(FilterViewModel.Driver != null)
				{
					carUnloadQuery.Where(() => routeListAlias.Driver.Id == FilterViewModel.Driver.Id);
				}
			}
			else
			{
				carUnloadQuery.Where(() => carUnLoadDocumentAlias.Id == -1);
			}

			return carUnloadQuery
				.Left.JoinAlias(() => carUnLoadDocumentItemAlias.WarehouseMovementOperation, () => warehouseMovementOperationAlias) // Нет данных в самом айтеме
				.Left.JoinAlias(() => warehouseMovementOperationAlias.Nomenclature, () => nomenclatureAlias)
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
					.Select(() => DocumentType.CarUnloadDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => warehouseMovementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
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
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<CarUnloadDocumentItem>>());
		}

		private IQueryOver<InventoryDocumentItem> GetQueryInventoryDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<InventoryDocumentItem> resultAlias = null;

			InventoryDocumentItem inventoryDocumentItemAlias = null;
			InventoryDocument inventoryDocumentAlias = null;
			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var inventoryQuery = unitOfWork.Session.QueryOver(() => inventoryDocumentItemAlias)
				.JoinQueryOver(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.InventoryDocument) &&
				FilterViewModel.Driver == null)
			{
				inventoryQuery.Where(FilterViewModel.GetWarehouseSpecification<InventoryDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<InventoryDocument>().IsSatisfiedBy());
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
				.SelectList(list => list
					.Select(() => inventoryDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => inventoryDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => inventoryDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.InventoryDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => inventoryDocumentItemAlias.AmountInFact).WithAlias(() => resultAlias.Amount) // AmountinDb AmountinFact???
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => inventoryDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => inventoryDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<InventoryDocumentItem>>());
		}

		private IQueryOver<ShiftChangeWarehouseDocumentItem> GetQueryShiftChangeWarehouseDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<ShiftChangeWarehouseDocumentItem> resultAlias = null;

			ShiftChangeWarehouseDocumentItem shiftChangeWarehouseDocumentItemAlias = null;
			ShiftChangeWarehouseDocument shiftChangeWarehouseDocumentAlias = null;

			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var shiftchangeQuery = unitOfWork.Session.QueryOver(() => shiftChangeWarehouseDocumentItemAlias)
				.JoinQueryOver(() => shiftChangeWarehouseDocumentItemAlias.Document, () => shiftChangeWarehouseDocumentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.ShiftChangeDocument) &&
				FilterViewModel.Driver == null)
			{
				shiftchangeQuery.Where(FilterViewModel.GetWarehouseSpecification<ShiftChangeWarehouseDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<ShiftChangeWarehouseDocument>().IsSatisfiedBy());
			}
			else
			{
				shiftchangeQuery.Where(() => shiftChangeWarehouseDocumentAlias.Id == -1);
			}

			return shiftchangeQuery
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => shiftChangeWarehouseDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => shiftChangeWarehouseDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => shiftChangeWarehouseDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => shiftChangeWarehouseDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.ShiftChangeDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => shiftChangeWarehouseDocumentItemAlias.AmountInFact).WithAlias(() => resultAlias.Amount)  // AmountinDb AmountinFact???
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => shiftChangeWarehouseDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => shiftChangeWarehouseDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<ShiftChangeWarehouseDocumentItem>>());
		}

		private IQueryOver<RegradingOfGoodsDocumentItem> GetQueryRegradingOfGoodsDocumentItem(IUnitOfWork unitOfWork)
		{
			WarehouseDocumentsItemsJournalNode<RegradingOfGoodsDocumentItem> resultAlias = null;

			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			RegradingOfGoodsDocument regradingOfGoodsDocumentAlias = null;

			Warehouse warehouseAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var regrandingQuery = unitOfWork.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.JoinQueryOver(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias);

			if((FilterViewModel.DocumentType == null || FilterViewModel.DocumentType == DocumentType.RegradingOfGoodsDocument) &&
				FilterViewModel.Driver == null)
			{
				regrandingQuery.Where(FilterViewModel.GetWarehouseSpecification<RegradingOfGoodsDocument>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<RegradingOfGoodsDocument>().IsSatisfiedBy());
			}
			else
			{
				regrandingQuery.Where(() => regradingOfGoodsDocumentAlias.Id == -1);
			}

			return regrandingQuery
				.Left.JoinAlias(() => regradingOfGoodsDocumentItemAlias.NomenclatureNew, () => nomenclatureAlias) //NomenclatureNew NomenclatureOld???
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Warehouse, () => warehouseAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => regradingOfGoodsDocumentAlias.LastEditor, () => lastEditorAlias)
				.SelectList(list => list
					.Select(() => regradingOfGoodsDocumentItemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => regradingOfGoodsDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
					.Select(() => regradingOfGoodsDocumentAlias.TimeStamp).WithAlias(() => resultAlias.Date)
					.Select(() => DocumentType.RegradingOfGoodsDocument).WithAlias(() => resultAlias.DocTypeEnum)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.Warehouse)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.ProductName)
					.Select(() => regradingOfGoodsDocumentItemAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => regradingOfGoodsDocumentAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => regradingOfGoodsDocumentAlias.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.TimeStamp).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<RegradingOfGoodsDocumentItem>>());
		}
	}
}
