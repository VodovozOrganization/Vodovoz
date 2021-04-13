using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;

namespace Vodovoz.Domain.Permissions
{
	[HistoryTrace]
	public class HierarchicalPresetSubdivisionPermission : HierarchicalPresetPermissionBase
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
