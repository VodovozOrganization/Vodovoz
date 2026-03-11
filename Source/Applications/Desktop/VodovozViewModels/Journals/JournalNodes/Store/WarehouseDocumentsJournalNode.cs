using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Store
{
	public class WarehouseDocumentsJournalNode<TEntity> : WarehouseDocumentsJournalNode
		where TEntity : class, IDomainObject
	{
		public WarehouseDocumentsJournalNode() : base(typeof(TEntity)) { }
	}

	public class WarehouseDocumentsJournalNode : JournalEntityNodeBase
	{
		public WarehouseDocumentsJournalNode(Type entityType) : base(entityType)
		{
			if(entityType == typeof(IncomingInvoice))
			{
				DocTypeEnum = DocumentType.IncomingInvoice;
			}

			if(entityType == typeof(IncomingWater))
			{
				DocTypeEnum = DocumentType.IncomingWater;
			}

			if(entityType == typeof(MovementDocument))
			{
				DocTypeEnum = DocumentType.MovementDocument;
			}

			if(entityType == typeof(WriteOffDocument))
			{
				DocTypeEnum = DocumentType.WriteoffDocument;
			}

			if(entityType == typeof(SelfDeliveryDocument))
			{
				DocTypeEnum = DocumentType.SelfDeliveryDocument;
			}

			if(entityType == typeof(CarLoadDocument))
			{
				DocTypeEnum = DocumentType.CarLoadDocument;
			}

			if(entityType == typeof(CarUnloadDocument))
			{
				DocTypeEnum = DocumentType.CarUnloadDocument;
			}

			if(entityType == typeof(InventoryDocument))
			{
				DocTypeEnum = DocumentType.InventoryDocument;
			}

			if(entityType == typeof(ShiftChangeWarehouseDocument))
			{
				DocTypeEnum = DocumentType.ShiftChangeDocument;
			}

			if(entityType == typeof(RegradingOfGoodsDocument))
			{
				DocTypeEnum = DocumentType.RegradingOfGoodsDocument;
			}

			if(entityType == typeof(DriverAttachedTerminalGiveoutDocument))
			{
				DocTypeEnum = DocumentType.DriverTerminalGiveout;
			}

			if(entityType == typeof(DriverAttachedTerminalReturnDocument))
			{
				DocTypeEnum = DocumentType.DriverTerminalReturn;
			}

			if(DocTypeEnum is null)
			{
				throw new InvalidOperationException("Тип ноды журнала складских документов не поддерживается");
			}
		}

		public string ProductName { get; set; }

		public DocumentType? DocTypeEnum { get; set; }

		public string DocTypeString => DocTypeEnum?.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

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
						switch(InventoryDocumentType)
						{
							case Domain.Documents.InventoryDocuments.InventoryDocumentType.WarehouseInventory:
								return $"По складу: {FromWarehouse}";
							case Domain.Documents.InventoryDocuments.InventoryDocumentType.EmployeeInventory:
								return $"По сотруднику: {FromEmployee}";
							case Domain.Documents.InventoryDocuments.InventoryDocumentType.CarInventory:
								return $"По автомобилю: {FromCar}";
						}
						return string.Empty;
					case DocumentType.ShiftChangeDocument:
						switch(ShiftChangeResidueDocumentType)
						{
							case Domain.Documents.ShiftChangeResidueDocumentType.Warehouse:
								return $"По складу: {FromWarehouse}";
							case Domain.Documents.ShiftChangeResidueDocumentType.Car:
								return $"По автомобилю: {FromCar}";
						}
						return string.Empty;
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
		/// <summary>
		/// Тип передачи остатков (для документа <see cref="DocumentType.ShiftChangeDocument"/>)
		/// </summary>
		public ShiftChangeResidueDocumentType? ShiftChangeResidueDocumentType { get; set; }
		/// <summary>
		/// Тип инвентаризации (для документа <see cref="DocumentType.InventoryDocument"/>)
		/// </summary>
		public InventoryDocumentType? InventoryDocumentType { get; set; }
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

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
	}
}
