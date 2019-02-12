using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Sale
{
	[EntityPermission]
	public class GeographicGroup : BusinessObjectBase<ScheduleRestrictedDistrict>, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}
	}
}