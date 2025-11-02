using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Core.Domain.Warehouses.Documents;
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
						return $"Поставщик: {Counterparty}; Склад поступления: {ToStorage};";
					case DocumentType.IncomingWater:
						return $"Количество: {Amount}; Склад поступления: {ToStorage}; Продукт производства: {NomenclatureName}";
					case DocumentType.MovementDocument:
						var carInfo = string.IsNullOrEmpty(CarNumber) ? null : $", Фура: {CarNumber}";
						return $"{FromStorage} -> {ToStorage}{carInfo}";
					case DocumentType.WriteoffDocument:
						return !string.IsNullOrWhiteSpace(FromStorage) ? $"С \"{FromStorage}\"" : FromStorage;
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
						return $"По: {FromStorage}";
					case DocumentType.ShiftChangeDocument:
						return $"По: {FromStorage}";
					case DocumentType.RegradingOfGoodsDocument:
						return $"По складу: {FromStorage}";
					case DocumentType.SelfDeliveryDocument:
						return string.IsNullOrWhiteSpace(FromStorage)
							? $"Склад: {ToStorage}, Заказ №: {OrderId}, Клиент: {Counterparty}"
							: $"Склад: {FromStorage}, Заказ №: {OrderId}, Клиент: {Counterparty}";
					case DocumentType.DriverTerminalGiveout:
						return "Выдача терминала водителю " +
							   $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} со склада {FromStorage}";
					case DocumentType.DriverTerminalReturn:
						return "Возврат терминала водителем " +
							   $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} на склад {ToStorage}";
					default:
						return string.Empty;
				}
			}
		}

		public string Counterparty { get; set; }

		public int OrderId { get; set; }

		public string FromStorage { get; set; }

		public int FromStorageId { get; set; }
		public StorageType StorageFromType { get; set; }

		public string ToStorage { get; set; }

		public int ToStorageId { get; set; }

		public decimal Amount { get; set; }

		public string CarModelName { get; set; }

		public string Comment { get; set; }

		/// <summary>
		/// Откуда списалась номенклатура
		/// Исключает <see cref="Target"/>
		/// </summary>
		public string Source { get; set; } = string.Empty;

		/// <summary>
		/// Куда пришла номенклатура
		/// Исключает <see cref="Source"/>
		/// </summary>
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
		public WriteOffType WriteOffType { get; set; }
		public InventoryDocumentType InventoryDocumentType { get; set; }
		public ShiftChangeResidueDocumentType ShiftChangeResidueDocumentType { get; set; }

		public bool MovementDocumentDiscrepancy { get; set; }
		public string FineEmployees { get; set; }
		public decimal FineTotalMoney { get; set; }
		public string FinesDescription => string.IsNullOrWhiteSpace(FineEmployees) ? "" : $"({FineEmployees}) = {FineTotalMoney:# ### ### ##0.00 ₽}";
		public string FinesDescriptionForReport => string.IsNullOrWhiteSpace(FineEmployees) ? "" : $"({FineEmployees}) = {FineTotalMoney:# ### ### ##0.00}";
		public string TypeOfDefect { get; set; }
		public DefectSource DefectSource { get; set; }
		public string DefectSourceString => DefectSource.GetEnumTitle();
		public string RegradingOfGoodsReason { get; set; }

		private string GetTitle(Type type) => type.GetAttribute<AppellativeAttribute>(true)?.Nominative;
	}
}
