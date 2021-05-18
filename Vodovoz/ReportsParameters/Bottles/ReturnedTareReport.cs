using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnedTareReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
		public ReturnedTareReport(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.Build();
			btnCreateReport.Clicked += (sender, e) => OnUpdate(true);
			btnCreateReport.Sensitive = false;
			daterangepicker.PeriodChangedByUser += Daterangepicker_PeriodChangedByUser;
			yenumcomboboxDateType.ItemsEnum = typeof(DateType);
			yenumcomboboxDateType.SelectedItem = DateType.CreationDate;
			entityviewmodelentryAuthor.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
		}

		void Daterangepicker_PeriodChangedByUser(object sender, EventArgs e) =>
			btnCreateReport.Sensitive = daterangepicker.EndDateOrNull.HasValue && daterangepicker.StartDateOrNull.HasValue;


		#region IParametersWidget implementation

		public string Title => "Отчет по забору тары";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Bottles.ReturnedTareReport",
				Parameters = new Dictionary<string, object>
				{
					{"start_date", daterangepicker.StartDate},
					{"end_date", daterangepicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59)},
					{"date", DateTime.Now},
					{"date_type", ((DateType) yenumcomboboxDateType.SelectedItem) == DateType.CreationDate},
					{"author_employee_id", entityviewmodelentryAuthor.Subject.GetIdOrNull()},
					{"author_employee_name", (entityviewmodelentryAuthor.Subject as Employee)?.FullName}
				}
			};
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		
		public enum DateType
		{
			[Display (Name = "Дата создания")]
			CreationDate,
			[Display(Name = "Дата доставки")]
			DeliveryDate
		}
	}
}
