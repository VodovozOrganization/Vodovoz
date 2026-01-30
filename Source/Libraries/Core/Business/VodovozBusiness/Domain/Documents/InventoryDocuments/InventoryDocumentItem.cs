using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки инвентаризации(объемный учет)",
		Nominative = "строка инвентаризации(объемный учет)")]
	[HistoryTrace]
	public abstract class InventoryDocumentItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Nomenclature _nomenclature;
		private decimal _amountInDb;
		private decimal _amountInFact;
		private string _comment;
		private Fine _fine;
		private GoodsAccountingOperation _goodsAccountingOperation;

		public virtual int Id { get; set; }

		public virtual InventoryDocument Document { get; set; }

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
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

		[Display (Name = "Количество по базе")]
		public virtual decimal AmountInDB
		{
			get => _amountInDb;
			set => SetField(ref _amountInDb, value);
		}

		[Display (Name = "Фактическое количество")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact
		{
			get => _amountInFact;
			set => SetField(ref _amountInFact, value);
		}

		[Display (Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display (Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField(ref _fine, value);
		}

		public virtual GoodsAccountingOperation GoodsAccountingOperation
		{
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}

		public abstract InventoryDocumentType Type { get; }

		#region Расчетные

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Difference <= 0 ? Nomenclature.SumOfDamage * Math.Abs(Difference) : 0;

		public virtual string Title =>
			$"[{Document.Title}] {Nomenclature.Name} - {(Nomenclature.Unit == null ? "" : Nomenclature.Unit.MakeAmountShortStr(AmountInFact))}";

		public virtual decimal Difference => AmountInFact - AmountInDB;

		#endregion

		#region Функции

		protected virtual void FillOperation(DateTime time)
		{
			if(GoodsAccountingOperation is null)
			{
				throw new InvalidOperationException("Не создана операция движения товаров");
			}

			GoodsAccountingOperation.Amount = Difference;
			GoodsAccountingOperation.OperationTime = time;
			GoodsAccountingOperation.Nomenclature = Nomenclature;
		}

		public virtual void UpdateOperation(DateTime timeStamp)
		{
			if(GoodsAccountingOperation is null)
			{
				CreateOperation(timeStamp);
			}
			else
			{
				UpdateOperation();
			}
		}
		
		protected abstract void CreateOperation(DateTime timeStamp);

		protected virtual void UpdateOperation()
		{
			if(GoodsAccountingOperation is null)
			{
				throw new InvalidOperationException("Не создана операция движения товаров");
			}
			
			GoodsAccountingOperation.Amount = Difference;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Comment?.Length > 255)
			{
				yield return new ValidationResult(
					$"Превышена длина комментария для номенклатуры: {Nomenclature.Name} ({Comment.Length}/255)",
					new[] {nameof(Comment)});
			}
		}

		#endregion
	}
}

