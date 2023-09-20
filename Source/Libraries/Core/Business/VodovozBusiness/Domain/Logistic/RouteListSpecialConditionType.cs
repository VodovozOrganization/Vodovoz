using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		GenitivePlural = "типов дополнительного условия МЛ",
		NominativePlural = "типы дополнительного условия МЛ",
		Nominative = "тип дополнительного условия МЛ")]
	public class RouteListSpecialConditionType
	{
		public int Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
	}
}
