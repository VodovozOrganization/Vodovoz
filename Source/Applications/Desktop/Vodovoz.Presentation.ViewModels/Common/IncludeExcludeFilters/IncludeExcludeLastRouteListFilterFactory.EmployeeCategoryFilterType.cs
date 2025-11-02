using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory
	{
		/// <summary>
		/// Фильтр по категории сотрудников
		/// </summary>

		[Appellative(Gender = GrammaticalGender.Masculine,
			Nominative = "категория сотрудника",
			NominativePlural = "категории сотрудника"
		)]
		public enum EmployeeCategoryFilterType
		{
			/// <summary>
			/// Выезд мастера
			/// </summary>
			[Display(Name = "Выезд мастера")]
			VisitingMaster,

			/// <summary>
			/// Водитель
			/// </summary>
			[Display(Name = "Водитель")]
			Driver,

			/// <summary>
			/// Экспедитор
			/// </summary>
			[Display(Name = "Экспедитор")]
			Forwarder
		}
	}
}
