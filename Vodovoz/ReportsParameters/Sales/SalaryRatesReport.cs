using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QSReport;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalaryRatesReport : SingleUoWWidgetBase, IParametersWidget
	{
		public SalaryRatesReport()
		{
			Build();
			var arrayOfWageRate = Enum.GetValues(typeof(WageRateTypes));
			var salaryRateFilterNode = new List<SalaryRateFilterNode>();
			
			foreach (var rate in arrayOfWageRate)
				salaryRateFilterNode.Add(new SalaryRateFilterNode((WageRateTypes)rate));
			
			treeViewSalaryProperties.ColumnsConfig = FluentColumnsConfig<SalaryRateFilterNode>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();
			
			treeViewSalaryProperties.ItemsDataSource = salaryRateFilterNode;
		}

		public string Title => "Ставки для водителей";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnYcheckAllClicked(object sender, EventArgs e)
		{
		}

		protected void OnYUnCheckAllClicked(object sender, EventArgs e)
		{
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
		}
	}

	public class SalaryRateFilterNode
	{
		public SalaryRateFilterNode(WageRateTypes wageRateTypes)
		{
			Title = wageRateTypes.GetEnumTitle();
			Selected = false;
		}
		public string Title { get; set; }
		
		public bool Selected { get; set; }
	}
}
