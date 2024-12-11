using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory
	{
		[Appellative(Gender = GrammaticalGender.Masculine,
			Nominative = "выездной мастер",
			NominativePlural = "выездной мастер"
		)]
		public enum VisitingMasterFilterType
		{
			[Display(Name = "Включая")]
			IncludeVisitingMaster,
			[Display(Name = "Исключая")]
			ExcludeVisitingMaster,
			[Display(Name = "Только")]
			OnlyVisitingMaster

		}
	}
}
