using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения сервиса",
		Nominative = "доп. соглашение сервиса")]
	[EntityPermission]
	public class RepairAgreementEntity : AdditionalAgreementEntity
	{
	}
}
