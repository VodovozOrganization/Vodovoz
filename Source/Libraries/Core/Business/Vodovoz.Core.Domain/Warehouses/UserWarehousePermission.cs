using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Право на склад для пользователя.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "право на склад пользователя",
		AccusativePlural = "права на склад пользователей",
		Genitive = "права на склад пользователя",
		GenitivePlural = "прав складов пользователей",
		Nominative = "Право на склад пользователя",
		NominativePlural = "Права на склад пользователей",
		Prepositional = "праве на склад пользователя",
		PrepositionalPlural = "правах на склады пользователей")]
	[EntityPermission]
	[HistoryTrace]
	public class UserWarehousePermission : WarehousePermissionBase
	{
		private User _user;

		/// <summary>
		/// Пользователь, которому принадлежат права на склад
		/// </summary>
		[Display(Name = "Пользователь")]
		public virtual User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		public override PermissionType PermissionType => PermissionType.User;
	}
}
