using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "предустановленное право для подразделения",
		NominativePlural = "предустановленные права для подразделений"
	)]
	[HistoryTrace]
	public class HierarchicalPresetSubdivisionPermission : HierarchicalPresetPermissionBase
	{
		[Display(Name = "Тип предустановленного права")]
		public override PresetPermissionType PresetPermissionType => PresetPermissionType.subdivision;

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		public override string ToString() => $"Предуст. право [{PermissionName}] для подразделения [{Subdivision?.Name}]";
	}
}
