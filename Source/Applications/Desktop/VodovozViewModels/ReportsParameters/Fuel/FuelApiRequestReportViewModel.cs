using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using VodovozInfrastructure.Utils;

namespace Vodovoz.ViewModels.ReportsParameters.Fuel
{
	public class FuelApiRequestReportViewModel : ReportParametersViewModelBase
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private User _user;

		private readonly IInteractiveService _interactiveService;

		public FuelApiRequestReportViewModel(
			ICommonServices commonServices,
			RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по запросам к API Газпром-нефть";
			Identifier = "Cash.MovementsPaymentControlReport";

			CreateReportCommand = new DelegateCommand(CreateReport, () => CanCreateReport);

			StartDate = GeneralUtils.GetCurrentMonthStartDate();
			EndDate = DateTime.Today;
		}

		public DelegateCommand CreateReportCommand { get; }

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		public bool CanCreateReport => StartDate.HasValue && EndDate.HasValue;

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDate.Value.ToString("yyyy-MM-dd") },
					{ "end_date", EndDate.Value.ToString("yyyy-MM-dd") },
					{ "user_id", User?.Id ?? 0 }
				};

				return parameters;
			}
		}

		private void CreateReport()
		{
			if(!CanCreateReport)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для формирования отчета необходимо указать период");
			}

			LoadReport();
		}
	}
}
