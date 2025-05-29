using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей: сортировка касс",
		Nominative = "настройки пользователя: сортировка касс")]
	public class CashSubdivisionSortingSettings : PropertyChangedBase, IDomainObject
	{
		private int _sortingIndex;
		private UserSettings _userSettings;
		private Subdivision _cashSubdivision;

		public CashSubdivisionSortingSettings()
		{
		}

		public CashSubdivisionSortingSettings(int sortingIndex, UserSettings userSettings, Subdivision cashSubdivision)
		{
			_sortingIndex = sortingIndex;
			_userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
			_cashSubdivision = cashSubdivision ?? throw new ArgumentNullException(nameof(cashSubdivision));
		}

		#region Свойства

		public virtual int Id { get; }

		[Display(Name = "Порядок при сортировке")]
		public virtual int SortingIndex
		{
			get => _sortingIndex;
			set => SetField(ref _sortingIndex, value);
		}

		[Display(Name = "Настройки пользователя")]
		public virtual UserSettings UserSettings
		{
			get => _userSettings;
			set => SetField(ref _userSettings, value);
		}

		[Display(Name = "Подразделение кассы")]
		public virtual Subdivision CashSubdivision
		{
			get => _cashSubdivision;
			set => SetField(ref _cashSubdivision, value);
		}

		#endregion
	}
}
