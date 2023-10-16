using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromBankClientReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IUserRepository userRepository;
		private readonly ICommonServices commonServices;
		public PaymentsFromBankClientReport(
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IUserRepository userRepository,
			ICommonServices commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			btnCreateReport.Clicked += (sender, e) => Validate();

            entryCounterparty.SetEntityAutocompleteSelectorFactory(_counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());
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
				parameters.Add("subdivision_id", ((Subdivision)entrySubdivision.ViewModel.Entity).Id);
			}

			parameters.Add("date", DateTime.Today);

			return new ReportInfo {
				Identifier = reportName,
				Parameters = parameters
			};
		}

		void Validate()
		{
			string errorString = string.Empty;
			if(entrySubdivision.ViewModel.Entity == null && !checkAllSubdivisions.Active)
			{
				errorString += "Не заполнено подразделение!\n";
			}

			if(entrySubdivision.ViewModel.Entity != null && checkAllSubdivisions.Active)
			{
				errorString += "Данные установки протеворечат логике работы!\n";
			}

			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
