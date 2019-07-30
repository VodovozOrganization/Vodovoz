using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "источники жалобы",
		Nominative = "источник жалобы"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintSource : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}
	}
}
