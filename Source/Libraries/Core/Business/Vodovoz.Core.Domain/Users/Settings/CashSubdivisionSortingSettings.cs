using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

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
		private int? _userSettingsId;
		private int? _cashSubdivisionId;

		/// <summary>
		/// Конструктор по умолчанию для NHibernate
		/// </summary>
		protected CashSubdivisionSortingSettings()
		{
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="sortingIndex"></param>
		/// <param name="userSettingsId"></param>
		/// <param name="cashSubdivisionId"></param>
		public CashSubdivisionSortingSettings(int sortingIndex, int userSettingsId, int cashSubdivisionId)
		{
			_sortingIndex = sortingIndex;
			_userSettingsId = userSettingsId;
			_cashSubdivisionId = cashSubdivisionId;
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

		/// <summary>
		/// Настройки пользователя
		/// </summary>
		[Display(Name = "Настройки пользователя")]
		public virtual int? UserSettingsId
		{
			get => _userSettingsId;
			set => SetField(ref _userSettingsId, value);
		}

		/// <summary>
		/// Подразделение кассы
		/// </summary>
		[Display(Name = "Подразделение кассы")]
		public virtual int? CashSubdivisionId
		{
			get => _cashSubdivisionId;
			set => SetField(ref _cashSubdivisionId, value);
		}
	}
}
