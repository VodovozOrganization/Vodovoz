using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Organizations;
using Gamma.ColumnConfig;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Parameters;
using QS.Project.Services;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
    public partial class OrderChangesReport : SingleUoWWidgetBase, IParametersWidget
    {
        private List<SelectedChangeTypeNode> _changeTypes = new List<SelectedChangeTypeNode>();
        private List<SelectedIssueTypeNode> _issueTypes = new List<SelectedIssueTypeNode>();
        private readonly IReportDefaultsProvider reportDefaultsProvider;
		private readonly IInteractiveService _interactiveService;
		private readonly int _monitoringPeriodAvailable;

		public OrderChangesReport(
			IReportDefaultsProvider reportDefaultsProvider,
			IInteractiveService interactiveService,
			IArchiveDataSettings archiveDataSettings)
        {
            this.reportDefaultsProvider = reportDefaultsProvider ?? throw new ArgumentNullException(nameof(reportDefaultsProvider));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_monitoringPeriodAvailable =
				(archiveDataSettings ?? throw new ArgumentNullException(nameof(archiveDataSettings))).GetMonitoringPeriodAvailableInDays;
			this.Build();
            Configure();
        }

        private void Configure()
        {
            UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			chkOldMonitoring.Toggled += (sender, e) => UpdatePeriod();
			UpdatePeriod();
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
            dateperiodpicker.PeriodChangedByUser += OnDateChanged;
            var organizations = UoW.GetAll<Organization>();
            comboOrganization.ItemsList = organizations;
            comboOrganization.SetRenderTextFunc<Organization>(x => x.FullName);
            comboOrganization.Changed += (sender, e) => UpdateSensitivity();
            comboOrganization.SelectedItem = organizations.FirstOrDefault(x => x.Id == reportDefaultsProvider.GetDefaultOrderChangesOrganizationId);
            ytreeviewChangeTypes.ColumnsConfig = FluentColumnsConfig<SelectedChangeTypeNode>.Create()
                .AddColumn("✓").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип").AddTextRenderer(x => x.Title)
                .Finish();

            AddChangeType("Фактическое кол-во товара", "ActualCount");
            AddChangeType("Цена товара", "Price");
            AddChangeType("Добавление/Удаление товаров", "OrderItemsCount");
            AddChangeType("Тип оплаты заказа", "PaymentType");

            ytreeviewChangeTypes.ItemsDataSource = _changeTypes;

            ytreeviewIssueTypes.ColumnsConfig = FluentColumnsConfig<SelectedIssueTypeNode>.Create()
                .AddColumn("✓").AddToggleRenderer(x => x.Selected)
                .AddColumn("Тип").AddTextRenderer(x => x.Title)
                .Finish();

            AddIssueType("Проблемы с смс", "SmsIssues");
            AddIssueType("Проблемы с qr", "QrIssues");
            AddIssueType("Проблемы с терминалами", "TerminalIssues");
            AddIssueType("Проблемы менеджеров", "ManagersIssues");

            ytreeviewIssueTypes.ItemsDataSource = _issueTypes;
		}

		private void UpdatePeriod()
		{
			if(chkOldMonitoring.Active)
			{
				dateperiodpicker.SetPeriod(
					DateTime.Today.AddDays(-_monitoringPeriodAvailable), DateTime.Today.AddDays(-_monitoringPeriodAvailable));
			}
			else
			{
				dateperiodpicker.SetPeriod(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
			}
		}
		
		private void AddChangeType(string title, string value)
        {
            var changeType = new SelectedChangeTypeNode
			{
				Title = title,
				Value = value
			};
			changeType.PropertyChanged += (sender, e) => UpdateSensitivity();
            changeType.Selected = true;
            _changeTypes.Add(changeType);
        }

        private void AddIssueType(string title, string value)
        {
            var issueType = new SelectedIssueTypeNode
			{
				Title = title,
				Value = value
			};
			issueType.PropertyChanged += (sender, e) => UpdateSensitivity();
            issueType.Selected = true;
            _issueTypes.Add(issueType);
        }

        #region IParametersWidget implementation

        public string Title => "Отчет по изменениям заказа при доставке";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        #endregion IParametersWidget implementation

        private ReportInfo GetReportInfo()
		{
			var rdlPath = chkOldMonitoring.Active ? GetTempRdl() : GetRdlPath();
			
            var ordganizationId = ((Organization)comboOrganization.SelectedItem).Id;
            var selectedChangeTypes =
				string.Join(",", _changeTypes.Where(x => x.Selected).Select(x => x.Value));
            var selectedIssueTypes = _changeTypes.Any(x => x.Selected && x.Value == "PaymentType")
				? string.Empty
				: string.Join(",", _issueTypes.Where(x => x.Selected).Select(x => x.Value));
            var selectedChangeTypesTitles =
				string.Join(", ", _changeTypes.Where(x => x.Selected).Select(x => x.Title));
            var selectedIssueTypesTitles = _changeTypes.Any(x => x.Selected && x.Value == "PaymentType")
				? string.Empty
				: string.Join(", ", _issueTypes.Where(x => x.Selected).Select(x => x.Title));

            var parameters = new Dictionary<string, object>
                {
                    { "start_date", dateperiodpicker.StartDate },
                    { "end_date", dateperiodpicker.EndDate },
                    { "organization_id", ordganizationId },
                    { "change_types", selectedChangeTypes },
                    { "change_types_rus", selectedChangeTypesTitles },
                    { "issue_types", selectedIssueTypes },
                    { "issue_types_rus", selectedIssueTypesTitles },
				};

            return new ReportInfo
            {
				Path = rdlPath,
                UseUserVariables = true,
                Parameters = parameters
            };
		}

		private string GetTempRdl()
		{
			string RdlText;
			
			using(var reader = new StreamReader(GetRdlPath()))
			{
				RdlText = reader.ReadToEnd();
			}

			RdlText = RdlText.Replace("history_changed_entities", "Vodovoz_old_monitoring.history_changed_entities");
			RdlText = RdlText.Replace("history_changes ", "Vodovoz_old_monitoring.history_changes ");
			RdlText = RdlText.Replace("history_changeset", "Vodovoz_old_monitoring.history_changeset");

			var tempRdlPath = System.IO.Path.GetTempFileName();
			using(StreamWriter sw = new StreamWriter(tempRdlPath))
			{
				sw.Write(RdlText);
			}

			return tempRdlPath;
		}

		private static string GetRdlPath()
		{
			return System.IO.Path.Combine(Environment.CurrentDirectory, "Reports/Orders/OrderChangesReport.rdl");
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
        {
            if (dateperiodpicker.StartDateOrNull == null
                || (dateperiodpicker.StartDateOrNull != null && dateperiodpicker.StartDate >= DateTime.Now)
                || comboOrganization.SelectedItem == null
                || (!_changeTypes.Any(x => x.Selected) && !_issueTypes.Any(x => x.Selected))
                )
			{
                return;
            }

            var reportInfo = GetReportInfo();
            LoadReport?.Invoke(this, new LoadReportEventArgs(reportInfo));
        }

        private bool issuesSensitive => _changeTypes.Any(x => x.Value == "PaymentType" && !x.Selected);
        
		private void UpdateSensitivity()
        {
			bool isValidDate;
			if(chkOldMonitoring.Active)
			{
				isValidDate =
				 dateperiodpicker.StartDateOrNull != null
					&& dateperiodpicker.EndDateOrNull != null
					&& dateperiodpicker.StartDate < DateTime.Today.AddDays(-_monitoringPeriodAvailable + 1)
					&& dateperiodpicker.EndDate < DateTime.Today.AddDays(-_monitoringPeriodAvailable + 1);
			}
			else
			{
				isValidDate = dateperiodpicker.StartDateOrNull != null
					&& dateperiodpicker.StartDate >= DateTime.Today.AddDays(-_monitoringPeriodAvailable + 1);
			}

			if(!isValidDate)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					chkOldMonitoring.Active ? "Можно выбирать период только раньше 60 дней" : "Можно выбирать только последние 60 дней");
			}

			bool hasOrganization = comboOrganization.SelectedItem != null;
            bool hasChangeTypes = _changeTypes.Any(x => x.Selected);
            bool hasIssueTypes = _issueTypes.Any(x => x.Selected);
            buttonCreateReport.Sensitive = isValidDate && hasOrganization && (hasChangeTypes || hasIssueTypes);
            ytreeviewIssueTypes.Sensitive = issuesSensitive;
        }

        private void UpdatePeriodMessage()
        {
            if(dateperiodpicker.StartDateOrNull == null && dateperiodpicker.EndDateOrNull == null)
            {
                ylabelDateWarning.Visible = false;
                return;
            }
            
			if((dateperiodpicker.StartDateOrNull == null && dateperiodpicker.EndDateOrNull != null)
				|| (dateperiodpicker.StartDateOrNull != null && dateperiodpicker.StartDate < DateTime.Today.AddDays(-13)))
			{
				ylabelDateWarning.Visible = true;
			}
			if(dateperiodpicker.StartDateOrNull != null)
			{
				var period = dateperiodpicker.EndDateOrNull == null
					? DateTime.Now - dateperiodpicker.StartDate
					: dateperiodpicker.EndDate - dateperiodpicker.StartDate;
				ylabelDateWarning.Visible = period.Days > 14;
			}
		}

        private void OnDateChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
            UpdatePeriodMessage();
        }
    }

    public class SelectedChangeTypeNode : PropertyChangedBase
    {
        private bool selected;
        public virtual bool Selected
        {
            get => selected;
            set => SetField(ref selected, value);
        }

        public string Title { get; set; }

        public string Value { get; set; }
    }

    public class SelectedIssueTypeNode : PropertyChangedBase
    {
        private bool selected;
        public virtual bool Selected
        {
            get => selected;
            set => SetField(ref selected, value);
        }

        public string Title { get; set; }

        public string Value { get; set; }
    }
}
