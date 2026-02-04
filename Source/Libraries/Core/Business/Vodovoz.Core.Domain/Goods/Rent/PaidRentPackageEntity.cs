using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.BasicHandbooks;

namespace Vodovoz.Core.Domain.Goods.Rent
{
	public class PaidRentPackageEntity : BusinessObjectBase<PaidRentPackageEntity>, IDomainObject
	{
		private int _id;
		private string _name;
		private decimal _priceDaily;
		private NomenclatureEntity _rentServiceDaily;
		private decimal _priceMonthly;
		private NomenclatureEntity _rentServiceMonthly;
		private EquipmentKind _equipmentKind;
		private decimal _deposit;
		private NomenclatureEntity _depositService;

		public PaidRentPackageEntity()
		{
			Name = string.Empty;
		}

		#region Свойства

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Стоимость дня
		/// </summary>
		[Display(Name = "Стоимость дня")]
		public virtual decimal PriceDaily
		{
			get => _priceDaily;
			set => SetField(ref _priceDaily, value);
		}

		/// <summary>
		/// Услуга КПА
		/// </summary>
		[Display(Name = "Услуга КПА")]
		public virtual NomenclatureEntity RentServiceDaily
		{
			get => _rentServiceDaily;
			set => SetField(ref _rentServiceDaily, value);
		}

		/// <summary>
		/// Стоимость месяца
		/// </summary>
		[Display(Name = "Стоимость месяца")]
		public virtual decimal PriceMonthly
		{
			get => _priceMonthly;
			set => SetField(ref _priceMonthly, value);
		}

		/// <summary>
		/// Услуга ДПА
		/// </summary>
		[Display(Name = "Услуга ДПА")]
		public virtual NomenclatureEntity RentServiceMonthly
		{
			get => _rentServiceMonthly;
			set => SetField(ref _rentServiceMonthly, value);
		}

		/// <summary>
		/// Вид оборудования
		/// </summary>
		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		/// <summary>
		/// Залог
		/// </summary>
		[Display(Name = "Залог")]
		public virtual decimal Deposit
		{
			get => _deposit;
			set => SetField(ref _deposit, value);
		}

		/// <summary>
		/// Услуга залога
		/// </summary>
		[Display(Name = "Услуга залога")]
		public virtual NomenclatureEntity DepositService
		{
			get => _depositService;
			set => SetField(ref _depositService, value);
		}

		#endregion
	}
}
