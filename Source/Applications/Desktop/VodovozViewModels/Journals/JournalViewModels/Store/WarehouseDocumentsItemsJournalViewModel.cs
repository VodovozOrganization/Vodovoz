using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsItemsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsItemsJournalNode, WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private Type[] _documentItemsTypes;

		private WarehouseDocumentsItemsJournalNode _warehouseDocumentsItemsJournalNodeAlias = null;

		public WarehouseDocumentsItemsJournalViewModel(
			WarehouseDocumentsItemsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
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
				typeof(DeliveryDocumentItem)
			};

			SearchEnabled = false;

			RegisterDocumentItems(_documentItemsTypes);

			UpdateOnChanges(_documentItemsTypes);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
		}

		private void RegisterDocumentItems(Type[] types)
		{
			RegisterEntity(GetQueryIncomingInvoiceItem)
				.AddDocumentConfiguration(
				() => null,
				(node) => null,
				(node) => node.EntityType == null,
				"Empty",
				new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
				.FinishConfiguration();

			//RegisterEntity(GetQueryIncomingWaterMaterial).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryMovementDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryWriteoffDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();
			//RegisterEntity(GetQuerySelfDeliveryDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryCarLoadDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryCarUnloadDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryInventoryDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryShiftChangeWarehouseDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryRegradingOfGoodsDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();

			//RegisterEntity(GetQueryDeliveryDocumentItem).AddDocumentConfiguration(
			//	() => null,
			//	(node) => null,
			//	(node) => node.EntityType == null,
			//	"Empty",
			//	new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true })
			//	.FinishConfiguration();






			//foreach(Type type in types)
			//{
			//	//var entityConfig = RegisterEntity()
			//}

			//var ordersConfig = RegisterEntity<VodovozOrder>(GetOrdersQuery)
			//	.AddDocumentConfiguration(
			//		//функция диалога создания документа
			//		() => null,
			//		//функция диалога открытия документа
			//		(WarehouseDocumentsItemsJournalNode node) => _gtkDialogsOpener.CreateOrderDlg(node.Id),
			//		//функция идентификации документа 
			//		(WarehouseDocumentsItemsJournalNode node) => node.EntityType == typeof(VodovozOrder),
			//		"Заказ",
			//		new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
			//	);

			////завершение конфигурации
			//ordersConfig.FinishConfiguration();
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

			var invoiceQuery = UoW.Session.QueryOver(() => incomingInvoiceItemAlias)
				.Left.JoinQueryOver(() => incomingInvoiceItemAlias.Document, () => invoiceAlias);

			if((FilterViewModel.DocumentType is null || FilterViewModel.DocumentType == DocumentType.IncomingInvoice)
				&& FilterViewModel.Driver is null)
			{
				invoiceQuery.Where(FilterViewModel.GetWarehouseSpecification<IncomingInvoice>().IsSatisfiedBy())
					.And(FilterViewModel.GetPeriodSpecification<IncomingInvoice>().IsSatisfiedBy());

				//if(FilterViewModel.Warehouse != null)
				//{
				//	invoiceQuery.Where(ii => ii.Warehouse.Id == FilterViewModel.Warehouse.Id);
				//}

				//invoiceQuery.Where(FilterViewModel.GetPeriodSpecification<IncomingInvoice>().IsSatisfiedBy());
				//if(FilterViewModel.StartDate.HasValue)
				//{
				//	invoiceQuery.Where(ii => ii.TimeStamp >= FilterViewModel.StartDate.Value);
				//}
				//if(FilterViewModel.EndDate.HasValue)
				//{
				//	invoiceQuery.Where(ii => ii.TimeStamp < FilterViewModel.EndDate.Value.AddDays(1));
				//}
			}
			else
			{
				invoiceQuery.Where(() => invoiceAlias.Id == -1);
			}

			return invoiceQuery.JoinQueryOver(() => invoiceAlias.Contractor, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => invoiceAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => invoiceAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => invoiceAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => invoiceAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime))
				.OrderByAlias(() => invoiceAlias.Id).Desc
				.OrderByAlias(() => incomingInvoiceItemAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<WarehouseDocumentsItemsJournalNode<IncomingInvoiceItem>>());
		}

		private IQueryOver<IncomingWaterMaterial> GetQueryIncomingWaterMaterial(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<IncomingWaterMaterial>();
		}

		private IQueryOver<MovementDocumentItem> GetQueryMovementDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<MovementDocumentItem>();
		}

		private IQueryOver<WriteoffDocumentItem> GetQueryWriteoffDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<WriteoffDocumentItem>();
		}

		private IQueryOver<SelfDeliveryDocumentItem> GetQuerySelfDeliveryDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<SelfDeliveryDocumentItem>();
		}

		private IQueryOver<CarLoadDocumentItem> GetQueryCarLoadDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<CarLoadDocumentItem>();
		}

		private IQueryOver<CarUnloadDocumentItem> GetQueryCarUnloadDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<CarUnloadDocumentItem>();
		}

		private IQueryOver<InventoryDocumentItem> GetQueryInventoryDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<InventoryDocumentItem>();
		}

		private IQueryOver<ShiftChangeWarehouseDocumentItem> GetQueryShiftChangeWarehouseDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<ShiftChangeWarehouseDocumentItem>();
		}

		private IQueryOver<RegradingOfGoodsDocumentItem> GetQueryRegradingOfGoodsDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<RegradingOfGoodsDocumentItem>();
		}

		private IQueryOver<DeliveryDocumentItem> GetQueryDeliveryDocumentItem(IUnitOfWork unitOfWork)
		{
			return unitOfWork.Query<DeliveryDocumentItem>();
		}
	}
}
