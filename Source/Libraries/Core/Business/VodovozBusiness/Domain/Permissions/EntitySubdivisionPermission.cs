using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace Vodovoz.Domain.Permissions
{
	public abstract class EntitySubdivisionPermission : EntityPermissionBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}
	}
}
