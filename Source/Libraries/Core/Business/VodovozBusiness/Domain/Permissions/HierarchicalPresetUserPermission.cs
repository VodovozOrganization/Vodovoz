using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "предустановленное право для пользователя",
		NominativePlural = "предустановленные права для пользователей"
	)]
	[HistoryTrace]
	public class HierarchicalPresetUserPermission : HierarchicalPresetPermissionBase
	{
		public override PresetPermissionType PresetPermissionType => PresetPermissionType.user;

		private User user;
		[Display(Name = "Пользователь")]
		public virtual User User {
			get => user;
			set => SetField(ref user, value, () => User);
		}
	}
}
