using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QSReport;
using Vodovoz.ReportsParameters.Retail;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Розница
	/// </summary>
	public class RetailReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public RetailReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
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
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<QualityReport>().As<IParametersWidget>());
		}

		/// <summary>
		/// Отчет по контрагентам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCounterpartyRetailReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<CounterpartyReport>().As<IParametersWidget>());
		}
	}
}
