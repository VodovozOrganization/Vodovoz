using System;
using System.Collections.Generic;
using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Filters.ViewModels
{
	public class CarJournalFilterViewModel : FilterViewModelBase<CarJournalFilterViewModel>, IJournalFilter
	{
		public CarJournalFilterViewModel()
		{
			visitingMasters = AllYesNo.All;
			raskat = AllYesNo.All;

			RestrictedCarTypesOfUse = new List<CarTypeOfUse>((CarTypeOfUse[])Enum.GetValues(typeof(CarTypeOfUse)));
			SetFilterSensitivity(true);
		}
		
		private bool includeArchive;
		public virtual bool IncludeArchive {
			get => includeArchive;
			set => UpdateFilterField(ref includeArchive, value);
		}
		
		private AllYesNo visitingMasters;
		public AllYesNo VisitingMasters {
			get => visitingMasters;
			set => UpdateFilterField(ref visitingMasters, value);
		}
		
		private AllYesNo raskat;
		public AllYesNo Raskat {
			get => raskat;
			set => UpdateFilterField(ref raskat, value);
		}
		
		private IList<CarTypeOfUse> restrictedCarTypesOfUse;
		public IList<CarTypeOfUse> RestrictedCarTypesOfUse {
			get => restrictedCarTypesOfUse;
			set => UpdateFilterField(ref restrictedCarTypesOfUse, value);
		}

		public bool CanChangeIsArchive { get; set; }
		public bool CanChangeVisitingMasters { get; set; }
		public bool CanChangeRaskat { get; set; }
		public bool CanChangeRestrictedCarTypesOfUse { get; set; }

		public void SetFilterSensitivity(bool isSensitive)
		{
			CanChangeRaskat = isSensitive;
			CanChangeIsArchive = isSensitive;
			CanChangeVisitingMasters = isSensitive;
			CanChangeRestrictedCarTypesOfUse = isSensitive;
		}
	}
}
