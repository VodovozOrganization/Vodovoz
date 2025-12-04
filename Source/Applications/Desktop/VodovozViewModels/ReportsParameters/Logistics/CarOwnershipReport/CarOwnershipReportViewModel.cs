using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Extensions;
using Vodovoz.Settings.Database.Logistics;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport
{
	public class CarOwnershipReportViewModel : ReportParametersViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly ICarEventSettings _carEventSettings;

		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private bool _isOneDayReportSelected;
		private bool _isPeriodReportSelected;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private IEnumerable<CarOwnType> _selectedCarOwnTypes;
		private DateTime? _dateInOneDayReport;
		private DateTime? _startDateInPeriodReport;
		private DateTime? _endDateInPeriodReport;

		private const string _oneDayReportIdentifier = "Logistic.CarOwnershipOneDayReport";
		private const string _periodReportIdentifier = "Logistic.CarOwnershipPeriodReport";

		public CarOwnershipReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			ICommonServices commonServices,
			IReportInfoFactory reportInfoFactory,
			ICarEventSettings carEventSettings
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));

			Title = "Принадлежность ТС";

			_selectedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_selectedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			IsOneDayReportSelected = true;

			IsUserHasAccessToCarOwnershipReport = 
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.HasAccessToCarOwnershipReport);
		}

		#region Properties
		protected override Dictionary<string, object> Parameters => _parameters;
		public DelegateCommand GenerateReportCommand { get; }
		public Type PeriodReportModes { get; }
		public bool IsUserHasAccessToCarOwnershipReport { get; }

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse;
			set => SetField(ref _selectedCarTypesOfUse, value);
		}

		public IEnumerable<CarOwnType> SelectedCarOwnTypes
		{
			get => _selectedCarOwnTypes;
			set => SetField(ref _selectedCarOwnTypes, value);
		}

		public bool IsOneDayReportSelected
		{
			get => _isOneDayReportSelected;
			set
			{
				if(SetField(ref _isOneDayReportSelected, value)
					&& _isOneDayReportSelected)
				{
					IsPeriodReportSelected = false;
				}
			}
		}

		public bool IsPeriodReportSelected
		{
			get => _isPeriodReportSelected;
			set
			{
				if(SetField(ref _isPeriodReportSelected, value)
					&& _isPeriodReportSelected)
				{
					IsOneDayReportSelected = false;
				}
			}
		}

		public DateTime? DateInOneDayReport
		{
			get => _dateInOneDayReport;
			set => SetField(ref _dateInOneDayReport, value);
		}

		public DateTime? StartDateInPeriodReport
		{
			get => _startDateInPeriodReport;
			set => SetField(ref _startDateInPeriodReport, value);
		}

		public DateTime? EndDateInPeriodReport
		{
			get => _endDateInPeriodReport;
			set => SetField(ref _endDateInPeriodReport, value);
		}

		#endregion Properties

		private void GenerateReport()
		{
			if(!IsReportFiltersSettingsValid())
			{
				return;
			}

			SetReportParameters();

			LoadReport();
		}

		private void SetReportParameters()
		{
			var selectedCarTypeOfUse = string.Join(", ", SelectedCarTypesOfUse.Select(s => s.GetEnumDisplayName()));
			var selectedCarOwnType = string.Join(", ", SelectedCarOwnTypes.Select(s => s.GetEnumDisplayName()));

			var filtersText = 
				$"Выбранные фильтры:" +
				$"\nТип авто: {selectedCarTypeOfUse}" +
				$"\nПринадлежность автомобиля: {selectedCarOwnType}";

			_parameters = new Dictionary<string, object>
			{
				{ "car_type_of_use", SelectedCarTypesOfUse.ToArray() },
				{ "car_own_type", SelectedCarOwnTypes.ToArray() },
				{ "filters_text", filtersText },
				{ "excluded_car_ids", _carEventSettings.CarsExcludedFromReportsIds }
			};

			if(IsOneDayReportSelected)
			{
				Identifier = _oneDayReportIdentifier;

				_parameters.Add("date", DateInOneDayReport.Value.Date);

				return;
			}

			if(IsPeriodReportSelected)
			{
				Identifier = _periodReportIdentifier;

				_parameters.Add("start_date", StartDateInPeriodReport.Value.Date.ToString(DateTimeFormats.QueryDateTimeFormat));
				_parameters.Add("end_date", EndDateInPeriodReport.Value.Date.ToString(DateTimeFormats.QueryDateTimeFormat));

				return;
			}

			throw new InvalidOperationException("Ошибка выбора типа отчета");
		}

		private bool IsReportFiltersSettingsValid()
		{
			if(IsOneDayReportSelected == IsPeriodReportSelected)
			{
				ShowErrorMessage("Ошибка выбора типа отчета");

				return false;
			}

			if(SelectedCarTypesOfUse.Count() == 0)
			{
				ShowErrorMessage("Не выбран тип ТС");

				return false;
			}

			if(SelectedCarOwnTypes.Count() == 0)
			{
				ShowErrorMessage("Не выбрана принадлежность ТС");

				return false;
			}

			if(IsOneDayReportSelected)
			{
				if(DateInOneDayReport is null)
				{
					ShowErrorMessage("Не установлена дата");

					return false;
				}

				if(DateInOneDayReport.Value > DateTime.Today)
				{
					ShowErrorMessage("Нельзя устанавливать дату более текущей");

					return false;
				}
			}

			if(IsPeriodReportSelected)
			{
				if(StartDateInPeriodReport == null
					|| EndDateInPeriodReport == null)
				{
					ShowErrorMessage("Не установлен период");

					return false;
				}
			}

			return true;
		}

		private void ShowErrorMessage(string message)
		{
			_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						message);
		}
	}
}
