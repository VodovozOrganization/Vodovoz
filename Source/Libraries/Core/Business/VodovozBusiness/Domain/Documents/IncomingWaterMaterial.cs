﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Warehouses.Operations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Строки производства (сырьё)",
		Nominative = "Строка производства (сырьё)")]
	[HistoryTrace]
	public class IncomingWaterMaterial : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual IncomingWater Document { get; set; }

		Nomenclature nomenclature;

		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField(ref nomenclature, value, () => Nomenclature);
				if(ConsumptionMaterialOperation != null && ConsumptionMaterialOperation.Nomenclature != nomenclature)
					ConsumptionMaterialOperation.Nomenclature = nomenclature;
			}
		}

		decimal? oneProductAmount;

		[Display(Name = "На один продукт")]
		public virtual decimal? OneProductAmount {
			get { return oneProductAmount; }
			set {
				SetField(ref oneProductAmount, value, () => OneProductAmount);
				if(oneProductAmount.HasValue && Document != null) {
					Amount = OneProductAmount.Value * Document.Amount;
				}
			}
		}

		//FIXME Костыль пока не научим Gtk.Binding работать с нулабле типами
		public virtual decimal OneProductAmountEdited {
			get { return OneProductAmount.HasValue ? OneProductAmount.Value : 0; }
			set { OneProductAmount = (value > 0) ? value : 0; }
		}

		decimal amount;

		[Display(Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
				if(ConsumptionMaterialOperation != null && ConsumptionMaterialOperation.Amount != amount)
					ConsumptionMaterialOperation.Amount = -amount;
			}
		}

		decimal amountOnSource = 10000000; //FIXME пока не реализуем способ загружать количество на складе на конкретный день

		[Display(Name = "Имеется на складе")]
		public virtual decimal AmountOnSource {
			get { return amountOnSource; }
			set {
				SetField(ref amountOnSource, value, () => AmountOnSource);
			}
		}

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";

		WarehouseBulkGoodsAccountingOperation consumptionMaterialOperation;

		public virtual WarehouseBulkGoodsAccountingOperation ConsumptionMaterialOperation {
			get { return consumptionMaterialOperation; }
			set { SetField(ref consumptionMaterialOperation, value, () => ConsumptionMaterialOperation); }
		}

		public virtual string Title {
			get {
				return String.Format("[{2}] {0} - {1}",
									 Nomenclature.Name,
									 Nomenclature.Unit.MakeAmountShortStr(Amount),
									 Document.Title);
			}
		}

		public IncomingWaterMaterial() { }

		#region Функции

		public virtual void CreateOperation(Warehouse warehouseSrc, DateTime time)
		{
			ConsumptionMaterialOperation = new WarehouseBulkGoodsAccountingOperation {
				Warehouse = warehouseSrc,
				Amount = -Amount,
				OperationTime = time,
				Nomenclature = Nomenclature
			};
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Amount < 1)
			{
				yield return new ValidationResult("Количество должно быть больше 1", new[] { nameof(Amount) });
			}
		}

		#endregion
	}
}
