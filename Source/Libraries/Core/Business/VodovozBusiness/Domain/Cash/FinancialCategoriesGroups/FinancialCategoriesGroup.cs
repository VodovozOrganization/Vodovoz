using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы финансовых статей",
		Nominative = "группа финансовых статей",
		Accusative = "группу финансовых статей")]
	[EntityPermission]
	[HistoryTrace]
	public class FinancialCategoriesGroup : PropertyChangedBase, IDomainObject
	{
		private int? _parentId;
		private string _title;
		private bool _isArchive;
		private FinancialSubType _financialSubtype;

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

		public virtual FinancialSubType FinancialSubtype
		{
			get => _financialSubtype;
			set => SetField(ref _financialSubtype, value);
		}

		public virtual GroupType GroupType => GroupType.Group;
	}
}
