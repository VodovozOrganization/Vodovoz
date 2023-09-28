using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.Charts;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;
using Gtk;
using Gdk;
using QS.Project.DB;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModel
{
	public class DocumentsVM : RepresentationModelWithoutEntityBase<DocumentVMNode>
	{
		public StockDocumentsFilter Filter
		{
			get => RepresentationFilter as StockDocumentsFilter;
			set => RepresentationFilter = value;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			IncomingInvoice invoiceAlias = null;
			IncomingWater waterAlias = null;
			MovementDocument movementAlias = null;
			WriteOffDocument writeOffAlias = null;
			InventoryDocument inventoryAlias = null;
			ShiftChangeWarehouseDocument shiftchangeAlias = null;
			SelfDeliveryDocument selfDeliveryAlias = null;
			RegradingOfGoodsDocument regradingOfGoodsAlias = null;
			DocumentVMNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Warehouse warehouseAlias = null;
			Warehouse secondWarehouseAlias = null;
			MovementWagon wagonAlias = null;
			Nomenclature productAlias = null;

			CarLoadDocument loadCarAlias = null;
			CarUnloadDocument unloadCarAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			Car carStorageFromAlias = null;
			Car carStorageToAlias = null;
			CarModel carModelAlias = null;
			CarModel carStorageModelFromAlias = null;
			CarModel carStorageModelToAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			Employee employeeStorageFromAlias = null;
			Employee employeeStorageToAlias = null;
			Domain.Orders.Order orderAlias = null;
			DriverAttachedTerminalGiveoutDocument terminalGiveoutAlias = null;
			DriverAttachedTerminalReturnDocument terminalReturnAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;

			List<DocumentVMNode> result = new List<DocumentVMNode>();

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.IncomingInvoice) &&
			   Filter.RestrictDriver == null)
			{
				var invoiceQuery = UoW.Session.QueryOver<IncomingInvoice>(() => invoiceAlias);
				if(Filter.RestrictWarehouse != null)
				{
					invoiceQuery.Where(x => x.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					invoiceQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					invoiceQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var invoiceList = invoiceQuery.JoinQueryOver(() => invoiceAlias.Contractor, () => counterpartyAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => invoiceAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => invoiceAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => invoiceAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
						.Select(() => invoiceAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					)
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(invoiceList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.IncomingWater) &&
			   Filter.RestrictDriver == null)
			{
				var waterQuery = UoW.Session.QueryOver<IncomingWater>(() => waterAlias);
				if(Filter.RestrictWarehouse != null)
				{
					waterQuery.Where(x => x.IncomingWarehouse.Id == Filter.RestrictWarehouse.Id ||
					                      x.WriteOffWarehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					waterQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					waterQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var waterList = waterQuery
					.JoinQueryOver(() => waterAlias.IncomingWarehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => waterAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => waterAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.Left.JoinAlias(() => waterAlias.Product, () => productAlias)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(waterList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.MovementDocument) &&
			   Filter.RestrictDriver == null)
			{
				var movementQuery = UoW.Session.QueryOver<MovementDocument>(() => movementAlias);
				if(Filter.RestrictWarehouse != null)
				{
					movementQuery.Where(x => x.FromWarehouse.Id == Filter.RestrictWarehouse.Id ||
					                         x.ToWarehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					movementQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					movementQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}
				if(Filter.RestrictMovementStatus.HasValue && Filter.RestrictDocumentType == DocumentType.MovementDocument)
				{
					movementQuery.Where(o => o.Status == Filter.RestrictMovementStatus.Value);
				}

				var movementList = movementQuery
					.Left.JoinAlias(() => movementAlias.FromWarehouse, () => warehouseAlias)
					.Left.JoinAlias(() => movementAlias.ToWarehouse, () => secondWarehouseAlias)
					.Left.JoinAlias(() => movementAlias.FromEmployee, () => employeeStorageFromAlias)
					.Left.JoinAlias(() => movementAlias.ToEmployee, () => employeeStorageToAlias)
					.Left.JoinAlias(() => movementAlias.FromCar, () => carStorageFromAlias)
					.Left.JoinAlias(() => carStorageFromAlias.CarModel, () => carStorageModelFromAlias)
					.Left.JoinAlias(() => movementAlias.ToCar, () => carStorageToAlias)
					.Left.JoinAlias(() => carStorageToAlias.CarModel, () => carStorageModelToAlias)
					.Left.JoinAlias(() => movementAlias.MovementWagon, () => wagonAlias)
					.Left.JoinAlias(() => movementAlias.Author, () => authorAlias)
					.Left.JoinAlias(() => movementAlias.LastEditor, () => lastEditorAlias)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(movementList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.WriteoffDocument) &&
			   Filter.RestrictDriver == null)
			{
				var writeoffQuery = UoW.Session.QueryOver<WriteOffDocument>(() => writeOffAlias);
				if(Filter.RestrictWarehouse != null)
				{
					writeoffQuery.Where(x => x.WriteOffFromWarehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					writeoffQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					writeoffQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var writeoffList = writeoffQuery
					.JoinQueryOver(() => writeOffAlias.WriteOffFromWarehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeOffAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => writeOffAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
						.Select(() => writeOffAlias.Comment).WithAlias(() => resultAlias.Comment)
					)
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(writeoffList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.InventoryDocument) &&
			   Filter.RestrictDriver == null)
			{
				var inventoryQuery = UoW.Session.QueryOver<InventoryDocument>(() => inventoryAlias);
				if(Filter.RestrictWarehouse != null)
				{
					inventoryQuery.Where(x => x.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					inventoryQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					inventoryQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var inventoryList = inventoryQuery
					.JoinQueryOver(() => inventoryAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => inventoryAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => inventoryAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.SelectList(list => list
						.Select(() => inventoryAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => inventoryAlias.TimeStamp).WithAlias(() => resultAlias.Date)
						.Select(() => DocumentType.InventoryDocument).WithAlias(() => resultAlias.DocTypeEnum)
						.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
						.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
						.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
						.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
						.Select(() => inventoryAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
						.Select(() => inventoryAlias.Comment).WithAlias(() => resultAlias.Comment))
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(inventoryList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.ShiftChangeDocument) &&
			   Filter.RestrictDriver == null)
			{
				var shiftchangeQuery = UoW.Session.QueryOver<ShiftChangeWarehouseDocument>(() => shiftchangeAlias);
				if(Filter.RestrictWarehouse != null)
				{
					shiftchangeQuery.Where(x => x.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					shiftchangeQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					shiftchangeQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var shiftchangeList = shiftchangeQuery
					.JoinQueryOver(() => shiftchangeAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => shiftchangeAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => shiftchangeAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.SelectList(list => list
						.Select(() => shiftchangeAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => shiftchangeAlias.TimeStamp).WithAlias(() => resultAlias.Date)
						.Select(() => DocumentType.ShiftChangeDocument).WithAlias(() => resultAlias.DocTypeEnum)
						.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.FromWarehouse)
						.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
						.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
						.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
						.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
						.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
						.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
						.Select(() => shiftchangeAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
						.Select(() => shiftchangeAlias.Comment).WithAlias(() => resultAlias.Comment))
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(shiftchangeList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.RegradingOfGoodsDocument) &&
			   Filter.RestrictDriver == null)
			{
				var regrandingQuery = UoW.Session.QueryOver<RegradingOfGoodsDocument>(() => regradingOfGoodsAlias);
				if(Filter.RestrictWarehouse != null)
				{
					regrandingQuery.Where(x => x.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					regrandingQuery.Where(o => o.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					regrandingQuery.Where(o => o.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var regrandingList = regrandingQuery
					.JoinQueryOver(() => regradingOfGoodsAlias.Warehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => regradingOfGoodsAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => regradingOfGoodsAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(regrandingList);
			}

			if((Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.SelfDeliveryDocument) &&
			   Filter.RestrictDriver == null)
			{
				var selfDeliveryQuery = UoW.Session.QueryOver<SelfDeliveryDocument>(() => selfDeliveryAlias)
					.JoinQueryOver(() => selfDeliveryAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => selfDeliveryAlias.Order, () => orderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => orderAlias.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					selfDeliveryQuery.Where(() => selfDeliveryAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					selfDeliveryQuery.Where(() => selfDeliveryAlias.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					selfDeliveryQuery.Where(() => selfDeliveryAlias.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				var selfDeliveryList = selfDeliveryQuery
					.JoinAlias(() => selfDeliveryAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => selfDeliveryAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(selfDeliveryList);
			}

			if(Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.CarLoadDocument)
			{
				var carLoadQuery = UoW.Session.QueryOver<CarLoadDocument>(() => loadCarAlias)
					.JoinQueryOver(() => loadCarAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => loadCarAlias.RouteList, () => routeListAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => routeListAlias.Car, () => carAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => routeListAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => carAlias.CarModel, () => carModelAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					carLoadQuery.Where(() => loadCarAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}
				if(Filter.RestrictStartDate.HasValue)
				{
					carLoadQuery.Where(() => loadCarAlias.TimeStamp >= Filter.RestrictStartDate.Value);
				}
				if(Filter.RestrictEndDate.HasValue)
				{
					carLoadQuery.Where(() => loadCarAlias.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}
				if(Filter.RestrictDriver != null)
				{
					carLoadQuery.Where(() => routeListAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var carLoadList = carLoadQuery
					.JoinAlias(() => loadCarAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => loadCarAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(carLoadList);
			}

			if(Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.CarUnloadDocument)
			{
				var carUnloadQuery = UoW.Session.QueryOver<CarUnloadDocument>(() => unloadCarAlias)
					.JoinQueryOver(() => unloadCarAlias.Warehouse, () => warehouseAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => unloadCarAlias.RouteList, () => routeListAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => routeListAlias.Car, () => carAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => routeListAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => carAlias.CarModel, () => carModelAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					carUnloadQuery.Where(() => unloadCarAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}

				if(Filter.RestrictStartDate.HasValue)
				{
					carUnloadQuery.Where(() => unloadCarAlias.TimeStamp >= Filter.RestrictStartDate.Value);
				}

				if(Filter.RestrictEndDate.HasValue)
				{
					carUnloadQuery.Where(() => unloadCarAlias.TimeStamp < Filter.RestrictEndDate.Value.AddDays(1));
				}

				if(Filter.RestrictDriver != null)
				{
					carUnloadQuery.Where(() => routeListAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var carUnloadList = carUnloadQuery
					.JoinAlias(() => unloadCarAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinAlias(() => unloadCarAlias.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();

				result.AddRange(carUnloadList);
			}

			//TODO проверить работу журнала
			if(Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == DocumentType.DriverTerminalMovement)
			{
				#region Giveout

				var driverTerminalGiveoutQuery = UoW.Session.QueryOver(() => terminalGiveoutAlias)
					.JoinQueryOver(() => terminalGiveoutAlias.GoodsAccountingOperation, () => operationAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => operationAlias.Warehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin,
						Restrictions.Lt(Projections.Property(() => operationAlias.Amount), 0))
					.JoinQueryOver(() => terminalGiveoutAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					driverTerminalGiveoutQuery.Where(() => operationAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}

				if(Filter.RestrictStartDate.HasValue)
				{
					driverTerminalGiveoutQuery.Where(() => terminalGiveoutAlias.CreationDate >= Filter.RestrictStartDate.Value);
				}

				if(Filter.RestrictEndDate.HasValue)
				{
					driverTerminalGiveoutQuery.Where(() => terminalGiveoutAlias.CreationDate < Filter.RestrictEndDate.Value.AddDays(1));
				}

				if(Filter.RestrictDriver != null)
				{
					driverTerminalGiveoutQuery.Where(() => terminalGiveoutAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var terminalGiveoutDocs = driverTerminalGiveoutQuery
					.JoinAlias(() => terminalGiveoutAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();
				result.AddRange(terminalGiveoutDocs);

				#endregion

				#region Return

				var driverTerminalReturnQuery = UoW.Session.QueryOver(() => terminalReturnAlias)
					.JoinQueryOver(() => terminalReturnAlias.GoodsAccountingOperation, () => operationAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => operationAlias.Warehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin,
						Restrictions.Gt(Projections.Property(() => operationAlias.Amount), 0))
					.JoinQueryOver(() => terminalReturnAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					driverTerminalReturnQuery.Where(() => operationAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}

				if(Filter.RestrictStartDate.HasValue)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.CreationDate >= Filter.RestrictStartDate.Value);
				}

				if(Filter.RestrictEndDate.HasValue)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.CreationDate < Filter.RestrictEndDate.Value.AddDays(1));
				}

				if(Filter.RestrictDriver != null)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var terminalReturnDocs = driverTerminalReturnQuery
					.JoinAlias(() => terminalReturnAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();
				result.AddRange(terminalReturnDocs);

				#endregion
			}

			if(Filter.RestrictDocumentType == DocumentType.DriverTerminalGiveout)
			{
				var driverterminalGiveoutQuery = UoW.Session.QueryOver(() => terminalGiveoutAlias)
					.JoinQueryOver(() => terminalGiveoutAlias.GoodsAccountingOperation, () => operationAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => operationAlias.Warehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin,
						Restrictions.Lt(Projections.Property(() => operationAlias.Amount), 0))
					.JoinQueryOver(() => terminalGiveoutAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					driverterminalGiveoutQuery.Where(() => operationAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}

				if(Filter.RestrictStartDate.HasValue)
				{
					driverterminalGiveoutQuery.Where(() => terminalGiveoutAlias.CreationDate >= Filter.RestrictStartDate.Value);
				}

				if(Filter.RestrictEndDate.HasValue)
				{
					driverterminalGiveoutQuery.Where(() => terminalGiveoutAlias.CreationDate < Filter.RestrictEndDate.Value.AddDays(1));
				}

				if(Filter.RestrictDriver != null)
				{
					driverterminalGiveoutQuery.Where(() => terminalGiveoutAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var terminalGiveoutDocs = driverterminalGiveoutQuery
					.JoinAlias(() => terminalGiveoutAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();
				result.AddRange(terminalGiveoutDocs);
			}

			if(Filter.RestrictDocumentType == DocumentType.DriverTerminalReturn)
			{
				var driverTerminalReturnQuery = UoW.Session.QueryOver(() => terminalReturnAlias)
					.JoinQueryOver(() => terminalReturnAlias.GoodsAccountingOperation, () => operationAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver(() => operationAlias.Warehouse, () => warehouseAlias,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin,
						Restrictions.Gt(Projections.Property(() => operationAlias.Amount), 0))
					.JoinQueryOver(() => terminalReturnAlias.Driver, () => driverAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

				if(Filter.RestrictWarehouse != null)
				{
					driverTerminalReturnQuery.Where(() => operationAlias.Warehouse.Id == Filter.RestrictWarehouse.Id);
				}

				if(Filter.RestrictStartDate.HasValue)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.CreationDate >= Filter.RestrictStartDate.Value);
				}

				if(Filter.RestrictEndDate.HasValue)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.CreationDate < Filter.RestrictEndDate.Value.AddDays(1));
				}

				if(Filter.RestrictDriver != null)
				{
					driverTerminalReturnQuery.Where(() => terminalReturnAlias.Driver.Id == Filter.RestrictDriver.Id);
				}

				var terminalReturnDocs = driverTerminalReturnQuery
					.JoinAlias(() => terminalReturnAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.TransformUsing(Transformers.AliasToBean<DocumentVMNode>())
					.List<DocumentVMNode>();
				result.AddRange(terminalReturnDocs);
			}

			result.Sort((x, y) =>
			{
				if(x.Date < y.Date)
					return 1;
				if(x.Date == y.Date)
					return 0;
				return -1;
			});

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<DocumentVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
			.AddColumn("Тип документа").AddTextRenderer(node => node.DocTypeString)
			.AddColumn("Дата").AddTextRenderer(node => node.DateString)
			.AddColumn("Автор").AddTextRenderer(node => node.Author)
			.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
			.AddColumn("Послед. изменения").AddTextRenderer(node =>
				node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString() : string.Empty)
			.AddColumn("Детали").AddTextRenderer(node => node.Description).SearchHighlight()
			.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
			.RowCells()
			.AddSetter<CellRenderer>((cell, node) =>
			{
				Color color = GdkColors.PrimaryBase;
				if(node.DocTypeEnum == DocumentType.MovementDocument)
				{
					switch(node.MovementDocumentStatus)
					{
						case MovementDocumentStatus.Sended:
							color = GdkColors.WarningText;
							break;
						case MovementDocumentStatus.Discrepancy:
							color = GdkColors.Pink;
							break;
						case MovementDocumentStatus.Accepted:
							color = node.MovementDocumentDiscrepancy ? GdkColors.BabyBlue : color;
							break;
					}
				}

				cell.CellBackgroundGdk = color;
			})
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(object updatedSubject)
		{
			return true;
		}

		#endregion

		public DocumentsVM(StockDocumentsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public DocumentsVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new StockDocumentsFilter(UoW);
		}

		public DocumentsVM(IUnitOfWork uow) : base(
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
			typeof(DriverAttachedTerminalDocumentBase)
		)
		{
			this.UoW = uow;
		}
	}

	public class DocumentVMNode
	{
		[UseForSearch] public int Id { get; set; }

		public string ProductName { get; set; }

		public DocumentType DocTypeEnum { get; set; }

		public string DocTypeString => DocTypeEnum.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

		[UseForSearch]
		public string Description
		{
			get
			{
				switch(DocTypeEnum)
				{
					case DocumentType.IncomingInvoice:
						return $"Поставщик: {Counterparty}; Склад поступления: {ToWarehouse};";
					case DocumentType.IncomingWater:
						return $"Количество: {Amount}; Склад поступления: {ToWarehouse}; Продукт производства: {ProductName}";
					case DocumentType.MovementDocument:
						var carInfo = string.IsNullOrEmpty(CarNumber) ? null : $", Фура: {CarNumber}";
						switch(MovementDocumentTypeByStorage)
						{
							case MovementDocumentTypeByStorage.ToWarehouse:
								switch(MovementDocumentStorageFrom)
								{
									case StorageType.Warehouse:
										return $"{FromWarehouse} -> {ToWarehouse}{carInfo}";
									case StorageType.Employee:
										return $"{FromEmployee} -> {ToWarehouse}";
									case StorageType.Car:
										return $"{FromCar} -> {ToWarehouse}";
								}
								break;
							case MovementDocumentTypeByStorage.ToEmployee:
								switch(MovementDocumentStorageFrom)
								{
									case StorageType.Warehouse:
										return $"{FromWarehouse} -> {ToEmployee}";
									case StorageType.Employee:
										return $"{FromEmployee} -> {ToEmployee}";
									case StorageType.Car:
										return $"{FromCar} -> {ToEmployee}";
								}
								break;
							case MovementDocumentTypeByStorage.ToCar:
								switch(MovementDocumentStorageFrom)
								{
									case StorageType.Warehouse:
										return $"{FromWarehouse} -> {ToCar}";
									case StorageType.Employee:
										return $"{FromEmployee} -> {ToCar}";
									case StorageType.Car:
										return $"{FromCar} -> {ToCar}";
								}
								break;
						}
						return string.Empty;
					case DocumentType.WriteoffDocument:
						if(FromWarehouse != string.Empty)
						{
							return $"Со склада \"{FromWarehouse}\"";
						}

						if(Counterparty != string.Empty)
						{
							return $"От клиента \"{Counterparty}\"";
						}

						return string.Empty;
					case DocumentType.CarLoadDocument:
						return string.Format(
							"Маршрутный лист: {3} Автомобиль: {0} ({1}) Водитель: {2}",
							CarModelName,
							CarNumber,
							PersonHelper.PersonNameWithInitials(
								DriverSurname,
								DriverName,
								DriverPatronymic
							),
							RouteListId
						);
					case DocumentType.CarUnloadDocument:
						return string.Format(
							"Маршрутный лист: {3} Автомобиль: {0} ({1}) Водитель: {2}",
							CarModelName,
							CarNumber,
							PersonHelper.PersonNameWithInitials(
								DriverSurname,
								DriverName,
								DriverPatronymic
							),
							RouteListId
						);
					case DocumentType.InventoryDocument:
						return $"По складу: {FromWarehouse}";
					case DocumentType.ShiftChangeDocument:
						return $"По складу: {FromWarehouse}";
					case DocumentType.RegradingOfGoodsDocument:
						return $"По складу: {FromWarehouse}";
					case DocumentType.SelfDeliveryDocument:
						return $"Склад: {FromWarehouse}, Заказ №: {OrderId}, Клиент: {Counterparty}";
					case DocumentType.DriverTerminalGiveout:
						return "Выдача терминала водителю " +
						       $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} со склада {FromWarehouse}";
					case DocumentType.DriverTerminalReturn:
						return "Возврат терминала водителем " +
						       $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} на склад {ToWarehouse}";
					default:
						return string.Empty;
				}
			}
		}

		public string Counterparty { get; set; }

		public int OrderId { get; set; }

		public string FromWarehouse { get; set; }

		public string ToWarehouse { get; set; }
		
		public string FromEmployee { get; set; }

		public string ToEmployee { get; set; }
		
		public string FromCar { get; set; }

		public string ToCar { get; set; }
		public StorageType MovementDocumentStorageFrom { get; set; }
		public MovementDocumentTypeByStorage MovementDocumentTypeByStorage { get; set; }
		public int Amount { get; set; }

		public string CarModelName { get; set; }

		public string Comment { get; set; }

		public string CarNumber { get; set; }

		public int RouteListId { get; set; }

		public DateTime LastEditedTime { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public string LastEditorSurname { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string LastEditor => PersonHelper.PersonNameWithInitials(LastEditorSurname, LastEditorName, LastEditorPatronymic);

		public string DriverSurname { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }

		public MovementDocumentStatus MovementDocumentStatus { get; set; }

		public bool MovementDocumentDiscrepancy { get; set; }
	}
}
