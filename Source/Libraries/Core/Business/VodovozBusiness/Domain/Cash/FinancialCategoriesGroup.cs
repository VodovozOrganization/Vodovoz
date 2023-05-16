using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы финансовых статей",
		Nominative = "группа финансовых статей")]
	[EntityPermission]
	[HistoryTrace]
	public class FinancialCategoriesGroup : PropertyChangedBase, IDomainObject
	{
		private int? _parentId;
		private string _title;
		private bool _isArchive;

		public virtual int Id { get; }

		public virtual int? ParentId
		{
			get => _parentId;
			set => SetField(ref _parentId, value);
		}

		public virtual string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
	}
}
