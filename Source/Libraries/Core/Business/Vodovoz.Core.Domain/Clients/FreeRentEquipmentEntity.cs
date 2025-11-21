using QS.DomainModel.Entity;
using QS.Utilities;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.Rent;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки БА соглашения",
		Nominative = "строка БА соглашения")]
	public class FreeRentEquipmentEntity : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private FreeRentPackageEntity _freeRentPackage;
		private EquipmentEntity _equipment;
		NomenclatureEntity _nomenclature;
		int _count;
		decimal _deposit;
		int _waterAmount;
		bool _isNew;

		/// <summary>
		/// Пакет бесплатной аренды
		/// </summary>
		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackageEntity FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		public virtual EquipmentEntity Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Count
		{
			get => _count;
			set => SetField(ref _count, value);
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
		/// Кол-во бутылей
		/// </summary>
		[Display(Name = "Кол-во бутылей")]
		public virtual int WaterAmount
		{
			get => _waterAmount;
			set => SetField(ref _waterAmount, value);
		}

		/// <summary>
		/// Новый
		/// </summary>
		[Display(Name = "Новый")]
		public virtual bool IsNew
		{
			get => _isNew;
			set => SetField(ref _isNew, value);
		}

		/// <summary>
		/// Имя пакета бесплатной аренды
		/// </summary>
		public virtual string PackageName => FreeRentPackage != null ? FreeRentPackage.Name : "";

		/// <summary>
		/// Выводит имя из оборудования если посерийный учет, иначе из номенклатуры 
		/// </summary>
		/// <value>The name of the equipment.</value>
		public virtual string EquipmentName
		{
			get
			{
				if(Equipment != null)
				{
					return Equipment.NomenclatureName;
				}
				else if(Nomenclature != null)
				{
					return Nomenclature.Name;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Серийный номер оборудования
		/// </summary>
		public virtual string EquipmentSerial => Equipment != null && Equipment.Nomenclature.IsSerial ? Equipment.Serial : "";

		/// <summary>
		/// Строковое представление залога
		/// </summary>
		public virtual string DepositString => CurrencyWorks.GetShortCurrencyString(Deposit);

		/// <summary>
		/// Строковое представление кол-ва бутылей
		/// </summary>
		public virtual string WaterAmountString => $"{WaterAmount} {NumberToTextRus.Case(WaterAmount, "бутыль", "бутыли", "бутылей")}";

		/// <summary>
		/// Заголовок 
		/// </summary>
		public virtual string Title => $"Бесплатная аренда {EquipmentName}";
	}
}
