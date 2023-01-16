using System;
using System.Collections.Generic;
using QS.Project.Filter;
using QS.Utilities.Enums;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarJournalFilterViewModel : FilterViewModelBase<CarJournalFilterViewModel>
	{
		private bool? _archive = false;
		private bool? _visitingMasters;
		private CarModel _carModel;
		private IList<CarTypeOfUse> _restrictedCarTypesOfUse;
		private IList<CarOwnType> _restrictedCarOwnTypes;

		private bool _canChangeCarModel;
		private bool _canChangeRestrictedCarOwnTypes;
		private bool _canChangeIsArchive;
		private bool _canChangeVisitingMasters;
		private bool _canChangeRestrictedCarTypesOfUse;

		public CarJournalFilterViewModel(ICarModelJournalFactory carModelJournalFactory)
		{
			CarModelJournalFactory = carModelJournalFactory ?? throw new ArgumentNullException(nameof(carModelJournalFactory));
			_restrictedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_restrictedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			SetFilterSensitivity(true);
		}

		public ICarModelJournalFactory CarModelJournalFactory { get; }

		public virtual bool? Archive
		{
			get => _archive;
			set => UpdateFilterField(ref _archive, value);
		}

		public bool? VisitingMasters
		{
			get => _visitingMasters;
			set => UpdateFilterField(ref _visitingMasters, value);
		}

		public CarModel CarModel
		{
			get => _carModel;
			set => UpdateFilterField(ref _carModel, value);
		}

		public IList<CarTypeOfUse> RestrictedCarTypesOfUse
		{
			get => _restrictedCarTypesOfUse;
			set => UpdateFilterField(ref _restrictedCarTypesOfUse, value);
		}

		public IList<CarOwnType> RestrictedCarOwnTypes
		{
			get => _restrictedCarOwnTypes;
			set => UpdateFilterField(ref _restrictedCarOwnTypes, value);
		}

		public bool CanChangeIsArchive
		{
			get => _canChangeIsArchive;
			set => SetField(ref _canChangeIsArchive, value);
		}

		public bool CanChangeVisitingMasters
		{
			get => _canChangeVisitingMasters;
			set => SetField(ref _canChangeVisitingMasters, value);
		}

		public bool CanChangeRestrictedCarTypesOfUse
		{
			get => _canChangeRestrictedCarTypesOfUse;
			set => SetField(ref _canChangeRestrictedCarTypesOfUse, value);
		}

		public bool CanChangeRestrictedCarOwnTypes
		{
			get => _canChangeRestrictedCarOwnTypes;
			set => SetField(ref _canChangeRestrictedCarOwnTypes, value);
		}

		public bool CanChangeCarModel
		{
			get => _canChangeCarModel;
			set => SetField(ref _canChangeCarModel, value);
		}

		public void SetFilterSensitivity(bool isSensitive)
		{
			CanChangeIsArchive = isSensitive;
			CanChangeVisitingMasters = isSensitive;
			CanChangeRestrictedCarTypesOfUse = isSensitive;
			CanChangeRestrictedCarOwnTypes = isSensitive;
			CanChangeCarModel = isSensitive;
		}
	}
}
