using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Gamma.ColumnConfig;
using Gamma.Utilities;
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
	[ToolboxItem(true)]
	public partial class OrderStatisticByWeekReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly GenericObservableList<GeographicGroupNode> _geographicGroupNodes;
		private bool _showPotentialOrders;
		private OrderStatisticsByWeekReportType _reportType;

		public OrderStatisticByWeekReport()
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, 1, 1);
			dateperiodpicker.EndDate = DateTime.Today;

			cmbReportType.ItemsEnum = typeof(OrderStatisticsByWeekReportType);
			cmbReportType.Binding
				.AddBinding(this, e => e.ReportType, w => w.SelectedItem)
				.InitializeFromSource();

			_geographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				UoW.GetAll<GeoGroup>().Select(gg => new GeographicGroupNode(gg){Selected = true}).ToList());
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(ggn => ggn.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(ggn => ggn.ToString())
				.Finish();

			ytreeviewGeographicGroup.ItemsDataSource = _geographicGroupNodes;
			ytreeviewGeographicGroup.HeadersVisible = false;
			
			chkPotentialOrders.Binding
				.AddBinding(this, e => e.ShowPotentialOrders, w => w.Active)
				.AddBinding(this, e => e.CanChangeShowPotentialOrders, w => w.Sensitive)
				.InitializeFromSource();
		}

		private bool CanChangeShowPotentialOrders => ReportType == OrderStatisticsByWeekReportType.Plan;

		private bool ShowPotentialOrders
		{
			get => _showPotentialOrders;
			set => SetField(ref _showPotentialOrders, value);
		}
		
		private OrderStatisticsByWeekReportType ReportType
		{
			get => _reportType;
			set
			{
				if(SetField(ref _reportType, value))
				{
					UpdateShowPotentialOrders();
				}
			}
		}

		#region IParametersWidget implementation

		public string Title => "Статистика заказов по дням недели";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion
		
		private void UpdateShowPotentialOrders()
		{
			if(ReportType != OrderStatisticsByWeekReportType.Plan)
			{
				ShowPotentialOrders = false;
			}
			OnPropertyChanged(nameof(CanChangeShowPotentialOrders));
		}
		
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

			var reportInfo = new ReportInfo
			{
				
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) },
					{ "report_mode", (int)ReportType },
					{ "geographic_group_id", selectedGeoGroupsIds },
					{ "selected_filters", GetSelectedFilters() },
				}
			};

			if(ShowPotentialOrders)
			{
				reportInfo.Identifier = "Logistic.OrderStatisticByWeekWithPotentialOrders";
				reportInfo.Parameters.Add("show_potential_orders", ShowPotentialOrders);
			}
			else
			{
				reportInfo.Identifier = "Logistic.OrderStatisticByWeek";
			}

			return reportInfo;
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

			result += $" тип значений - {ReportType.GetEnumTitle()}";

			if(ShowPotentialOrders)
			{
				result += $", {chkPotentialOrders.Label}";
			}
			
			return result;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if(EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}
