using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods.Rent;

namespace VodovozBusiness.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Пакеты бесплатной аренды шаблонов автозаказов",
		Nominative = "Пакет бесплатной аренды шаблона автозаказа",
		Prepositional = "Пакете бесплатной аренды шаблона автозаказа",
		PrepositionalPlural = "Пакетах бесплатной аренды шаблонов автозаказов"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateFreeRentPackage : PropertyChangedBase, IDomainObject
	{
		private FreeRentPackage _freeRentPackage;
		private int _count;
		private decimal _price;
		private int _templateId;

		protected OnlineOrderTemplateFreeRentPackage() { }
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		[Display(Name = "Идентификатор шаблона")]
		public virtual int TemplateId
		{
			get => _templateId;
			set => SetField(ref _templateId, value);
		}
		
		/// <summary>
		/// Бесплатный пакет аренды
		/// </summary>
		[Display(Name = "Пакет аренды")]
		public virtual FreeRentPackage FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
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
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}
		
		[Display(Name = "Цена аренды из ДВ")]
		public virtual decimal FreeRentPackagePriceFromProgram { get; set; }

		public virtual decimal CurrentRawPrice => Count * Price;

		public static OnlineOrderTemplateFreeRentPackage Create(
			int templateId,
			int count,
			decimal price,
			FreeRentPackage rentPackage)
		{
			return new OnlineOrderTemplateFreeRentPackage
			{
				Count = count,
				Price = price,
				FreeRentPackage = rentPackage,
				TemplateId = templateId
			};
		}
	}
}
