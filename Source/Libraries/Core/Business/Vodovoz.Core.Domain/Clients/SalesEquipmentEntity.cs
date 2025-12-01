using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "оборудование для продажи",
		Nominative = "оборудование для продажи")]
	public class SalesEquipmentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private AdditionalAgreementEntity _additionalAgreement;
		private NomenclatureEntity _nomenclature;
		private decimal _price;
		private int _count;

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
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title => $"{Nomenclature.Name} - {Price}";

		/// <summary>
		/// Соглашение
		/// </summary>
		[Display(Name = "Соглашение")]
		public virtual AdditionalAgreementEntity AdditionalAgreement
		{
			get => _additionalAgreement; 
			set => SetField(ref _additionalAgreement, value);
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
	}
}
