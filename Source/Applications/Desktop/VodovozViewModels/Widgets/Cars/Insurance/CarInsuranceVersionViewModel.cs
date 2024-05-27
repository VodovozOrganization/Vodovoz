using QS.Commands;
using QS.DomainModel.Entity;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceVersionViewModel : EntityWidgetViewModelBase<Car>
	{
		private readonly ICarInsuranceVersionService _carInsuranceVersionService;
		private CarInsuranceType? _insuranceType;
		private CarInsurance _selectedCarInsurance;
		private bool _isInsuranceNotRelevantForCar;

		public CarInsuranceVersionViewModel(
			Car entity,
			ICommonServices commonServices,
			ICarInsuranceVersionService carInsuranceVersionService)
			: base(entity, commonServices)
		{
			AddCarInsuranceCommand = new DelegateCommand(AddCarInsurance, () => CanAddCarInsurance);
			EditCarInsuranceCommand = new DelegateCommand(EditCarInsurance, () => CanEditCarInsurance);
			_carInsuranceVersionService = carInsuranceVersionService ?? throw new System.ArgumentNullException(nameof(carInsuranceVersionService));
		}

		public DelegateCommand AddCarInsuranceCommand { get; }
		public DelegateCommand EditCarInsuranceCommand { get; }

		[PropertyChangedAlso(nameof(CanAddCarInsurance))]
		public CarInsuranceType? InsuranceType
		{
			get => _insuranceType;
			set
			{
				if(_insuranceType != null)
				{
					return;
				}

				SetField(ref _insuranceType, value);
			}
		}

		[PropertyChangedAlso(nameof(CanEditCarInsurance))]
		public CarInsurance SelectedCarInsurance
		{
			get => _selectedCarInsurance;
			set => SetField(ref _selectedCarInsurance, value);
		}

		public bool IsInsuranceNotRelevantForCar
		{
			get => _isInsuranceNotRelevantForCar;
			set => SetField(ref _isInsuranceNotRelevantForCar, value);
		}

		public IList<CarInsurance> Insurances =>
			Entity.CarInsurances.Where(o => !InsuranceType.HasValue || o.InsuranceType == InsuranceType.Value)
			.ToList();

		public bool CanSetInsuranceNotRelevantForCar =>
			PermissionResult.CanUpdate
			&& InsuranceType.HasValue
			&& InsuranceType.Value == CarInsuranceType.Kasko;

		public bool CanAddCarInsurance => PermissionResult.CanUpdate && InsuranceType.HasValue;
		public bool CanEditCarInsurance => PermissionResult.CanUpdate && SelectedCarInsurance != null;

		private void AddCarInsurance()
		{
			if(!InsuranceType.HasValue)
			{
				return;
			}

			_carInsuranceVersionService.AddNewCarInsurance(InsuranceType.Value);
		}

		private void EditCarInsurance()
		{
			if(SelectedCarInsurance is null)
			{
				return;
			}

			_carInsuranceVersionService.EditCarInsurance(SelectedCarInsurance);
		}
	}
}
