using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
		private bool _isHiddenFromPublicAccess;

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

		[Display(Name = "Скрыта из общего доступа")]
		public virtual bool IsHiddenFromPublicAccess
		{
			get => _isHiddenFromPublicAccess;
			set => SetField(ref _isHiddenFromPublicAccess, value);
		}

		public virtual IEnumerable<FinancialCategoriesGroup> GetAllLevelsSubGroups(IUnitOfWork unitOfWork, int parentId)
		{
			foreach(var childGroup in GetSubGroupsByParentId(unitOfWork, parentId))
			{
				yield return childGroup;

				foreach(var nextLevelChildGroup in GetAllLevelsSubGroups(unitOfWork, childGroup.Id))
				{
					yield return nextLevelChildGroup;
				}
			}
		}

		private IQueryable<FinancialCategoriesGroup> GetSubGroupsByParentId(IUnitOfWork unitOfWork, int parentId) => 
			unitOfWork.GetAll<FinancialCategoriesGroup>().Where(g => g.ParentId == parentId);

		public virtual List<FinancialIncomeCategory> GetFinancialIncomeSubCategories(IUnitOfWork unitOfWork, IEnumerable<int> parentIds)
		{
			var categories = unitOfWork.GetAll<FinancialIncomeCategory>()
				.Where(g => g.ParentId != null && parentIds.Contains(g.ParentId.Value))
				.ToList();

			return categories;
		}

		public virtual List<FinancialExpenseCategory> GetFinancialExpenseSubCategories(IUnitOfWork unitOfWork, IEnumerable<int> parentIds)
		{
			var categories = unitOfWork.GetAll<FinancialExpenseCategory>()
				.Where(g => g.ParentId != null && parentIds.Contains(g.ParentId.Value))
				.ToList();

			return categories;
		}

	}
}
