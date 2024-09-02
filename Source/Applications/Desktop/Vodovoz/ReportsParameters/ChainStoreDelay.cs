﻿using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Reports;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class ChainStoreDelayReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly ReportFactory _reportFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly ICounterpartySettings _counterpartySettings;
		private KeyValuePair<string, string> _mode;

		public ChainStoreDelayReport(
			ILifetimeScope lifetimeScope,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			ICounterpartySettings counterpartySettings)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			Build();
			Configure(lifetimeScope);
		}

		public KeyValuePair<string, string> Mode
		{
			get => _mode;
			set
			{
				if(!_mode.Equals(value))
				{
					_mode = value;
				}
			}
		}

		public Dictionary<string, string> Modes = new Dictionary<string, string>
		{
			{ "Networks", "Сетям" },
			{ "Tenders", "Тендерам" }
		};

		#region IParametersWidget implementation

		public string Title => "Отсрочка сети";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void Configure(ILifetimeScope lifetimeScope)
		{
			var uowFactory = lifetimeScope.Resolve<IUnitOfWorkFactory>();
			UoW = uowFactory.CreateWithoutRoot();
			ydatepicker.Date = DateTime.Now.Date;
			ConfigureEntries(lifetimeScope);
			ydatepicker.Date = DateTime.Now;
			buttonRun.Sensitive = true;
			buttonRun.Clicked += OnButtonCreateReportClicked;
		}

		private void ConfigureEntries(ILifetimeScope lifetimeScope)
		{
			entityviewmodelentryCounterparty.SetEntityAutocompleteSelectorFactory(
				_counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope));

			entityviewmodelentryCounterparty.ChangedByUser += OnEntityviewmodelentryCounterpartyChangedByUser;

			entityviewmodelentrySellManager.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());

			entityviewmodelentryOrderAuthor.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());

			speciallistcomboboxReportBy.ItemsList = Modes;
			speciallistcomboboxReportBy.SetRenderTextFunc<KeyValuePair<string, string>>(node => node.Value);
			speciallistcomboboxReportBy.Binding
				.AddBinding(this, r => r.Mode, w => w.SelectedItem)
				.InitializeFromSource();

			speciallistcomboboxReportBy.SelectedItem = Modes.FirstOrDefault();
		}

		private void OnEntityviewmodelentryCounterpartyChangedByUser(object sender, EventArgs e)
		{
			speciallistcomboboxReportBy.Sensitive = entityviewmodelentryCounterparty.Subject == null;
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			. return new ReportInfo
			{
				Identifier = "Payments.PaymentsDelayNetwork",
				Parameters = new Dictionary<string, object> {
					{ "date", ydatepicker.Date },
					{ "mode", Mode.Key },
					{ "counterparty_id", (entityviewmodelentryCounterparty.Subject as Counterparty)?.Id ?? -1},
					{ "sell_manager_id", (entityviewmodelentrySellManager.Subject as Employee)?.Id ?? -1},
					{ "order_author_id", (entityviewmodelentryOrderAuthor.Subject as Employee)?.Id ?? -1},
					{ "counterparty_from_tender_id", _counterpartySettings.CounterpartyFromTenderId }
				}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Payments.PaymentsDelayNetwork";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		private void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
