using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.HistoryLog;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "особое право на документ для подразделения",
		NominativePlural = "особые права на документы для подразделения"
	)]
	[HistoryTrace]
	public class EntitySubdivisionPermissionExtended : EntityPermissionExtendedBase
	{
		public override PermissionExtendedType PermissionExtendedType {
			get => PermissionExtendedType.Subdivision;
			set { }
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		public override string ToString() => $"Особое право на документ [{TypeOfEntity?.CustomName}] для подразделения [{Subdivision?.Name}]";

		public virtual void CopySettingsFromPermissionExceptSubdivision(EntitySubdivisionPermissionExtended sourcePermission)
		{
			TypeOfEntity = sourcePermission.TypeOfEntity;
			PermissionId = sourcePermission.PermissionId;
			IsPermissionAvailable = sourcePermission.IsPermissionAvailable;
		}
	}
}
