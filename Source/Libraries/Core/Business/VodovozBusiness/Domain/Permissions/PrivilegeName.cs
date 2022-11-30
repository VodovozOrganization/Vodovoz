using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "имена привелегий",
		Nominative = "имя привелегии")]
	public class PrivilegeName : IDomainObject
	{
		public virtual int Id { get; set; }
		
		[Display(Name = "Имя")]
		public virtual string Name { get; set; }
		
		[Display(Name = "Тип")]
		public virtual PrivilegeType PrivilegeType { get; set; }

		[Display(Name = "Недоступные БД")]
		public virtual IList<AvailableDatabase> UnavailableDatabases { get; set; } = new List<AvailableDatabase>();
	}
}
