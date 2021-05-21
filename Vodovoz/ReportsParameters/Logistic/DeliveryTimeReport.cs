using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.ColumnConfig;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IInteractiveService interactiveService;

		public DeliveryTimeReport(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService)
		{
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			Build();
			ConfigureDlg();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет 'Время доставки'";

		#endregion

		private void ConfigureDlg()
		{
			ytreeGeoGroups.HeadersVisible = false;
			ytreeGeoGroups.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			ytreeGeoGroups.ItemsDataSource =
				UoW.GetAll<GeographicGroup>().Select(x => new SelectableParameter { GeographicGroup = x, IsSelected = true }).ToList();

			ytreeRouteListTypeOfUse.HeadersVisible = false;
			ytreeRouteListTypeOfUse.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("").AddEnumRenderer(x => x.RouteListTypeOfUse)
				.Finish();
			ytreeRouteListTypeOfUse.ItemsDataSource = Enum.GetValues(typeof(RouteListTypeOfUse)).Cast<RouteListTypeOfUse>()
				.Select(x => new SelectableParameter { RouteListTypeOfUse = x, IsSelected = x == RouteListTypeOfUse.Logistics}).ToList();
		}

		private void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.DeliveryTime",
				Parameters = new Dictionary<string, object>
				{
					{ "beforeTime", ytimeDelivery.Text },
					{ "geographic_groups", GetSelectedGeoGroupIds() },
					{ "rl_type_of_use", GetSelectedRouteListTypeOfUses() }
				}
			};
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(!GetSelectedGeoGroupIds().Any())
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна часть города");
				return;
			}
			if(!GetSelectedRouteListTypeOfUses().Any())
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана ни одна принадлежность МЛ");
				return;
			}
			OnUpdate(true);
		}

		private IEnumerable<RouteListTypeOfUse> GetSelectedRouteListTypeOfUses()
		{
			return (ytreeRouteListTypeOfUse.ItemsDataSource as IEnumerable<SelectableParameter>)
				?.Where(x => x.IsSelected)
				.Select(x => x.RouteListTypeOfUse)
				?? new List<RouteListTypeOfUse>();
		}

		private IEnumerable<int> GetSelectedGeoGroupIds()
		{
			return (ytreeGeoGroups.ItemsDataSource as IEnumerable<SelectableParameter>)
				?.Where(x => x.IsSelected)
				.Select(x => x.GeographicGroup.Id)
				?? new List<int>();
		}

		private void OnYtimeDeliveryChanged(object sender, EventArgs e)
		{
			buttonCreateReport.Sensitive = ytimeDelivery.Time != default(TimeSpan);
		}

		private class SelectableParameter
		{
			public bool IsSelected { get; set; }
			public GeographicGroup GeographicGroup { get; set; }
			public RouteListTypeOfUse RouteListTypeOfUse { get; set; }
		}

		private enum RouteListTypeOfUse
		{
			[Display(Name = "Логистика")]
			Logistics,
			[Display(Name = "СЦ")]
			ServiceCenter,
			[Display(Name = "Фуры")]
			CompanyTrucks
		}
	}
}
