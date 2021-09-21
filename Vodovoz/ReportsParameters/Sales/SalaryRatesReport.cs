using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using MoreLinq;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalaryRatesReport : SingleUoWWidgetBase, IParametersWidget
	{
		public SalaryRatesReport(IUnitOfWorkFactory unitOfWorkFactory)
		{
			Build();
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			SalaryRateFilterNode salaryRateFilterNodeAlias = null;
			WageDistrictLevelRates wageDistrictLevelRatesAlias = null;
			_salaryRateFilterNodes = UoW.Session.QueryOver(() => wageDistrictLevelRatesAlias).Where(x => !x.IsArchive)
				.SelectList(list => list
					.Select(() => wageDistrictLevelRatesAlias.Name).WithAlias(() => salaryRateFilterNodeAlias.Name)
					.Select(() => wageDistrictLevelRatesAlias.Id).WithAlias(() => salaryRateFilterNodeAlias.WageId))
				.TransformUsing(Transformers.AliasToBean<SalaryRateFilterNode>()).List<SalaryRateFilterNode>();

			treeViewSalaryProperties.ColumnsConfig = FluentColumnsConfig<SalaryRateFilterNode>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();
			
			treeViewSalaryProperties.ItemsDataSource = _salaryRateFilterNodes;
		}

		public string Title => "Ставки для водителей";

		private IList<SalaryRateFilterNode> _salaryRateFilterNodes;
		
		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Sales.SalaryRatesReport",
				Parameters = new Dictionary<string, object>
				{ 
					{ "wageIds", _salaryRateFilterNodes.Where(d => d.Selected).Select(d => d.WageId) }
				}
			};
		}

		private void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}
		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnYcheckAllClicked(object sender, EventArgs e)
		{
			_salaryRateFilterNodes.ForEach(x => x.Selected = true);
			treeViewSalaryProperties.SetItemsSource(_salaryRateFilterNodes);
		}

		protected void OnYUnCheckAllClicked(object sender, EventArgs e)
		{
			_salaryRateFilterNodes.ForEach(x => x.Selected = false);
			treeViewSalaryProperties.SetItemsSource(_salaryRateFilterNodes);
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(!_salaryRateFilterNodes.Where(d => d.Selected).Select(d => d.WageId).Any())
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать хотя бы одну ставку");
				return;
			}

			OnUpdate(true);
			
		}
	}

	public class SalaryRateFilterNode
	{
		public int WageId { get; set; }
		
		public string Title { get; set; }
		
		public string Name { get; set; }
		
		public bool Selected { get; set; }
	}
}
