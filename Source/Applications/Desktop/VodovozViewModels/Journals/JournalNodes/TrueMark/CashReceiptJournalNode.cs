using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class CashReceiptJournalNode : JournalNodeBase, IHierarchicalNode<CashReceiptJournalNode>
	{
		public override string Title => $"{ReceiptOrProductCodeId} {CreatedTime}".Trim();

		//Свойства для поддержки иерархии
		public int Id { get; set; }
		public int? ParentId { get; set; }
		public CashReceiptJournalNode Parent { get; set; }
		public IList<CashReceiptJournalNode> Children { get; set; }
		public CashReceiptNodeType NodeType { get; set; }

		//Код чека / Код маркировки товара
		public string ReceiptOrProductCodeId => Id == 0 ? "" : Id.ToString();

		//Id чека
		public string ReceiptDocId =>
			NodeType == CashReceiptNodeType.Receipt ? CashReceipt.GetDocumentId(OrderAndItemId, ReceiptInnerNumber) : "";

		//Создан
		public DateTime? Created { get; set; }
		public string CreatedTime => Created.HasValue ? Created.Value.ToString("dd.MM.yy HH:mm") : "";

		//Изменен
		public DateTime? Changed { get; set; }
		public string ChangedTime => Changed.HasValue ? Changed.Value.ToString("dd.MM.yy HH:mm") : "";

		//Статус
		public CashReceiptStatus? ReceiptStatus { get; set; }
		public string Status => ReceiptStatus.HasValue ? ReceiptStatus.GetEnumTitle() : "";

		//Сумма
		public decimal ReceiptSum { get; set; }

		//Код МЛ
		public int RouteListId { get; set; }
		public string RouteList => RouteListId == 0 ? "" : RouteListId.ToString();

		//Водитель
		public string DriverName { get; set; }
		public string DriverLastName { get; set; }
		public string DriverPatronimyc { get; set; }
		public string DriverFIO => PersonHelper.PersonNameWithInitials(DriverLastName, DriverName, DriverPatronimyc);

		//Код заказа / Код строки заказа
		public int OrderAndItemId { get; set; }
		public int? ReceiptInnerNumber { get; set; }

		//Статус фиск. док. / Исх. GTIN
		public FiscalDocumentStatus? FiscalDocStatus { get; set; }
		public string SourceGtin { get; set; }
		public string FiscalDocStatusOrSourceGtin
		{
			get
			{
				if(FiscalDocStatus.HasValue)
				{
					return FiscalDocStatus.GetEnumTitle();
				}

				return SourceGtin;
			}
		}

		//Номер фиск. док. / Исх. Сер. номер
		public bool IsUnscannedProductCode { get; set; }
		public bool IsDuplicateProductCode { get; set; }
		public long? FiscalDocNumber { get; set; }
		public string SourceCodeSerialNumber { get; set; }
		public string FiscalDocNumberOrSourceCodeInfo
		{
			get
			{
				if(FiscalDocNumber.HasValue)
				{
					return FiscalDocNumber.Value.ToString();
				}

				if(!string.IsNullOrWhiteSpace(SourceCodeSerialNumber))
				{
					return SourceCodeSerialNumber;
				}

				if(IsUnscannedProductCode)
				{
					return "Не был отсканирован";
				}

				if(IsDuplicateProductCode)
				{
					return "Дубликат";
				}

				return "";
			}
		}

		//Дата фиск. док. / Итог. GTIN
		public DateTime? FiscalDocDate { get; set; }
		public string ResultGtin { get; set; }
		public string FiscalDocDateOrResultGtin
		{
			get
			{
				if(FiscalDocDate.HasValue)
				{
					return FiscalDocDate.Value.ToString("dd.MM.yy HH:mm");
				}

				return ResultGtin;
			}
		}

		//Дата статуса фиск. док. / Итог. Сер. номер
		public DateTime? FiscalDocStatusDate { get; set; }
		public string ResultSerialnumber { get; set; }
		public string FiscalDocStatusDateOrResultSerialnumber
		{
			get
			{
				if(FiscalDocStatusDate.HasValue)
				{
					return FiscalDocStatusDate.Value.ToString("dd.MM.yy HH:mm");
				}

				return ResultSerialnumber;
			}
		}

		//Ручная отправка / Брак
		public bool IsManualSentOrIsDefectiveCode { get; set; }

		//Отправлен на
		public string Contact { get; set; }

		//Причина не отскан. бутылей
		public string UnscannedReason { get; set; }

		//Описание ошибки
		public string ErrorDescription { get; set; }
	}
}
