﻿using DateTimeHelpers;
using QS.Commands;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Cash.Reports
{
	public class MovementsPaymentControlViewModel : ReportParametersViewModelBase
	{
		private DateTime _startDate;
		private DateTime _endDate;
		private DelegateCommand _createReportCommand;

		public MovementsPaymentControlViewModel(
			RdlViewerViewModel rdlViewerViewModel
			) : base(rdlViewerViewModel)
		{
			var now = DateTime.Now;
			StartDate = now.Date;
			EndDate = now.LatestDayTime();

			Title = "Контроль оплаты перемещений";
			Identifier = "Cash.DayOfSalaryGiveout";
		}

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
				{
					{ "start_date", StartDate },
					{ "creation_date", EndDate }
				};

				return parameters;
			}
		}

		#region Commands
		public DelegateCommand CreateReportCommand
		{
			get
			{
				if(_createReportCommand == null)
				{
					_createReportCommand = new DelegateCommand(CreateReport, () => CanCreateReport);
					_createReportCommand.CanExecuteChangedWith(this, x => x.CanCreateReport);
				}
				return _createReportCommand;
			}
		}

		public bool CanCreateReport => true;

		private void CreateReport()
		{
			LoadReport();
		}
		#endregion
	}
}
