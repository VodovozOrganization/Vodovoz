using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars
{
	public class CarVersionsViewModel : EntityWidgetViewModelBase<Car>
	{
		private DateTime? _selectedDate;
		private CarVersion _selectedCarVersion;
		private readonly ICarVersionsController _carVersionsController;

		public CarVersionsViewModel(Car entity, ICommonServices commonServices, ICarVersionsController carVersionsController)
			: base(entity, commonServices)
		{
			_carVersionsController = carVersionsController ?? throw new ArgumentNullException(nameof(carVersionsController));

			CanRead = PermissionResult.CanRead;
			CanCreate = PermissionResult.CanCreate && Entity.Id == 0
				|| commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_car_version");
			CanEdit = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_car_version_date");

			if(IsNewCar)
			{
				SelectedDate = DateTime.Now.Date;
			}
		}

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					OnPropertyChanged(nameof(CanAddNewVersion));
					OnPropertyChanged(nameof(CanChangeVersionDate));
				}
			}
		}

		public virtual CarVersion SelectedCarVersion
		{
			get => _selectedCarVersion;
			set
			{
				if(SetField(ref _selectedCarVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeVersionDate));
				}
			}
		}

		public virtual bool CanRead { get; }
		public virtual bool CanCreate { get; }
		public virtual bool CanEdit { get; }

		public bool CanAddNewVersion =>
			CanCreate
			&& SelectedDate.HasValue
			&& Entity.CarVersions.All(x => x.Id != 0)
			&& _carVersionsController.IsValidDateForNewCarVersion(SelectedDate.Value);

		public bool CanChangeVersionDate =>
			SelectedDate.HasValue
			&& SelectedCarVersion != null
			&& (CanEdit || SelectedCarVersion.Id == 0)
			&& _carVersionsController.IsValidDateForVersionStartDateChange(SelectedCarVersion, SelectedDate.Value);

		public void AddNewCarVersion()
		{
			if(SelectedDate == null)
			{
				return;
			}
			_carVersionsController.CreateAndAddVersion(SelectedDate);

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}

		public void ChangeVersionStartDate()
		{
			if(SelectedDate == null || SelectedCarVersion == null)
			{
				return;
			}
			_carVersionsController.ChangeVersionStartDate(SelectedCarVersion, SelectedDate.Value);

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}

		public IList<CarOwnType> GetAvailableCarOwnTypesForVersion(CarVersion version)
		{
			return _carVersionsController.GetAvailableCarOwnTypesForVersion(version);
		}

		public IList<RouteList> GetAllAffectedRouteLists(IUnitOfWork uow)
		{
			return _carVersionsController.GetAllAffectedRouteLists(uow);
		}

		public bool IsNewCar => Entity.Id == 0;
	}
}
