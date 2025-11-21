using QS.DomainModel.Entity;
using QS.Utilities;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.Rent;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки ПА соглашения",
		Nominative = "строка ПА соглашения")]
	public class PaidRentEquipmentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private PaidRentPackageEntity _paidRentPackage;
		private EquipmentEntity _equipment;
		private NomenclatureEntity _nomenclature;
		private int _count;
		private decimal _price;
		private decimal _deposit;
		private bool _isNew;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value, () => Id);
		}

		/// <summary>
		/// Пакет платной аренды
		/// </summary>
		[Display(Name = "Пакет платной аренды")]
		public virtual PaidRentPackageEntity PaidRentPackage
		{
			get => _paidRentPackage;
			set => SetField(ref _paidRentPackage, value);
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
		/// Цена
		/// </summary>
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
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
		/// Является ли оборудование новым
		/// </summary>
		public virtual bool IsNew
		{
			get => _isNew;
			set => SetField(ref _isNew, value);
		}

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
		/// Имя пакета платной аренды
		/// </summary>
		public virtual string PackageName => PaidRentPackage != null ? PaidRentPackage.Name : "";

		/// <summary>
		/// Строковое представление цены
		/// </summary>
		public virtual string PriceString => CurrencyWorks.GetShortCurrencyString(Price);

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title => $"Платная аренда {Equipment?.NomenclatureName}";
	}
}
