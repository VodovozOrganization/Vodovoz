using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Permissions
{
	public class PresetSubdivisionPermission : PresetPermissionBase
	{
		public override PresetPermissionType PresetPermissionType => PresetPermissionType.subdivision;

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}
	}
}
