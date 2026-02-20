using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentReturned : SelfDeliveryDocumentReturnedEntity
	{
		private SelfDeliveryDocument _document;
		private Nomenclature _nomenclature;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;

		decimal _amountUnloaded;

		/// <summary>
		/// Документ самовывоза
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
		/// Операция передвижения товаров по складу (объемный учет)
		/// </summary>
		[Display(Name = "Операция передвижения товаров по складу (объемный учет)")]
		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation
		{
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}

		#region Не сохраняемые

		public virtual string Title
		{
			get
			{
				return string.Format(
					"{0} - {1}",
					GoodsAccountingOperation.Nomenclature.Name,
					GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount)
				);
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, CounterpartyEntity counterparty, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				Amount = Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
			};

			CounterpartyMovementOperation = new CounterpartyMovementOperation
			{
				WriteoffCounterparty = counterparty,
				Amount = Amount,
				OperationTime = time,
				Nomenclature = Nomenclature,
			};
		}

		public virtual void UpdateOperation(Warehouse warehouse, CounterpartyEntity counterparty)
		{
			GoodsAccountingOperation.Warehouse = warehouse;
			GoodsAccountingOperation.Amount = Amount;

			CounterpartyMovementOperation.WriteoffCounterparty = counterparty;
			CounterpartyMovementOperation.Amount = Amount;
		}

		#endregion
	}
}

