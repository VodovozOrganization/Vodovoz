using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "премии сотрудникам",
		Nominative = "премия сотрудникам")]
	[EntityPermission]

	public class Premium : PremiumBase
	{

	}
}
