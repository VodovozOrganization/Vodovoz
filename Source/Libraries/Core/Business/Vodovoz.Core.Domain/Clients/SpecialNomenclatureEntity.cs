using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Специальная номенклатура
	/// </summary>
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "специальные номенклатуры",
			Nominative = "специальная номенклатура")]
	public class SpecialNomenclatureEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private NomenclatureEntity _nomenclature;
		private int _specialId;
		private CounterpartyEntity _counterparty;

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
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Код ТМЦ
		/// </summary>
		[Display(Name = "Код ТМЦ")]
		public virtual int SpecialId
		{
			get => _specialId;
			set => SetField(ref _specialId, value);
		}

		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

	}
}
