using System;
using Gtk;
using Vodovoz.ReportsParameters.Production;
using Vodovoz.TempAdapters;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class ManufacturingReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ManufacturingReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var manufacturingMenuItem = _concreteMenuItemCreator.CreateMenuItem("Производство");
			var manufacturingMenu = new Menu();
			manufacturingMenuItem.Submenu = manufacturingMenu;

			manufacturingMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по произведенной продукции", OnProducedProductionReportPressed));
			
			return manufacturingMenuItem;
		}
		
		/// <summary>
		/// Отчет по произведенной продукции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProducedProductionReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ProducedProductionReport>(),
				() => new QSReport.ReportViewDlg(
					new ProducedProductionReport(new NomenclatureJournalFactory())));
		}
	}
}
