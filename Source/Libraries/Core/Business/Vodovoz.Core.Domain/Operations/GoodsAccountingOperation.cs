using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Operations
{
	/// <summary>
	/// Операция передвижения товаров по складу
	/// </summary>
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по складу",
		Accusative = "операцию передвижения товаров по складу",
		Nominative = "операция передвижения товаров по складу")]
	public class GoodsAccountingOperation : OperationBase
	{
		private NomenclatureEntity _nomenclature;
		private decimal _amount;

		#region Свойства

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField (ref _amount, value);
		}

		#endregion
		
		/// <summary>
		/// Тип операции
		/// </summary>
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

