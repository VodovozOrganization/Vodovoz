using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Store
{
	public class WarehouseDocumentsItemsJournalNode : JournalEntityNodeBase
	{
		public override string Title => GetTitle(EntityType);

		public int DocumentId { get; set; }

		public string NomenclatureName { get; set; }

		public int NomenclatureId { get; set; }

		public DocumentType DocumentTypeEnum { get; set; }

		public string DocumentTypeString => DocumentTypeEnum.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

		public string Description
		{
			get
			{
				switch(DocumentTypeEnum)
				{
					case DocumentType.IncomingInvoice:
						return $"Поставщик: {Counterparty}; Склад поступления: {ToWarehouse};";
					case DocumentType.IncomingWater:
						return $"Количество: {Amount}; Склад поступления: {FromWarehouse}; Продукт производства: {NomenclatureName}";
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
						switch(WriteOffType)
						{
							case WriteOffType.Warehouse:
								return !string.IsNullOrWhiteSpace(FromWarehouse) ? $"Со склада \"{FromWarehouse}\"" : FromWarehouse;
							case WriteOffType.Employee:
								return !string.IsNullOrWhiteSpace(FromEmployee) ? $"С сотрудника \"{FromEmployee}\"" : FromEmployee;
							case WriteOffType.Car:
								return !string.IsNullOrWhiteSpace(FromCar) ? $"С автомобиля \"{FromCar}\"" : FromCar;
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
							case InventoryDocumentType.WarehouseInventory:
								return $"По складу: {FromWarehouse}";
							case InventoryDocumentType.EmployeeInventory:
								return $"По сотруднику: {FromEmployee}";
							case InventoryDocumentType.CarInventory:
								return $"По автомобилю: {FromCar}";
						}
						return string.Empty;
					case DocumentType.ShiftChangeDocument:
						switch(ShiftChangeResidueDocumentType)
						{
							case ShiftChangeResidueDocumentType.Warehouse:
								return $"По складу: {FromWarehouse}";
							case ShiftChangeResidueDocumentType.Car:
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
							   $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} на склад {FromWarehouse}";
					default:
						return string.Empty;
				}
			}
		}

		public string Counterparty { get; set; }

		public int OrderId { get; set; }

		public string FromWarehouse { get; set; }

		public int FromWarehouseId { get; set; }

		public string ToWarehouse { get; set; }

		public int ToWarehouseId { get; set; }
		
		public string FromEmployee { get; set; }

		public int FromEmployeeId { get; set; }

		public string ToEmployee { get; set; }

		public int ToEmployeeId { get; set; }
		
		public string FromCar { get; set; }

		public int FromCarId { get; set; }

		public string ToCar { get; set; }

		public int ToCarId { get; set; }

		public decimal Amount { get; set; }

		public string CarModelName { get; set; }

		public string Comment { get; set; }

		public string Source { get; set; } = string.Empty;

		public string Target { get; set; } = string.Empty;

		public string CarNumber { get; set; }

		public int RouteListId { get; set; }

		public DateTime LastEditedTime { get; set; }

		public string LastEditedTimeString => LastEditedTime.ToShortDateString() + " " + LastEditedTime.ToShortTimeString();

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
		public StorageType MovementDocumentStorageFrom { get; set; }
		public MovementDocumentTypeByStorage MovementDocumentTypeByStorage { get; set; }
		public WriteOffType WriteOffType { get; set; }
		public InventoryDocumentType InventoryDocumentType { get; set; }
		public ShiftChangeResidueDocumentType ShiftChangeResidueDocumentType { get; set; }

		public bool MovementDocumentDiscrepancy { get; set; }

		private string GetTitle(Type type) => type.GetAttribute<AppellativeAttribute>(true)?.Nominative;
	}
}
