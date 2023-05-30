using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "статьи расхода",
		Nominative = "статья расхода",
		Accusative = "статью расхода")]
	[EntityPermission]
	[HistoryTrace]
	public class FinancialExpenseCategory : PropertyChangedBase, IDomainObject
	{
		private int? _parentId;
		private string _title;
		private bool _isArchive;
		private TargetDocument _targetDocument;
		private int? _subdivisionId;
		private string _numbering;

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

		public virtual TargetDocument TargetDocument
		{
			get => _targetDocument;
			set => SetField(ref _targetDocument, value);
		}
		
		public virtual int? SubdivisionId
		{
			get => _subdivisionId;
			set => SetField(ref _subdivisionId, value);
		}

		[MaxLength(150)]
		public virtual string Numbering
		{
			get => _numbering;
			set => SetField(ref _numbering, value);
		}

		public virtual GroupType GroupType { get; set; } = GroupType.Category;

		public virtual FinancialSubType FinancialSubtype { get; set; } = FinancialSubType.Expense;
	}
}
