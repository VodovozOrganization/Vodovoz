using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "доступные БД для пользовательской роли",
		Nominative = "доступная БД для пользовательской роли")]
	public class AvailableDatabase : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<UserRole> UserRoles { get; set; } = new List<UserRole>();
		public virtual IList<PrivilegeName> UnavailableForPrivilegeNames { get; set; } = new List<PrivilegeName>();
	}
}
