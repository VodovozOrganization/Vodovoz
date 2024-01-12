using QS.Commands;
using QS.Dialog;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport
{
	public class CarOwnershipReportViewModel : ReportParametersViewModelBase
	{
		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private bool _isOneDayReportSelected;
		private bool _isPeriodReportSelected;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private IEnumerable<CarOwnType> _selectedCarOwnTypes;
		private DateTime? _dateInOneDayReport;
		private DateTime? _startDateInPeriodReport;
		private DateTime? _endDateInPeriodReport;
		private CarOwnershipReportPeriodMode _selectedPeriodReportMode;

		private const string _oneDayReportIdentifier = "Logistic.CarOwnershipOneDayReport";
		private const string _periodReportIdentifier = "Logistic.CarOwnershipPeriodReport";
		private readonly ICommonServices _commonServices;

		public CarOwnershipReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			ICommonServices commonServices) : base(rdlViewerViewModel)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Title = "Количество ТС компании";

			_selectedCarTypesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>();
			_selectedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>();
			
			PeriodReportModes = typeof(CarOwnershipReportPeriodMode);
			GenerateReportCommand = new DelegateCommand(SetReportParametersAndLoadReport);

			IsOneDayReportSelected = true;
		}

		#region Properties
		protected override Dictionary<string, object> Parameters => _parameters;
		public DelegateCommand GenerateReportCommand { get; }
		public Type PeriodReportModes { get; }

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

		public CarOwnershipReportPeriodMode SelectedPeriodReportMode
		{
			get => _selectedPeriodReportMode;
			set => SetField(ref _selectedPeriodReportMode, value);
		}

		#endregion Properties

		private void SetReportParametersAndLoadReport()
		{
			if(!SetReportParameters())
			{
				return;
			}

			LoadReport();
		}

		private bool SetReportParameters()
		{
			if(!IsReportSettingsValid())
			{
				return false;
			}

			var selectedCarTypeOfUse = string.Join(", ", SelectedCarTypesOfUse.Select(s => s.GetEnumDisplayName()));
			var selectedCarOwnType = string.Join(", ", SelectedCarOwnTypes.Select(s => s.GetEnumDisplayName()));

			var filtersText = 
				$"Выбранные фильтры:" +
				$"\nТип авто: {selectedCarTypeOfUse}" +
				$"\nПринадлежность автомобиля: {selectedCarOwnType}";

			_parameters = new Dictionary<string, object>
			{
				{ "car_type_of_use", selectedCarTypeOfUse },
				{ "car_own_type", selectedCarOwnType }
			};

			if(IsOneDayReportSelected)
			{
				Identifier = _oneDayReportIdentifier;

				_parameters.Add("date", DateInOneDayReport.Value.Date);
				_parameters.Add("filters_text", filtersText);

				return true;
			}

			if(IsPeriodReportSelected)
			{
				Identifier = _periodReportIdentifier;

				filtersText += $"\nШаг: {SelectedPeriodReportMode.GetEnumDisplayName()}";

				_parameters.Add("start_date", StartDateInPeriodReport.Value.Date);
				_parameters.Add("end_date", EndDateInPeriodReport.Value.Date);
				_parameters.Add("filters_text", filtersText);

				return true;
			}

			throw new InvalidOperationException("Ошибка выбора типа отчета");
		}

		private bool IsReportSettingsValid()
		{
			if(IsOneDayReportSelected == IsPeriodReportSelected)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Ошибка выбора типа отчета");

				return false;
			}

			if(SelectedCarTypesOfUse.Count() == 0)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Не выбран тип ТС");

				return false;
			}

			if(SelectedCarOwnTypes.Count() == 0)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Не выбрана принадлежность ТС");

				return false;
			}

			if(IsOneDayReportSelected)
			{
				if(DateInOneDayReport is null)
				{
					_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"Не установлена дата");

					return false;
				}
			}

			if(IsPeriodReportSelected)
			{
				if(StartDateInPeriodReport == null
					|| EndDateInPeriodReport == null)
				{
					_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						"Не установлен период");

					return false;
				}
			}

			return true;
		}

		public enum CarOwnershipReportPeriodMode
		{
			[Display(Name = "Месяц")]
			Month,
			[Display(Name = "Год")]
			Year
		}
	}
}
