using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Users.Settings
{
	/// <summary>
	/// Настройки сортировки касс для пользователя
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "настройки пользователя: сортировка касс",
		AccusativePlural = "настроек пользователя: сортировка касс",
		Genitive = "настроек пользователя: сортировка касс",
		GenitivePlural = "настроек пользователя: сортировка касс",
		NominativePlural = "настройки пользователей: сортировка касс",
		Nominative = "настройки пользователя: сортировка касс",
		Prepositional = "настройках пользователя: сортировка касс",
		PrepositionalPlural = "настройках пользователя: сортировка касс")]
	public class CashSubdivisionSortingSettings : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _sortingIndex;
		private UserSettings _userSettings;
		private SubdivisionEntity _cashSubdivision;

		/// <summary>
		/// Конструктор по умолчанию для NHibernate
		/// </summary>
		protected CashSubdivisionSortingSettings()
		{
			
		}

		
		public CashSubdivisionSortingSettings(int sortingIndex, UserSettings userSettings, SubdivisionEntity cashSubdivision)
		{
			_sortingIndex = sortingIndex;
			_userSettings = userSettings;
			_cashSubdivision = cashSubdivision;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Порядок при сортировке
		/// </summary>
		[Display(Name = "Порядок при сортировке")]
		public virtual int SortingIndex
		{
			get => _sortingIndex;
			set => SetField(ref _sortingIndex, value);
		}
		
		public virtual int? UserSettingsId => UserSettings?.Id;
		public virtual int? CashSubdivisionId => CashSubdivision?.Id;
		
		/// <summary>
		/// Настройки пользователя сущность
		/// </summary>
		[Display(Name = "Настройки пользователя")]
		public virtual UserSettings UserSettings
		{
			get => _userSettings;
			set => SetField(ref _userSettings, value);
		}

		/// <summary>
		/// Подразделение кассы сущность
		/// </summary>
		[Display(Name = "Подразделение кассы")]
		public virtual SubdivisionEntity CashSubdivision
		{
			get => _cashSubdivision;
			set => SetField(ref _cashSubdivision, value);
		}
	}
}
