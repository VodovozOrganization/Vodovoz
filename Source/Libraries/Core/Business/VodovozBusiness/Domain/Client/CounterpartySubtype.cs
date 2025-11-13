using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Подтип контрагента",
		NominativePlural = "Подтипы контрагентов",
		GenitivePlural = "Подтипов контрагентов")]
	public class CounterpartySubtype : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
