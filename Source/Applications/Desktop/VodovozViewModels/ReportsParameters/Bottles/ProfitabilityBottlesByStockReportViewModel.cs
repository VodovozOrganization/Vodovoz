using QS.Commands;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public partial class ProfitabilityBottlesByStockReportViewModel : ReportParametersViewModelBase
	{
		private DateTime _startDate;
		private DateTime _endDate;
		private PercentNode _percentNode;

		public ProfitabilityBottlesByStockReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
		) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Рентабельность акции \"Бутыль\"";
			Identifier = "Bottles.ProfitabilityBottlesByStock";

			PercentNodes = new List<PercentNode> {
				new PercentNode(0),
				new PercentNode(10),
				new PercentNode(20)
			};

			_startDate = DateTime.Today;
			_endDate = DateTime.Today;

			GenerateReportCommand = new DelegateCommand(LoadReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public IEnumerable<PercentNode>	PercentNodes { get; }

		public virtual PercentNode SelectedPercentNode
		{
			get => _percentNode;
			set => SetField(ref _percentNode, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "discount_stock", SelectedPercentNode?.Pct ?? -1}
					};

				return parameters;
			}
		}
	}
}
