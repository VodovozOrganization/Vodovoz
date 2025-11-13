using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public class ReportDebtorsBottlesViewModel : ReportParametersViewModelBase
	{
		private bool _showAll;
		private bool _notManualEntered;
		private bool _onlyManualEntered;

		public ReportDebtorsBottlesViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Отчет по должникам тары";
			Identifier = "Client.DebtorsBottles";

			GenerateReportCommand = new DelegateCommand(LoadReport);

			_showAll = true;
		}

		public DelegateCommand GenerateReportCommand;

		public virtual bool ShowAll
		{
			get => _showAll;
			set => SetField(ref _showAll, value);
		}

		public virtual bool NotManualEntered
		{
			get => _notManualEntered;
			set => SetField(ref _notManualEntered, value);
		}

		public virtual bool OnlyManualEntered
		{
			get => _onlyManualEntered;
			set => SetField(ref _onlyManualEntered, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				if(ShowAll)
				{
					return new Dictionary<string, object>
					{
						{ "allshow", "1" },
						{ "withresidue", "-1" }
					};
				}

				if(NotManualEntered)
				{
					return new Dictionary<string, object>
					{
						{ "allshow", "0" },
						{ "withresidue", "0" }
					};
				}

				if(OnlyManualEntered)
				{
					return new Dictionary<string, object>
					{
						{ "allshow", "0" },
						{ "withresidue", "1" }
					};
				}

				throw new NotSupportedException("Не поддерживаемый режим работы отчета");
			}
		}
	}
}
