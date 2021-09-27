using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using MoreLinq;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalaryRatesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ICommonServices _commonServices;

		public SalaryRatesReport(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
		{
			Build();
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
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

		private GenericObservableList<SalaryRateFilterNode> _salaryRateFilterNodes;
		
		private ReportInfo GetReportInfo()
		{
			var cityDistrict = UoW.Session.QueryOver<WageDistrict>().Where(x => x.Id == 1).SingleOrDefault();
			var suburbDistrict = UoW.Session.QueryOver<WageDistrict>().Where(x => x.Id == 1).SingleOrDefault();
			return new ReportInfo
			{
				Identifier = "Sales.SalaryRatesReport",
				Parameters = new Dictionary<string, object>
				{ 
					{ "wageIds", _salaryRateFilterNodes.Where(d => d.Selected).Select(d => d.WageId) },
					{ "cityId", cityDistrict.Id},
					{ "suburbId", suburbDistrict.Id},
				}
			};
		}

		private void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

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

			OnUpdate(true);
		}
	}

	public class SalaryRateFilterNode : PropertyChangedBase
	{
		public int WageId { get; set; }
		
		public string Title { get; set; }
		
		public string Name { get; set; }

		private bool _selected;

		public bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}
	}
}
