using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Selfdelivery
{
	public class SelfDeliveryReportViewModel : ReportParametersViewModelBase
	{
		private const int _reportMaxPeriod = 62;

		private bool _canGenerateReport;
		private DateTime _startDate;
		private DateTime _endDate;
		private bool _warningVisible;
		private string _warningText;

		public SelfDeliveryReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по самовывозу";
			Identifier = "Orders.SelfDeliveryReport";

			GenerateReportCommand = new DelegateCommand(LoadReport, () => CanGenerateReport);
			GenerateReportCommand.CanExecuteChangedWith(this, x => x.CanGenerateReport);

			_startDate = DateTime.Now.Date;
			_endDate = DateTime.Now.Date;
			Validate();
		}

		public DelegateCommand GenerateReportCommand;

		public virtual bool CanGenerateReport
		{
			get => _canGenerateReport;
			set => SetField(ref _canGenerateReport, value);
		}

		public virtual DateTime StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value))
				{
					Validate();
				}
			}
		}

		public virtual DateTime EndDate
		{
			get => _endDate;
			set
			{
				if(SetField(ref _endDate, value))
				{
					Validate();
				}
			}
		}

		public virtual bool WarningVisible
		{
			get => _warningVisible;
			set => SetField(ref _warningVisible, value);
		}

		public virtual string WarningText
		{
			get => _warningText;
			set => SetField(ref _warningText, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "startDate", StartDate },
						{ "endDate", EndDate },
						{ "isOneDayReport", StartDate.Date == EndDate.Date }
					};

				return parameters;
			}
		}

		private void Validate()
		{
			if(StartDate.Date.AddDays(_reportMaxPeriod - 1) < EndDate.Date)
			{
				WarningVisible = true;
				WarningText = $"Выбран период более {_reportMaxPeriod} дней";
				CanGenerateReport = false;
			}
			else
			{
				WarningVisible = false;
				WarningText = "";
				CanGenerateReport = true;
			}
		}
	}
}
