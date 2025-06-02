using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Право на склад для подразделения.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "право на склад подразделения",
		AccusativePlural = "права на склад подразделений",
		Nominative = "право на склад подразделения",
		NominativePlural = "права на склад подразделения",
		Genitive = "права на склад подразделения",
		GenitivePlural = "прав складов подразделений",
		Prepositional = "праве на склад подразделения",
		PrepositionalPlural = "правах на склады подразделений")]
	[EntityPermission]
	[HistoryTrace]
	public class SubdivisionWarehousePermission : WarehousePermissionBase
	{
		private SubdivisionEntity _subdivision;

		/// <summary>
		/// Подразделение, которому принадлежат права на склад
		/// </summary>
		[Display(Name = "Подразделение")]
		public virtual SubdivisionEntity Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		public override PermissionType PermissionType => PermissionType.Subdivision;
	}
}
