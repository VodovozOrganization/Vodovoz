using System;
using QSTDI;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	public partial class TransferGoodsBetweenRLDlg : TdiTabBase, ITdiDialog, IOrmDialog
	{
		#region Поля

		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		#endregion

		public TransferGoodsBetweenRLDlg()
		{
			this.Build();
			this.TabName = "Перенос разгрузок";
			ConfigureDlg();
		}

		#region ITdiDialog implementation

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			return false;
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges {
			get {
				return false;
			}
		}

		#endregion

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get { return uow; } }

		public object EntityObject { get { return null; } }

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			//Настройка элементов откуда переносим
			RouteListsFilter filterFrom = new RouteListsFilter(UoW);
			filterFrom.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListFrom.RepresentationModel = new ViewModel.RouteListsVM(filterFrom);
			yentryreferenceRouteListFrom.Changed += YentryreferenceRouteListFrom_Changed;
			ylistcomboReceptionTicketFrom.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. Склад {d.Warehouse.Name}");

			//Настройка компонентов куда переносим
			RouteListsFilter filterTo = new RouteListsFilter(UoW);
			filterTo.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteListTo.RepresentationModel = new ViewModel.RouteListsVM(filterTo);
		}

		void YentryreferenceRouteListFrom_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRouteListFrom.Subject == null)
				return;

			RouteList routeList = (RouteList)yentryreferenceRouteListFrom.Subject;

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == routeList.Id).List();
			
			ylistcomboReceptionTicketFrom.ItemsList = unloadDocs;
		}

		#endregion
	}
}

