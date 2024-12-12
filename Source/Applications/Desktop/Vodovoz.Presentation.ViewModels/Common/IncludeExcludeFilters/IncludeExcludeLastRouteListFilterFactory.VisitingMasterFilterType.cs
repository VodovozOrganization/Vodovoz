using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory
	{
		/// <summary>
		/// Фильтры по выездному мастеру
		/// </summary>

		[Appellative(Gender = GrammaticalGender.Masculine,
			Nominative = "выездной мастер",
			NominativePlural = "выездной мастер"
		)]
		public enum VisitingMasterFilterType
		{
			/// <summary>
			/// В отчёт попадает выездные мастера
			/// </summary>
			[Display(Name = "Включая")]
			IncludeVisitingMaster,

			/// <summary>
			/// Исключение из отчёта выездных мастеров
			/// </summary>
			[Display(Name = "Исключая")]
			ExcludeVisitingMaster,

			/// <summary>
			/// В отчёте только выездные мастера
			/// </summary>
			[Display(Name = "Только")]
			OnlyVisitingMaster
		}
	}
}
