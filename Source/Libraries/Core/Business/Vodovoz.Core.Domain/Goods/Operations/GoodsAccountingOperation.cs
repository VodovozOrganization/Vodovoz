using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Core.Domain.Goods.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по складу",
		Accusative = "операцию передвижения товаров по складу",
		Nominative = "операция передвижения товаров по складу")]
	public class GoodsAccountingOperation : OperationBase
	{
		private int? _nomenclatureId;
		private decimal _amount;

		#region Свойства

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual int? NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}

		public virtual decimal Amount
		{
			get => _amount;
			set => SetField (ref _amount, value);
		}

		#endregion
		
		public virtual OperationType OperationType { get; }

		#region Вычисляемые

		public virtual string Title
		{
			get
			{
				return null;
			}
		}

		#endregion
	}
}

