using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы финансовых статей",
		Nominative = "группа финансовых статей",
		Accusative = "группу финансовых статей",
		Genitive = "группы финансовых статей")]
	[EntityPermission]
	[HistoryTrace]
	public class FinancialCategoriesGroup : PropertyChangedBase, IDomainObject
	{
		private int? _parentId;
		private string _title;
		private bool _isArchive;
		private FinancialSubType _financialSubtype;
		private string _numbering;

		[Display(Name = "Код")]
		public virtual int Id { get; }

		[Display(Name = "Родительская группа")]
		[HistoryIdentifier(TargetType = typeof(FinancialCategoriesGroup))]
		public virtual int? ParentId
		{
			get => _parentId;
			set => SetField(ref _parentId, value);
		}

		[Display(Name = "Название")]
		public virtual string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Нумерация")]
		[MaxLength(150)]
		public virtual string Numbering
		{
			get => _numbering;
			set => SetField(ref _numbering, value);
		}

		[Display(Name = "Приход/расход")]
		public virtual FinancialSubType FinancialSubtype
		{
			get => _financialSubtype;
			set => SetField(ref _financialSubtype, value);
		}

		[Display(Name = "Тип группы")]
		public virtual GroupType GroupType => GroupType.Group;
	}
}
