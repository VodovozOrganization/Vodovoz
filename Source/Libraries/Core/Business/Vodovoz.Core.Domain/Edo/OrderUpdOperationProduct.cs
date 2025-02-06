using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderUpdOperationProduct : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _nomenclatureId;
		private string _nomenclatureName;
		private string _unitCode;
		private string _measurementUnitName;
		private int _count;
		private decimal _itemPrice;
		private decimal _includeVat;
		private decimal _vat;
		private decimal _itemDiscount;
		private decimal _itemDiscountMoney;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Код номенклатуры
		/// </summary>
		[Display(Name = "Код номенклатуры")]
		public int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}

		/// <summary>
		/// Название номенклатуры
		/// </summary>
		[Display(Name = "Название номенклатуры")]
		public string NomenclatureName
		{
			get => _nomenclatureName;
			set => SetField(ref _nomenclatureName, value);
		}

		/// <summary>
		/// Код единицы измерения
		/// </summary>
		[Display(Name = "Код единицы измерения")]
		public string UnitCode
		{
			get => _unitCode;
			set => SetField(ref _unitCode, value);
		}

		/// <summary>
		/// Название единицы измерения
		/// </summary>
		[Display(Name = "Название единицы измерения")]
		public string MeasurementUnitName
		{
			get => _measurementUnitName;
			set => SetField(ref _measurementUnitName, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public int Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}

		/// <summary>
		/// Цена за единицу
		/// </summary>
		[Display(Name = "Цена за единицу")]
		public decimal ItemPrice
		{
			get => _itemPrice;
			set => SetField(ref _itemPrice, value);
		}

		/// <summary>
		/// Включая НДС
		/// </summary>
		[Display(Name = "Включая НДС")]
		public decimal IncludeVat
		{
			get => _includeVat;
			set => SetField(ref _includeVat, value);
		}

		/// <summary>
		/// НДС
		/// </summary>
		[Display(Name = "НДС")]
		public decimal Vat
		{
			get => _vat;
			set => SetField(ref _vat, value);
		}

		/// <summary>
		/// Скидка на товар
		/// </summary>
		[Display(Name = "Скидка на товар")]
		public decimal ItemDiscount
		{
			get => _itemDiscount;
			set => SetField(ref _itemDiscount, value);
		}

		/// <summary>
		/// Сумма скидки на товар
		/// </summary>
		[Display(Name = "Сумма скидки на товар")]
		public decimal ItemDiscountMoney
		{
			get => _itemDiscountMoney;
			set => SetField(ref _itemDiscountMoney, value);
		}
	}
}
