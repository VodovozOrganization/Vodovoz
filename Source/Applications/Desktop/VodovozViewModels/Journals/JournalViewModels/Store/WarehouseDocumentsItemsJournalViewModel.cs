using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Documents;
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
			return unitOfWork.Query<IncomingInvoiceItem>()
				   .SelectList(list =>
				   list.Select(iii => iii.Id).WithAlias(() => _warehouseDocumentsItemsJournalNodeAlias.Id))
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
