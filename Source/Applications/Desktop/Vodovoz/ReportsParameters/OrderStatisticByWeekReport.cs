using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Gamma.ColumnConfig;
using MoreLinq;
using NHibernate.Exceptions;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderStatisticByWeekReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly GenericObservableList<GeographicGroupNode> _geographicGroupNodes;

		public OrderStatisticByWeekReport()
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, 1, 1);
			dateperiodpicker.EndDate = DateTime.Today;

			new List<string>()
			{
				"План",
				"Факт"
			}.ForEach(comboboxReportMode.AppendText);

			comboboxReportMode.Active = 0;

			_geographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				UoW.GetAll<GeoGroup>().Select(gg => new GeographicGroupNode(gg){Selected = true}).ToList());
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(ggn => ggn.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(ggn => ggn.ToString())
				.Finish();

			ytreeviewGeographicGroup.ItemsDataSource = _geographicGroupNodes;
			ytreeviewGeographicGroup.HeadersVisible = false;
		}

		#region IParametersWidget implementation

		public string Title => "Статистика заказов по дням недели";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private void OnUpdate(bool hide = false) =>
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		private void OnButtonRunClicked(object sender, EventArgs e)
		{
			try
			{
				OnUpdate(true);
			}
			catch(GenericADOException ex)
			{
				if(ex.InnerException?.InnerException?.InnerException?.InnerException?.InnerException is SocketException exception
					&& exception.SocketErrorCode == SocketError.TimedOut)
				{
					MessageDialogHelper.RunWarningDialog("Превышен интервал ожидания ответа от сервера:\n" +
														"Попробуйте выбрать меньший интервал времени\n" +
														"или сформируйте отчет чуть позже");
				}
			}
		} 

		private ReportInfo GetReportInfo()
		{
			var selectedGeoGroupsIds = _geographicGroupNodes.Any(ggn => ggn.Selected)
				? _geographicGroupNodes.Where(ggn => ggn.Selected).Select(ggn => ggn.GeographicGroup.Id)
				: _geographicGroupNodes.Select(ggn => ggn.GeographicGroup.Id);

			return new ReportInfo
			{
				Identifier = "Logistic.OrderStatisticByWeek",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) },
					{ "report_mode", comboboxReportMode.Active },
					{ "geographic_group_id", selectedGeoGroupsIds },
					{ "selected_filters", GetSelectedFilters() }
				}
			};
		}

		private string GetSelectedFilters()
		{
			var result = "Фильтры: части города -";
			if(!_geographicGroupNodes.Any(ggn => ggn.Selected)
			   || _geographicGroupNodes.Count(ggn => ggn.Selected) == _geographicGroupNodes.Count)
			{
				result += " все,";
			}
			else
			{
				_geographicGroupNodes.Where(ggn => ggn.Selected).Select(ggn => ggn.ToString()).ForEach(ggName => result += $" {ggName},");
			}

			result += " тип значений -";
			switch(comboboxReportMode.Active)
			{
				case 0:
					result += " план.";
					break;
				case 1:
					result += " факт.";
					break;
			}

			return result;
		}
	}
}
