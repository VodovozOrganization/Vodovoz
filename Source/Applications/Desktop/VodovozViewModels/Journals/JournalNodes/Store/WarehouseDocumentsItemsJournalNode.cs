using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Documents;

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
						return $"Поставщик: {Counterparty}; Склад поступления: {FromWarehouse};";
					case DocumentType.IncomingWater:
						return $"Количество: {Amount}; Склад поступления: {FromWarehouse}; Продукт производства: {NomenclatureName}";
					case DocumentType.MovementDocument:
						var carInfo = string.IsNullOrEmpty(CarNumber) ? null : $", Фура: {CarNumber}";
						return $"{FromWarehouse} -> {ToWarehouse}{carInfo}";
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
						return string.Format("По складу: {0}", FromWarehouse);
					case DocumentType.ShiftChangeDocument:
						return string.Format("По складу: {0}", FromWarehouse);
					case DocumentType.RegradingOfGoodsDocument:
						return string.Format("По складу: {0}", FromWarehouse);
					case DocumentType.SelfDeliveryDocument:
						return string.Format("Склад: {0}, Заказ №: {1}, Клиент: {2}", FromWarehouse, OrderId, Counterparty);
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

		public decimal Amount { get; set; }

		public string CarModelName { get; set; }

		public string Comment { get; set; }

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

		public bool MovementDocumentDiscrepancy { get; set; }

		private string GetTitle(Type type) => type.GetAttribute<AppellativeAttribute>(true)?.Nominative;
	}
}
