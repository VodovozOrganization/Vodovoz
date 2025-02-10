using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Товар в операции УПД по заказу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "товар в операции УПД по заказу",
		NominativePlural = "товары в операциях УПД по заказам"
	)]
	public class OrderUpdOperationProduct : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderUpdOperation _orderUpdOperation;
		private int _nomenclatureId;
		private string _oKEI;
		private string _nomenclatureName;
		private string _unitCode;
		private string _measurementUnitName;
		private decimal _count;
		private decimal _itemPrice;
		private decimal _includeVat;
		private decimal? _vat;
		private decimal _itemDiscount;
		private decimal _itemDiscountMoney;
		private bool _isService;

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
		/// Операция УПД по заказу
		/// </summary>
		[Display(Name = "Операция УПД по заказу")]
		public virtual OrderUpdOperation OrderUpdOperation
		{
			get => _orderUpdOperation;
			set => SetField(ref _orderUpdOperation, value);
		}

		/// <summary>
		/// Код номенклатуры
		/// </summary>
		[Display(Name = "Код номенклатуры")]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}

		/// <summary>
		/// Код ОКЕИ
		/// </summary>
		public virtual string OKEI
		{
			get => _oKEI;
			set => SetField(ref _oKEI, value);
		}

		/// <summary>
		/// Название номенклатуры
		/// </summary>
		[Display(Name = "Название номенклатуры")]
		public virtual string NomenclatureName
		{
			get => _nomenclatureName;
			set => SetField(ref _nomenclatureName, value);
		}

		/// <summary>
		/// Код единицы измерения
		/// </summary>
		[Display(Name = "Код единицы измерения")]
		public virtual string UnitCode
		{
			get => _unitCode;
			set => SetField(ref _unitCode, value);
		}

		/// <summary>
		/// Название единицы измерения
		/// </summary>
		[Display(Name = "Название единицы измерения")]
		public virtual string MeasurementUnitName
		{
			get => _measurementUnitName;
			set => SetField(ref _measurementUnitName, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}

		/// <summary>
		/// Цена за единицу
		/// </summary>
		[Display(Name = "Цена за единицу")]
		public virtual decimal ItemPrice
		{
			get => _itemPrice;
			set => SetField(ref _itemPrice, value);
		}

		/// <summary>
		/// Включая НДС
		/// </summary>
		[Display(Name = "Включая НДС")]
		public virtual decimal IncludeVat
		{
			get => _includeVat;
			set => SetField(ref _includeVat, value);
		}

		/// <summary>
		/// НДС
		/// </summary>
		[Display(Name = "НДС")]
		public virtual decimal? Vat
		{
			get => _vat;
			set => SetField(ref _vat, value);
		}

		/// <summary>
		/// Скидка на товар
		/// </summary>
		[Display(Name = "Скидка на товар")]
		public virtual decimal ItemDiscount
		{
			get => _itemDiscount;
			set => SetField(ref _itemDiscount, value);
		}

		/// <summary>
		/// Сумма скидки на товар
		/// </summary>
		[Display(Name = "Сумма скидки на товар")]
		public virtual decimal ItemDiscountMoney
		{
			get => _itemDiscountMoney;
			set => SetField(ref _itemDiscountMoney, value);
		}

		/// <summary>
		/// Строка заказа - услуга
		/// </summary>
		[Display(Name = "Строка заказа - услуга")]
		public virtual bool IsService
		{
			get => _isService;
			set => SetField(ref _isService, value);
		}
	}
}
