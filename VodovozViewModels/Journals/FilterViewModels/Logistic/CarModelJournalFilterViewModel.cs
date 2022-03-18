using QS.Project.Filter;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarModelJournalFilterViewModel : FilterViewModelBase<CarModelJournalFilterViewModel>
	{
		private bool? _archive;
		private CarOwnType? _restrictedCarOwnType;
		private bool _canChangeRestrictedCarOwnType;

		public CarModelJournalFilterViewModel()
		{
			_canChangeRestrictedCarOwnType = true;
		}

		public CarOwnType? RestrictedCarOwnType
		{
			get => _restrictedCarOwnType;
			set => UpdateFilterField(ref _restrictedCarOwnType, value);
		}

		public bool? Archive
		{
			get => _archive;
			set => UpdateFilterField(ref _archive, value);
		}

		public bool CanChangeRestrictedCarOwnType
		{
			get => _canChangeRestrictedCarOwnType;
			set => UpdateFilterField(ref _canChangeRestrictedCarOwnType, value);
		}
	}
}
