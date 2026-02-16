using QS.Project.Filter;
using QS.Utilities.Enums;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarJournalFilterViewModel : FilterViewModelBase<CarJournalFilterViewModel>
	{
		private readonly ViewModelEEVMBuilder<CarModel> _carModelViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Organization> _organizationModelViewModelBuilder;
		private CarModel _carModel;
		private CarJournalViewModel _journal;

		private bool? _archive = false;
		private bool? _visitingMasters;
		private IList<CarTypeOfUse> _restrictedCarTypesOfUse;
		private IList<CarOwnType> _restrictedCarOwnTypes;
		private bool _canChangeCarModel;
		private bool _canChangeRestrictedCarOwnTypes;
		private bool _canChangeIsArchive;
		private bool _canChangeVisitingMasters;
		private bool _canChangeRestrictedCarTypesOfUse;
		private Organization _carOwner;
		private Counterparty _insurer;
		private bool _isOnlyCarsWithoutCarOwner;
		private bool _isOnlyCarsWithoutInsurer;
		private bool _isUsedInDelivery;
		private bool _isNotUsedInDelivery;
		private IEnumerable<CarTypeOfUse> _excludedCarTypesOfUse;
		private string _vinFilter;

		public CarJournalFilterViewModel(
			ViewModelEEVMBuilder<CarModel> carModelViewModelBuilder,
			ViewModelEEVMBuilder<Organization> organizationModelViewModelBuilder)
		{
			_restrictedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_restrictedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			SetFilterSensitivity(true);
			_carModelViewModelBuilder = carModelViewModelBuilder
				?? throw new ArgumentNullException(nameof(carModelViewModelBuilder));
			_organizationModelViewModelBuilder = organizationModelViewModelBuilder
				?? throw new ArgumentNullException(nameof(organizationModelViewModelBuilder));

			_isUsedInDelivery = true;
			_isNotUsedInDelivery = true;
		}

		public CarJournalViewModel Journal
		{
			get => _journal;
			set
			{
				if(_journal != null)
				{
					return;
				}

				SetField(ref _journal, value);

				CarModelViewModel = _carModelViewModelBuilder
					.SetViewModel(value)
					.SetUnitOfWork(value.UoW)
					.ForProperty(this, x => x.CarModel)
					.UseViewModelJournalAndAutocompleter<CarModelJournalViewModel, CarModelJournalFilterViewModel>(filter =>
					{
						filter.ExcludedCarTypesOfUse = ExcludedCarTypesOfUse;
					})
					.UseViewModelDialog<CarModelViewModel>()
					.Finish();

				OrganizationViewModel = _organizationModelViewModelBuilder
					.SetViewModel(value)
					.SetUnitOfWork(value.UoW)
					.ForProperty(this, x => x.CarOwner)
					.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
					.UseViewModelDialog<OrganizationViewModel>()
					.Finish();
			}
		}

		public IEntityEntryViewModel OrganizationViewModel { get; private set; }

		public IEntityEntryViewModel CarModelViewModel { get; private set; }

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

		public IEnumerable<CarTypeOfUse> ExcludedCarTypesOfUse
		{
			get => _excludedCarTypesOfUse;
			set => UpdateFilterField(ref _excludedCarTypesOfUse, value);
		}

		public IList<CarOwnType> RestrictedCarOwnTypes
		{
			get => _restrictedCarOwnTypes;
			set => UpdateFilterField(ref _restrictedCarOwnTypes, value);
		}

		public Organization CarOwner
		{
			get => _carOwner;
			set => UpdateFilterField(ref _carOwner, value);
		}

		public virtual Counterparty Insurer
		{
			get => _insurer;
			set => UpdateFilterField(ref _insurer, value);
		}

		public virtual bool IsOnlyCarsWithoutCarOwner
		{
			get => _isOnlyCarsWithoutCarOwner;
			set
			{
				UpdateFilterField(ref _isOnlyCarsWithoutCarOwner, value);

				if(_isOnlyCarsWithoutCarOwner)
				{
					CarOwner = null;
				}
			}
		}

		public virtual bool IsOnlyCarsWithoutInsurer
		{
			get => _isOnlyCarsWithoutInsurer;
			set
			{
				UpdateFilterField(ref _isOnlyCarsWithoutInsurer, value);

				if(_isOnlyCarsWithoutInsurer)
				{
					Insurer = null;
				}
			}
		}

		public bool IsUsedInDelivery
		{
			get => _isUsedInDelivery;
			set => UpdateFilterField(ref _isUsedInDelivery, value);
		}

		public bool IsNotUsedInDelivery
		{
			get => _isNotUsedInDelivery;
			set => UpdateFilterField(ref _isNotUsedInDelivery, value);
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

		public string VinFilter
		{
			get => _vinFilter;
			set => UpdateFilterField(ref _vinFilter, value);
		}

		public void SetFilterSensitivity(bool isSensitive)
		{
			CanChangeIsArchive = isSensitive;
			CanChangeVisitingMasters = isSensitive;
			CanChangeRestrictedCarTypesOfUse = isSensitive;
			CanChangeRestrictedCarOwnTypes = isSensitive;
			CanChangeCarModel = isSensitive;
		}

		public override void Dispose()
		{
			_journal = null;
			base.Dispose();
		}
	}
}
