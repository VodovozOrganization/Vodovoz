using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "право на документ для подразделения",
		NominativePlural = "права на документы для подразделения"
	)]
	[HistoryTrace]
	public class EntitySubdivisionOnlyPermission : EntitySubdivisionPermission
	{
		public override string ToString() => $"Право на документ [{TypeOfEntity?.CustomName}] для подразделения [{Subdivision?.Name}]";
	}
}
