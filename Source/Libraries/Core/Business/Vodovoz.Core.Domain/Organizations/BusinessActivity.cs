using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Направления деятельности",
		Nominative = "Направление деятельности",
		GenitivePlural = "Направлений деятельности")]
	[EntityPermission]
	[HistoryTrace]
	public class BusinessActivity : PropertyChangedBase, INamedDomainObject
	{
		private string _name;
		
		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
