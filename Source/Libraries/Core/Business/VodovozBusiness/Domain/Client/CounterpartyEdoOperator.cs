using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "Оператор ЭДО контрагента",
		NominativePlural = "Операторы ЭДО контрагента"
	)]
	public class CounterpartyEdoOperator : PropertyChangedBase, IDomainObject
	{
		private Counterparty _counterparty;
		private EdoOperator _edoOperator;
		private string _personalAccountIdInEdo;
		public virtual int Id { get; set; }

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public virtual EdoOperator EdoOperator
		{
			get => _edoOperator;
			set => SetField(ref _edoOperator, value);
		}

		public virtual string PersonalAccountIdInEdo
		{
			get => _personalAccountIdInEdo;
			set => SetField(ref _personalAccountIdInEdo, value);
		}
		public virtual string Title => $"{EdoOperator?.Name} ({PersonalAccountIdInEdo})";
	}
}
