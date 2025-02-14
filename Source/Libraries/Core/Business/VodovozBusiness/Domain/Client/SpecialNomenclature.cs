using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Специальная номенклатура
	/// </summary>
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "специальные номенклатуры",
			Nominative = "специальная номенклатура")]
	public class SpecialNomenclature : SpecialNomenclatureEntity
	{
		private Nomenclature _nomenclature;
		private Counterparty _counterparty;

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

	}
}
