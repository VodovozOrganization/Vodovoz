using System;
using Autofac;
using Gtk;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.ReportsParameters.Retail;
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
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				addingRegistrations: builder => builder.RegisterType<QualityReport>().As<IParametersWidget>());
		}

		//TODO: проверить новый формат загрузки старых отчетов
		/// <summary>
		/// Отчет по контрагентам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyRetailReportPressed(object sender, ButtonPressEventArgs e)
		{
			var interactiveService = Startup.AppDIContainer.Resolve<IInteractiveService>();
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			var uowFactory = Startup.AppDIContainer.Resolve<IUnitOfWorkFactory>();

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
				() => new QSReport.ReportViewDlg(new CounterpartyReport(
					reportInfoFactory,
					new SalesChannelJournalFactory(),
					Startup.AppDIContainer.Resolve<IDistrictJournalFactory>(),
					uowFactory,
					interactiveService)));
		}
	}
}
