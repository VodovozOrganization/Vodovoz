using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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

		private bool _isWidgetVisible;
		private CarInsurance _entity;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Counterparty _insurer;
		private string _insuranceNumber;
		private DialogViewModelBase _parentDialog;

		public CarInsuranceVersionEditingViewModel(
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(nameof(CarInsuranceVersionEditingViewModel));

			SaveInsuranceCommand = new DelegateCommand(SaveInsurance, () => CanSaveInsurance);
			CancelEditingInsuranceCommand = new DelegateCommand(CancelEditingInsurance);
		}

		public DelegateCommand SaveInsuranceCommand { get; }
		public DelegateCommand CancelEditingInsuranceCommand { get; }
		public IUnitOfWork UnitOfWork { get; }

		public bool IsWidgetVisible
		{
			get => _isWidgetVisible;
			set => SetField(ref _isWidgetVisible, value);
		}

		[PropertyChangedAlso(nameof(CanSaveInsurance))]
		public CarInsurance Entity
		{
			get => _entity;
			set
			{
				if(_entity != null)
				{
					throw new InvalidOperationException("Страховка авто уже в процессе изменения");
				}

				SetField(ref _entity, value);
				IsWidgetVisible = true;
			}
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
				if(_parentDialog != null)
				{
					return;
				}

				SetField(ref _parentDialog, value);
			}
		}
		public IEntityEntryViewModel InsurerEntryViewModel { get; private set; }

		public bool CanSaveInsurance =>
			Entity is null
			|| Entity.Car is null
			|| StartDate == null
			|| EndDate == null
			|| Insurer is null
			|| string.IsNullOrWhiteSpace(InsuranceNumber);

		private void SaveInsurance()
		{
			if(CanSaveInsurance)
			{
				return;
			}

			var carInsurance = new CarInsurance
			{
				Car = Entity.Car,
				StartDate = StartDate.Value,
				EndDate = EndDate.Value,
				Insurer = Insurer,
				InsuranceNumber = InsuranceNumber,
				InsuranceType = Entity.InsuranceType
			};

			if(!_commonServices.ValidationService.Validate(carInsurance))
			{
				return;
			}

			Entity.StartDate = StartDate.Value;
			Entity.EndDate = EndDate.Value;
			Entity.Insurer = Insurer;
			Entity.InsuranceNumber = InsuranceNumber;

			ClearPropertiesAndHideWidget();
		}

		private void CancelEditingInsurance()
		{
			ClearPropertiesAndHideWidget();
		}

		private void ClearPropertiesAndHideWidget()
		{
			Entity = null;
			StartDate = null;
			EndDate = null;
			Insurer = null;
			InsuranceNumber = string.Empty;

			IsWidgetVisible = false;
		}
	}
}
