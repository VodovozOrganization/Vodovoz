using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Store
{
	public class WarehouseDocumentsItemsJournalNode<TEntity> : WarehouseDocumentsItemsJournalNode
		where TEntity : class, IDomainObject
	{
		public WarehouseDocumentsItemsJournalNode() : base(typeof(TEntity)) { }
	}

	public class WarehouseDocumentsItemsJournalNode : JournalEntityNodeBase
	{
		private Type[] _supportedDocuments = new[]
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

		protected WarehouseDocumentsItemsJournalNode(Type entityType) : base(entityType)
		{
			if(!_supportedDocuments.Contains(entityType))
			{
				throw new ArgumentOutOfRangeException(nameof(entityType));
			}

			Title = GetTitle(entityType);
		}

		public int DocumentId { get; set; }

		public string ProductName { get; set; }

		public DocumentType DocTypeEnum { get; set; }

		public string DocTypeString => DocTypeEnum.GetEnumTitle();

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

		public string Description
		{
			get
			{
				switch(DocTypeEnum)
				{
					case DocumentType.IncomingInvoice:
						return $"Поставщик: {Counterparty}; Склад поступления: {Warehouse};";
					case DocumentType.IncomingWater:
						return $"Количество: {Amount}; Склад поступления: {Warehouse}; Продукт производства: {ProductName}";
					case DocumentType.MovementDocument:
						var carInfo = string.IsNullOrEmpty(CarNumber) ? null : $", Фура: {CarNumber}";
						return $"{Warehouse} -> {SecondWarehouse}{carInfo}";
					case DocumentType.WriteoffDocument:
						if(Warehouse != string.Empty)
						{
							return $"Со склада \"{Warehouse}\"";
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
						return string.Format("По складу: {0}", Warehouse);
					case DocumentType.ShiftChangeDocument:
						return string.Format("По складу: {0}", Warehouse);
					case DocumentType.RegradingOfGoodsDocument:
						return string.Format("По складу: {0}", Warehouse);
					case DocumentType.SelfDeliveryDocument:
						return string.Format("Склад: {0}, Заказ №: {1}, Клиент: {2}", Warehouse, OrderId, Counterparty);
					case DocumentType.DriverTerminalGiveout:
						return "Выдача терминала водителю " +
							   $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} со склада {Warehouse}";
					case DocumentType.DriverTerminalReturn:
						return "Возврат терминала водителем " +
							   $"{PersonHelper.PersonNameWithInitials(DriverSurname, DriverName, DriverPatronymic)} на склад {Warehouse}";
					default:
						return string.Empty;
				}
			}
		}

		public string Counterparty { get; set; }

		public int OrderId { get; set; }

		public string Warehouse { get; set; }

		public string SecondWarehouse { get; set; }

		public decimal Amount { get; set; }

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

		private string GetTitle(Type type) => type.GetAttribute<AppellativeAttribute>(true)?.Nominative;
	}
}
