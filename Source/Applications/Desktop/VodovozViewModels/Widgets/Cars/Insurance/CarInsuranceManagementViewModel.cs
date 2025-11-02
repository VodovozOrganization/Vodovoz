using QS.Dialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceManagementViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private Car _car;
		private DialogViewModelBase _parentDialog;

		public CarInsuranceManagementViewModel(
			ICommonServices commonServices,
			CarInsuranceVersionViewModel osagoInsuranceVersionViewModel,
			CarInsuranceVersionViewModel kaskoInsuranceVersionViewModel,
			CarInsuranceVersionEditingViewModel carInsuranceVersionEditingViewModel)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			OsagoInsuranceVersionViewModel = osagoInsuranceVersionViewModel ?? throw new ArgumentNullException(nameof(osagoInsuranceVersionViewModel));
			KaskoInsuranceVersionViewModel = kaskoInsuranceVersionViewModel ?? throw new ArgumentNullException(nameof(kaskoInsuranceVersionViewModel));
			CarInsuranceVersionEditingViewModel = carInsuranceVersionEditingViewModel ?? throw new ArgumentNullException(nameof(carInsuranceVersionEditingViewModel));
			_interactiveService = _commonServices.InteractiveService;

			OsagoInsuranceVersionViewModel.AddCarInsuranceClicked += OnAddCarInsuranceClicked;
			OsagoInsuranceVersionViewModel.EditCarInsurenceClicked += OnEditCarInsurenceClicked;

			KaskoInsuranceVersionViewModel.AddCarInsuranceClicked += OnAddCarInsuranceClicked;
			KaskoInsuranceVersionViewModel.EditCarInsurenceClicked += OnEditCarInsurenceClicked;
			KaskoInsuranceVersionViewModel.ChangeIsKaskoNotRelevantClicked += OnChangeIsKaskoNotRelevantClicked;

			CarInsuranceVersionEditingViewModel.CarInsuranceEditingCompeted += OnCarInsuranceEditingCompeted;
			CarInsuranceVersionEditingViewModel.CarInsuranceEditingCancelled += OnCarInsuranceEditingCancelled;
		}

		public CarInsuranceVersionViewModel OsagoInsuranceVersionViewModel { get; }
		public CarInsuranceVersionViewModel KaskoInsuranceVersionViewModel { get; }
		public CarInsuranceVersionEditingViewModel CarInsuranceVersionEditingViewModel { get; }

		public bool IsUserHasPermissionToEditCarEntity =>
			_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

		public Car Car
		{
			get => _car;
			private set
			{
				if(!(_car is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(Car)} уже установлено");
				}

				SetField(ref _car, value);
			}
		}

		public DialogViewModelBase ParentDialog
		{
			get => _parentDialog;
			private set
			{
				if(!(_parentDialog is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(ParentDialog)} уже установлено");
				}

				SetField(ref _parentDialog, value);
			}
		}

		private bool _isInsuranceEditingInProgress => CarInsuranceVersionEditingViewModel.IsWidgetVisible;

		private IList<CarInsurance> GetCarsInsurances(CarInsuranceType insuranceType) =>
			Car.CarInsurances
			.Where(ins => ins.InsuranceType == insuranceType)
			.ToList();

		private bool IsNewInsuranceAlreadyAdded(CarInsuranceType insuranceType) =>
			Car.CarInsurances
			.Where(ins => ins.InsuranceType == insuranceType)
			.Count(item => item.Id == 0) > 0;

		public void Initialize(Car car, DialogViewModelBase parentDialog)
		{
			Car = car ?? throw new ArgumentNullException(nameof(car));
			ParentDialog = parentDialog ?? throw new ArgumentNullException(nameof(parentDialog));

			InitializeOsagoCarInsuranceVersionViewModel();
			InitializeKaskoCarInsuranceVersionViewModel();
			InitializeCarInsuranceEditingViewModel();
		}

		private void InitializeOsagoCarInsuranceVersionViewModel()
		{
			OsagoInsuranceVersionViewModel.InsuranceType = CarInsuranceType.Osago;
			OsagoInsuranceVersionViewModel.Insurances = GetCarsInsurances(CarInsuranceType.Osago);
		}

		private void InitializeKaskoCarInsuranceVersionViewModel()
		{
			KaskoInsuranceVersionViewModel.InsuranceType = CarInsuranceType.Kasko;
			KaskoInsuranceVersionViewModel.Insurances = GetCarsInsurances(CarInsuranceType.Kasko);
			KaskoInsuranceVersionViewModel.IsInsuranceNotRelevantForCar = Car.IsKaskoInsuranceNotRelevant;
		}

		private void InitializeCarInsuranceEditingViewModel()
		{
			CarInsuranceVersionEditingViewModel.IsWidgetVisible = false;
			CarInsuranceVersionEditingViewModel.ParentDialog = ParentDialog;
		}

		private void RefreshOsagoAndKaskoLists()
		{
			OsagoInsuranceVersionViewModel.Insurances = GetCarsInsurances(CarInsuranceType.Osago);
			KaskoInsuranceVersionViewModel.Insurances = GetCarsInsurances(CarInsuranceType.Kasko);
		}

		private void EditCarInsurance(CarInsurance carInsurance)
		{
			if(_isInsuranceEditingInProgress)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В данный момент уже выполняется редактирование страховки.\n" +
					"Сохраните редактируемую страховку или отмените редактирование");

				return;
			}

			ShowEditInsuranceWidget();
			CarInsuranceVersionEditingViewModel.IsWidgetVisible = true;
			CarInsuranceVersionEditingViewModel.SetWidgetProperties(carInsurance);
		}

		private void SetIsKaskoNotRelevantValue(bool isKaskoNotRelevant)
		{
			Car.IsKaskoInsuranceNotRelevant = isKaskoNotRelevant;

			KaskoInsuranceVersionViewModel.IsInsuranceNotRelevantForCar = isKaskoNotRelevant;
		}

		private void OnAddCarInsuranceClicked(object sender, AddCarInsuranceEventArgs e)
		{
			var newInsuranceType = e.InsuranceType;

			if(IsNewInsuranceAlreadyAdded(newInsuranceType))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Новая страховка уже была добавлена. Сначала сохраните существующие изменения.");

				return;
			}

			var insurrance = new CarInsurance
			{
				Car = Car,
				InsuranceType = newInsuranceType
			};

			EditCarInsurance(insurrance);
		}

		private void OnEditCarInsurenceClicked(object sender, EditCarInsuranceEventArgs e)
		{
			EditCarInsurance(e.CarInsurance);
		}

		private void OnChangeIsKaskoNotRelevantClicked(object sender, ChangeIsKaskoNotRelevantClickedEventArgs e)
		{
			SetIsKaskoNotRelevantValue(e.IsKaskoNotRelevant);
		}

		private void OnCarInsuranceEditingCompeted(object sender, CarInsuranceEditingCompetedEventArgs e)
		{
			var editedInsurence = e.CarInsurance;

			if(editedInsurence.Id == 0)
			{
				Car.CarInsurances.Add(editedInsurence);
			}

			RefreshOsagoAndKaskoLists();
			HideEditInsuranceWidget();
		}

		private void OnCarInsuranceEditingCancelled(object sender, EventArgs e)
		{
			HideEditInsuranceWidget();
		}

		private void ShowEditInsuranceWidget()
		{
			CarInsuranceVersionEditingViewModel.IsWidgetVisible = true;
		}

		private void HideEditInsuranceWidget()
		{
			CarInsuranceVersionEditingViewModel.IsWidgetVisible = false;
		}

		public void Dispose()
		{
			if(!(OsagoInsuranceVersionViewModel is null))
			{
				OsagoInsuranceVersionViewModel.AddCarInsuranceClicked -= OnAddCarInsuranceClicked;
				OsagoInsuranceVersionViewModel.EditCarInsurenceClicked -= OnEditCarInsurenceClicked;
			}

			if(!(KaskoInsuranceVersionViewModel is null))
			{
				KaskoInsuranceVersionViewModel.AddCarInsuranceClicked -= OnAddCarInsuranceClicked;
				KaskoInsuranceVersionViewModel.EditCarInsurenceClicked -= OnEditCarInsurenceClicked;
				KaskoInsuranceVersionViewModel.ChangeIsKaskoNotRelevantClicked -= OnChangeIsKaskoNotRelevantClicked;
			}

			if(!(CarInsuranceVersionEditingViewModel is null))
			{
				CarInsuranceVersionEditingViewModel.CarInsuranceEditingCompeted -= OnCarInsuranceEditingCompeted;
				CarInsuranceVersionEditingViewModel.CarInsuranceEditingCancelled -= OnCarInsuranceEditingCancelled;
			}
		}
	}
}
