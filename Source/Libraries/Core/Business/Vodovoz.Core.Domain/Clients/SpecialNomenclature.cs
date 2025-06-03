using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Специальная номенклатура клиента
	/// </summary>
	[Appellative(
			Gender = GrammaticalGender.Feminine,
			NominativePlural = "специальные номенклатуры клиентов",
			Nominative = "специальная номенклатура клиента")]
	[HistoryTrace]
	public class SpecialNomenclature : PropertyChangedBase, IDomainObject
	{
		private NomenclatureEntity _nomenclature;
		private int _specialId;
		private CounterpartyEntity _counterparty;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

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
		/// Код ТМЦ в системе покупателя
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
