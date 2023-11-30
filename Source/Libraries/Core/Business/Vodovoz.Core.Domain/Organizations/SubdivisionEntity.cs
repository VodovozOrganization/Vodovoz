using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение",
		GenitivePlural = "подразделений")]
	[EntityPermission]
	[HistoryTrace]
	public class SubdivisionEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private bool _pacsTimeManagementEnabled;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Контроль времени по СКУД")]
		public virtual bool PacsTimeManagementEnabled
		{
			get => _pacsTimeManagementEnabled;
			set => SetField(ref _pacsTimeManagementEnabled, value);
		}
	}
}
