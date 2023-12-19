using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.ReportsParameters.Retail;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class RetailReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public RetailReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var retailMenuItem = _concreteMenuItemCreator.CreateMenuItem("Розница");
			var retailMenu = new Menu();
			retailMenuItem.Submenu = retailMenu;

			retailMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Качественный отчет", OnQualityRetailReportPressed));
			retailMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по контрагентам", OnCounterpartyRetailReportPressed));
			
			return retailMenuItem;
		}
		
		/// <summary>
		/// Качественный отчет
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnQualityRetailReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<QualityReport>(),
				() => new QSReport.ReportViewDlg(new QualityReport(
					new CounterpartyJournalFactory(),
					new EmployeeJournalFactory(Startup.MainWin.NavigationManager),
					new SalesChannelJournalFactory(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Отчет по контрагентам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyRetailReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
				() => new QSReport.ReportViewDlg(new CounterpartyReport(
					new SalesChannelJournalFactory(),
					new DistrictJournalFactory(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.InteractiveService)));

		}
	}
}
