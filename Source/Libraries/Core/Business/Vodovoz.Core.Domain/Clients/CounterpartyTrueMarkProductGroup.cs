using TrueMark.Contracts;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Связь контрагента с подключенной товарной группой маркировки «Честный знак» (таблица counterparty_true_mark_product_groups).
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "товарная группа ЧЗ контрагента",
		NominativePlural = "товарные группы ЧЗ контрагента")]
	[HistoryTrace]
	public class CounterpartyTrueMarkProductGroup : PropertyChangedBase, IDomainObject
	{
		private CounterpartyEntity _counterparty;
		private TrueMarkProductGroup _productGroup;

		/// <summary>
		/// Идентификатор записи
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Товарная группа маркировки
		/// </summary>
		[Display(Name = "Товарная группа маркировки")]
		public virtual TrueMarkProductGroup ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}
	}
}
