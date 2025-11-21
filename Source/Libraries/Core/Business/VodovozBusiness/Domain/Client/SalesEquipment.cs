using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "оборудование для продажи",
		Nominative = "оборудование для продажи")]
	public class SalesEquipment : SalesEquipmentEntity
	{
		private AdditionalAgreement _additionalAgreement;
		private Nomenclature _nomenclature;

		/// <summary>
		/// Соглашение
		/// </summary>
		[Display(Name = "Соглашение")]
		public virtual new AdditionalAgreement AdditionalAgreement
		{
			get => _additionalAgreement;
			set => SetField(ref _additionalAgreement, value, () => AdditionalAgreement);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
				get => _nomenclature;
				set => SetField(ref _nomenclature, value, () => Nomenclature);
		}
	}
}
