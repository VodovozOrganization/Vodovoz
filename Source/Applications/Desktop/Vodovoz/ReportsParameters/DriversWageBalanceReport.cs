using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	public partial class DriversWageBalanceReport : SingleUoWWidgetBase, IParametersWidget
	{
		IList<DriverNode> _driversList = new List<DriverNode>();
		private readonly IReportInfoFactory _reportInfoFactory;

		public DriversWageBalanceReport(IReportInfoFactory reportInfoFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по балансу водителей";

		#endregion

		private void Configure()
		{
			FillDrivers();

			ytreeviewDrivers.ColumnsConfig = ColumnsConfigFactory.Create<DriverNode>()
				.AddColumn("Код").AddNumericRenderer(d => d.Id)
				.AddColumn("Имя").AddTextRenderer(d => d.FullName)
				.AddColumn("Выбрать").AddToggleRenderer(d => d.IsSelected)
				.RowCells().AddSetter<CellRenderer>((c, n) =>
					c.CellBackgroundGdk = n.Category == EmployeeCategory.forwarder ? GdkColors.InsensitiveBG : GdkColors.PrimaryBase)
				.Finish();
			ytreeviewDrivers.SetItemsSource(_driversList);

			ydateBalanceBefore.Date = DateTime.Today;
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "date", ydateBalanceBefore.Date.Date.AddDays(1).AddTicks(-1) },
				{ "drivers", _driversList.Where(d => d.IsSelected).Select(d => d.Id) }
			};

			var reportInfo = _reportInfoFactory.Create("Employees.DriversWageBalance", Title, parameters);

			return reportInfo;
		}

		private void FillDrivers()
		{
			DriverNode resultAlias = null;
			Employee employeeAlias = null;

			_driversList = UoW.Session.QueryOver<Employee>(() => employeeAlias)
				.Where(
					() => employeeAlias.Status != EmployeeStatus.IsFired
						&& (employeeAlias.Category == EmployeeCategory.driver || employeeAlias.Category == EmployeeCategory.forwarder)
						&& !employeeAlias.VisitingMaster
						&& employeeAlias.Status != EmployeeStatus.OnCalculation
				)
				.SelectList(list => list
					.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.Category)
					.Select(() => employeeAlias.FirstWorkDay).WithAlias(() => resultAlias.FirstWorkDay))
				.OrderBy(e => e.LastName).Asc.ThenBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DriverNode>())
				.List<DriverNode>();
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			//Сохранение значений во временную структуру
			var oldSelected = new Dictionary<int, bool>();
			foreach(var item in _driversList)
			{
				if(item.IsSelected)
				{
					oldSelected.Add(item.Id, item.IsSelected);
				}
			}

			FillDrivers(); // обновление значений

			//Возврат значений
			foreach(var item in oldSelected)
			{
				for(int i = 0; i < _driversList.Count; i++)
				{
					if(_driversList[i].Id == item.Key)
					{
						_driversList[i].IsSelected = item.Value;
					}
				}
			}

			ytreeviewDrivers.SetItemsSource(_driversList); // Обновление списка

			if(!_driversList.Any(d => d.IsSelected))
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать хотя бы одного водителя");
				return;
			}

			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		protected void OnButtonSelectAllClicked(object sender, EventArgs e)
		{
			foreach(var item in _driversList)
			{
				item.IsSelected = true;
			}
			ytreeviewDrivers.SetItemsSource(_driversList);
		}

		protected void OnButtonUnselectAllClicked(object sender, EventArgs e)
		{
			foreach(var item in _driversList)
			{
				item.IsSelected = false;
			}
			ytreeviewDrivers.SetItemsSource(_driversList);
		}

		protected void OnButtonSelectWageClicked(object sender, EventArgs e)
		{
			FillDrivers();
			foreach(var item in _driversList)
			{
				item.IsSelected = false;
			}

			var driversListFiltered = _driversList.Where(x => CheckDate(x.FirstWorkDay, ydateDateSolary.Date)).ToList();
			foreach(var item in driversListFiltered)
			{
				item.IsSelected = true;
			}

			ytreeviewDrivers.SetItemsSource(_driversList);
		}

		private bool CheckDate(DateTime firstWorkDay, DateTime currentDay)
		{
			if(currentDay.Subtract(firstWorkDay).Days > 14)
			{
				var monday = currentDay.AddDays(1 - ((int)currentDay.DayOfWeek == 0 ? 7 : (int)currentDay.DayOfWeek));
				var daydiff = (monday - firstWorkDay).Days;
				return (daydiff - 1) % 14 < 7;
			}

			return false;
		}

		private class DriverNode
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public string FullName => LastName + " " + Name + (String.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic);
			public bool IsSelected { get; set; }
			public EmployeeCategory Category { get; set; }
			public DateTime FirstWorkDay { get; set; }
		}
	}
}
