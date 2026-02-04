using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	/// <summary>
	/// Строка документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItem : SelfDeliveryDocumentItemEntity
	{
		private SelfDeliveryDocument _document;
		private Nomenclature _nomenclature;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;

		/// <summary>
		/// Документ самовывоза, к которому относится строка
		/// </summary>
		[Display(Name = "Документ самовывоза")]
		public virtual new SelfDeliveryDocument Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				SetField(ref _nomenclature, value);

				if(GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != _nomenclature)
				{
					GoodsAccountingOperation.Nomenclature = _nomenclature;
				}
			}
		}

		/// <summary>
		/// Операция передвижения товаров по складу
		/// </summary>
		[Display(Name = "Операция передвижения товаров по складу")]
		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation
		{
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}

		#region Функции

		public virtual string Title
		{
			get
			{
				string res = string.Empty;
				if(GoodsAccountingOperation != null)
				{
					res = string.Format(
						"[{2}] {0} - {1}",
						GoodsAccountingOperation.Nomenclature.Name,
						GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount),
						Document.Title
					);
				}
				else if(Nomenclature != null)
				{
					res = string.Format(
						"[{2}] {0} - {1}",
						Nomenclature.Name,
						Nomenclature.Unit.MakeAmountShortStr(Amount),
						Document.Title
					);
				}

				return res;
			}
		}

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				Amount = -Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
			};
		}

		public virtual void UpdateOperation(Warehouse warehouse)
		{
			GoodsAccountingOperation.Warehouse = warehouse;
			GoodsAccountingOperation.Amount = -Amount;
		}

		#endregion
	}
}
