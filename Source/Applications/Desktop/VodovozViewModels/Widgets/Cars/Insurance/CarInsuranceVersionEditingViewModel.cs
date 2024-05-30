using Autofac;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceVersionEditingViewModel : WidgetViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private CarInsurance _carInsurance;
		private CarInsuranceType? _insuranceType;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Counterparty _insurer;
		private string _insuranceNumber;
		private DialogViewModelBase _parentDialog;
		private bool _isWidgetVisible;

		public CarInsuranceVersionEditingViewModel(
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(CarInsuranceVersionEditingViewModel));

			SaveInsuranceCommand = new DelegateCommand(SaveInsurance, () => CanSaveInsurance);
			CancelEditingInsuranceCommand = new DelegateCommand(CancelEditingInsurance);
		}

		public event EventHandler<CarInsuranceEditingCompetedEventArgs> CarInsuranceEditingCompeted;
		public event EventHandler CarInsuranceEditingCancelled;

		public DelegateCommand SaveInsuranceCommand { get; }
		public DelegateCommand CancelEditingInsuranceCommand { get; }
		public IUnitOfWork UnitOfWork { get; }
		public INavigationManager NavigationManager { get; }
		public ILifetimeScope LifetimeScope { get; }
		public IEntityEntryViewModel InsurerEntryViewModel { get; private set; }

		public bool IsWidgetVisible
		{
			get => _isWidgetVisible;
			set => SetField(ref _isWidgetVisible, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public CarInsuranceType? InsuranceType
		{
			get => _insuranceType;
			set => SetField(ref _insuranceType, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public virtual Counterparty Insurer
		{
			get => _insurer;
			set => SetField(ref _insurer, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public virtual string InsuranceNumber
		{
			get => _insuranceNumber;
			set => SetField(ref _insuranceNumber, value);
		}

		public DialogViewModelBase ParentDialog
		{
			get => _parentDialog;
			set
			{
				if(!(_parentDialog is null))
				{
					return;
				}

				SetField(ref _parentDialog, value);
			}
		}

		public bool CanSaveInsurance =>
			InsuranceType.HasValue
			&& StartDate.HasValue
			&& EndDate.HasValue
			&& !(Insurer is null)
			&& !string.IsNullOrWhiteSpace(InsuranceNumber);

		private void SaveInsurance()
		{
			if(!CanSaveInsurance)
			{
				return;
			}

			var carInsurance = new CarInsurance
			{
				Car = _carInsurance.Car,
				StartDate = StartDate.Value,
				EndDate = EndDate.Value,
				Insurer = Insurer,
				InsuranceNumber = InsuranceNumber,
				InsuranceType = InsuranceType.Value
			};

			if(!_commonServices.ValidationService.Validate(carInsurance))
			{
				return;
			}

			_carInsurance.StartDate = StartDate.Value;
			_carInsurance.StartDate = StartDate.Value;
			_carInsurance.EndDate = EndDate.Value;
			_carInsurance.Insurer = Insurer;
			_carInsurance.InsuranceNumber = InsuranceNumber;
			_carInsurance.InsuranceType = InsuranceType.Value;

			CarInsuranceEditingCompeted?.Invoke(this, new CarInsuranceEditingCompetedEventArgs(_carInsurance));
			ClearWidgetProperties();
		}

		private void CancelEditingInsurance()
		{
			CarInsuranceEditingCancelled?.Invoke(this, EventArgs.Empty);
			ClearWidgetProperties();
		}

		public void SetWidgetProperties(CarInsurance insurance)
		{
			if(insurance is null)
			{
				return;
			}

			if(!(_carInsurance is null))
			{
				throw new InvalidOperationException("Страховка уже в процессе редактирования");
			}

			_carInsurance = insurance;
			InsuranceType = insurance.InsuranceType;

			if(insurance.StartDate != default)
			{
				StartDate = insurance.StartDate;
			}

			if(insurance.EndDate != default)
			{
				EndDate = insurance.EndDate;
			}

			Insurer = insurance.Insurer;
			InsuranceNumber = insurance.InsuranceNumber;
		}

		private void ClearWidgetProperties()
		{
			_carInsurance = null;
			InsuranceType = null;
			StartDate = null;
			EndDate = null;
			Insurer = null;
			InsuranceNumber = string.Empty;
		}
	}
}
