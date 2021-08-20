using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnedTareReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
		private readonly IInteractiveService _interactiveService;
		public ReturnedTareReport(IEntityAutocompleteSelectorFactory employeeSelectorFactory, IInteractiveService interactiveService)
		{
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.Build();
			btnCreateReport.Clicked += (sender, e) => OnUpdate(true);
			btnCreateReport.Sensitive = false;
			daterangepicker.PeriodChangedByUser += Daterangepicker_PeriodChangedByUser;
			yenumcomboboxDateType.ItemsEnum = typeof(OrderDateType);
			yenumcomboboxDateType.SelectedItem = OrderDateType.CreationDate;
			entityviewmodelentryAuthor.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
			buttonHelp.Clicked += OnButtonHelpClicked;
		}

		private void OnButtonHelpClicked(object sender, EventArgs e)
		{
			var info =
				"В отчёт попадают заказы с учётом выбранных фильтров, а также следующих условий:\n" +
				"- есть возвращённые бутыли\n" +
				"- отстуствуют тмц категории \"Вода\" с объёмом тары 19л.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
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
					{"date_type", ((OrderDateType) yenumcomboboxDateType.SelectedItem) == OrderDateType.CreationDate},
					{"author_employee_id", entityviewmodelentryAuthor.Subject.GetIdOrNull()},
					{"author_employee_name", (entityviewmodelentryAuthor.Subject as Employee)?.FullName},
					{"is_closed_order_only", chkClosedOrdersOnly.Active}
				}
			};
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
