using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromBankClientReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;
		private readonly IUserRepository userRepository;
		private readonly ICommonServices commonServices;
		public PaymentsFromBankClientReport(
			ReportFactory reportFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IUserRepository userRepository, 
			ICommonServices commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			btnCreateReport.Clicked += (sender, e) => Validate();
			yentryRefSubdivision.SubjectType = typeof(Subdivision);
            entryCounterparty.SetEntityAutocompleteSelectorFactory(counterpartySelectorFactory);
            var currentUserSettings = userRepository.GetUserSettings(UoW, commonServices.UserService.CurrentUserId);
            var defaultCounterparty = currentUserSettings.DefaultCounterparty;
            entryCounterparty.Subject = defaultCounterparty;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по оплатам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			string reportName;
			var parameters = new Dictionary<string, object>();

			parameters.Add("counterparty_id", ((Counterparty)entryCounterparty.Subject)?.Id ?? 0);
			parameters.Add("sort_date", checkSortDate.Active);
			if(checkAllSubdivisions.Active) {
				reportName = "Payments.PaymentsFromBankClientAllSubdivisionsReport";
			} else {
				reportName = "Payments.PaymentsFromBankClientBySubdivisionReport";
				parameters.Add("subdivision_id", ((Subdivision)yentryRefSubdivision.Subject).Id);
			}

			parameters.Add("date", DateTime.Today);

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = reportName;
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void Validate()
		{
			string errorString = string.Empty;
			if(yentryRefSubdivision.Subject == null && !checkAllSubdivisions.Active)
				errorString += "Не заполнено подразделение!\n";
			if(yentryRefSubdivision.Subject != null && checkAllSubdivisions.Active)
				errorString += "Данные установки протеворечат логике работы!\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
