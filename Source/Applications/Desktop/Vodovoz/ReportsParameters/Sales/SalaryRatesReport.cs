using Gamma.ColumnConfig;
using MoreLinq;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Services;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class SalaryRatesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IWageSettings _wageSettings;
		private readonly ICommonServices _commonServices;

		private readonly GenericObservableList<SalaryRateFilterNode> _salaryRateFilterNodes;

		public SalaryRatesReport(IReportInfoFactory reportInfoFactory, IUnitOfWorkFactory unitOfWorkFactory, IWageSettings wageSettings,
			ICommonServices commonServices)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_wageSettings = wageSettings ?? throw new ArgumentNullException(nameof(wageSettings));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Build();
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			SalaryRateFilterNode salaryRateFilterNodeAlias = null;
			WageDistrictLevelRates wageDistrictLevelRatesAlias = null;
			_salaryRateFilterNodes = new GenericObservableList<SalaryRateFilterNode>(UoW.Session
				.QueryOver(() => wageDistrictLevelRatesAlias).Where(x => !x.IsArchive)
				.SelectList(list => list
					.Select(() => wageDistrictLevelRatesAlias.Name).WithAlias(() => salaryRateFilterNodeAlias.Name)
					.Select(() => wageDistrictLevelRatesAlias.Id).WithAlias(() => salaryRateFilterNodeAlias.WageId))
				.TransformUsing(Transformers.AliasToBean<SalaryRateFilterNode>()).List<SalaryRateFilterNode>());

			treeViewSalaryProperties.ColumnsConfig = FluentColumnsConfig<SalaryRateFilterNode>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.Finish();

			treeViewSalaryProperties.ItemsDataSource = _salaryRateFilterNodes;
		}

		public string Title => "Ставки для водителей";

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "wageIds", _salaryRateFilterNodes.Where(d => d.Selected).Select(d => d.WageId) },
				{ "cityId", _wageSettings.CityWageDistrictId },
				{ "suburbId", _wageSettings.SuburbWageDistrictId }
			};

			var reportInfo = _reportInfoFactory.Create("Sales.SalaryRatesReport", Title, parameters);
			return reportInfo;
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private void OnYcheckAllClicked(object sender, EventArgs e) => _salaryRateFilterNodes.ForEach(x => x.Selected = true);

		private void OnYUnCheckAllClicked(object sender, EventArgs e) => _salaryRateFilterNodes.ForEach(x => x.Selected = false);

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(!_salaryRateFilterNodes.Any(x => x.Selected))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Необходимо выбрать хотя бы одну ставку");
				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
