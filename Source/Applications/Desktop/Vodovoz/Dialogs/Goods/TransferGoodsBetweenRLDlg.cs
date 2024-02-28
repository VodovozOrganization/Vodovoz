using Autofac;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz
{
	public partial class TransferGoodsBetweenRLDlg : QS.Dialog.Gtk.TdiTabBase, ITdiDialog, ISingleUoWDialog, INotifyPropertyChanged
	{
		#region Поля

		private readonly ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

		private readonly IEmployeeNomenclatureMovementRepository _employeeNomenclatureMovementRepository;
		private readonly INomenclatureSettings _nomenclatureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();

		private IColumnsConfig _colConfigFrom = ColumnsConfigFactory.Create<CarUnloadDocumentNode>()
			.AddColumn("Номенклатура").AddTextRenderer(d => d.Nomenclature)
			.AddColumn("Кол-во").AddNumericRenderer(d => d.ItemsCount)
			.AddColumn("Перенести").AddNumericRenderer(d => d.TransferCount)
				.Adjustment(new Gtk.Adjustment(0, 0, 1000, 1, 1, 1)).Editing()
			.AddColumn("Остаток").AddNumericRenderer(d => d.Residue)
			.Finish();

		private IColumnsConfig _colConfigTo = ColumnsConfigFactory.Create<CarUnloadDocumentNode>()
			.AddColumn("Номенклатура").AddTextRenderer(d => d.Nomenclature)
			.AddColumn("Кол-во").AddNumericRenderer(d => d.ItemsCount)
			.Finish();

		private RouteList _routeListFrom;
		private RouteList _routeListTo;

		#endregion

		#region Конструкторы

		public TransferGoodsBetweenRLDlg(IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository)
		{
			Build();
			_employeeNomenclatureMovementRepository = employeeNomenclatureMovementRepository
				?? throw new ArgumentNullException(nameof(employeeNomenclatureMovementRepository));
			TabName = "Перенос разгрузок";
			ConfigureDlg();
		}

		public TransferGoodsBetweenRLDlg(
			RouteList routeList,
			OpenParameter param,
			IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository)
			: this(employeeNomenclatureMovementRepository)
		{
			switch(param) {
				case OpenParameter.Sender:
					RouteListFrom = routeList;
					break;
				case OpenParameter.Receiver:
					RouteListTo = routeList;
					break;
			}
		}

		#endregion

		public IUnitOfWork UoW { get; } = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

		#region ITdiDialog implementation

		public event EventHandler<EntitySavedEventArgs> EntitySaved;
		public event PropertyChangedEventHandler PropertyChanged;

		public bool Save()
		{
			UoW.Commit();

			CheckSensitivities();
			return true;
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges => UoW.HasChanges;

		public virtual bool HasCustomCancellationConfirmationDialog => false;

		public virtual Func<int> CustomCancellationConfirmationDialogFunc => null;

		public ITdiCompatibilityNavigation NavigationManager { get; private set; }

		public RouteList RouteListFrom
		{
			get => _routeListFrom;
			set
			{
				if(_routeListFrom != value)
				{
					_routeListFrom = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RouteListFrom)));
				}
			}
		}

		public RouteList RouteListTo
		{
			get => _routeListTo;
			set
			{
				if(_routeListTo != value)
				{
					_routeListTo = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RouteListTo)));
				}
			}
		}

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			NavigationManager = Startup.MainWin.NavigationManager;

			//Настройка элементов откуда переносим

			entryRouteListFrom.ViewModel = new LegacyEEVMBuilderFactory<TransferGoodsBetweenRLDlg>(this, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.RouteListFrom)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(filter =>
				{
					filter.StartDate = DateTime.Today.AddDays(-7);
					filter.EndDate = DateTime.Today.AddDays(1);
				})
				.Finish();

			entryRouteListFrom.ViewModel.IsEditable = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			entryRouteListFrom.ViewModel.Changed += YentryreferenceRouteListFrom_Changed;

			ylistcomboReceptionTicketFrom.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketFrom.ItemSelected += YlistcomboReceptionTicketFrom_ItemSelected;

			ytreeviewFrom.ColumnsConfig = _colConfigFrom;

			//Настройка компонентов куда переносим
			entryRouteListTo.ViewModel = new LegacyEEVMBuilderFactory<TransferGoodsBetweenRLDlg>(this, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.RouteListTo)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(filter =>
				{
					filter.StartDate = DateTime.Today.AddDays(-7);
					filter.EndDate = DateTime.Today.AddDays(1);
				})
				.Finish();

			entryRouteListTo.ViewModel.IsEditable = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			entryRouteListTo.ViewModel.Changed += YentryreferenceRouteListTo_Changed;

			ylistcomboReceptionTicketTo.SetRenderTextFunc<CarUnloadDocument>(d => $"Талон разгрузки №{d.Id}. {d.Warehouse.Name}");
			ylistcomboReceptionTicketTo.ItemSelected += YlistcomboReceptionTicketTo_ItemSelected;

			ytreeviewTo.ColumnsConfig = _colConfigTo;

			CheckSensitivities();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<CarUnloadDocument>(
				s => {
					foreach(var doc in s.Select(x => x.GetEntity<CarUnloadDocument>())) {
						if(RouteListFrom != null && doc.RouteList.Id == RouteListFrom.Id)
						{
							YentryreferenceRouteListFrom_Changed(this, new EventArgs());
						}

						if(RouteListTo != null && doc.RouteList.Id == RouteListTo.Id)
						{
							YentryreferenceRouteListTo_Changed(this, new EventArgs());
						}
					}
				}
			);
		}

		private void CheckSensitivities()
		{
			buttonTransfer.Sensitive =
				ylistcomboReceptionTicketFrom.SelectedItem != null && ylistcomboReceptionTicketTo.SelectedItem != null;

			buttonCreateNewReceptionTicket.Sensitive = RouteListTo != null && ylistcomboReceptionTicketFrom.SelectedItem != null;

			entryRouteListFrom.Sensitive = !HasChanges;
			entryRouteListTo.Sensitive = !HasChanges;

			ylistcomboReceptionTicketFrom.Sensitive = !HasChanges;
			ylistcomboReceptionTicketTo.Sensitive = !HasChanges;

			ytreeviewFrom.YTreeModel?.EmitModelChanged();
			ytreeviewTo.YTreeModel?.EmitModelChanged();
		}

		#endregion

		#region Обработчики событий

		private void YentryreferenceRouteListFrom_Changed(object sender, EventArgs e)
		{
			if(RouteListFrom == null)
			{
				return;
			}

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == RouteListFrom.Id).List();

			ylistcomboReceptionTicketFrom.ItemsList = unloadDocs;

			if(unloadDocs.Count == 1)
			{
				ylistcomboReceptionTicketFrom.Active = 0;
			}

			CheckSensitivities();
		}

		private void YentryreferenceRouteListTo_Changed(object sender, EventArgs e)
		{
			if(RouteListTo == null)
			{
				return;
			}

			var unloadDocs = UoW.Session.QueryOver<CarUnloadDocument>()
				.Where(cud => cud.RouteList.Id == RouteListTo.Id).List();

			ylistcomboReceptionTicketTo.ItemsList = unloadDocs;

			if(unloadDocs.Count == 1)
			{
				ylistcomboReceptionTicketTo.Active = 0;
			}

			CheckSensitivities();
		}

		private void YlistcomboReceptionTicketFrom_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;

			ytreeviewFrom.SetItemsSource(GetNodesWithoutDriverBalanceNomenclatures(selectedItem));

			CheckSensitivities();
		}

		private void YlistcomboReceptionTicketTo_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			CarUnloadDocument selectedItem = (CarUnloadDocument)e.SelectedItem;
			
			ytreeviewTo.SetItemsSource(GetNodesWithoutDriverBalanceNomenclatures(selectedItem));

			CheckSensitivities();
		}

		private IList<CarUnloadDocumentNode> GetNodesWithoutDriverBalanceNomenclatures(CarUnloadDocument selectedItem) {
			var result = new List<CarUnloadDocumentNode>();

			var driverBalanceNomenclatures =
				_employeeNomenclatureMovementRepository.GetNomenclaturesFromDriverBalance(UoW, selectedItem.RouteList.Driver.Id);

			foreach(var item in selectedItem.Items) {
				var nomenclature = driverBalanceNomenclatures.SingleOrDefault(x =>
					x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
				
				if (nomenclature == null) {
					result.Add(new CarUnloadDocumentNode {DocumentItem = item});
				}
			}

			return result;
		} 

		protected void OnButtonTransferClicked(object sender, EventArgs e)
		{
			var itemsFrom = ytreeviewFrom.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var itemsTo = ytreeviewTo.ItemsDataSource as IList<CarUnloadDocumentNode>;
			var fromDoc = ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument;
			var toDoc = ylistcomboReceptionTicketTo.SelectedItem as CarUnloadDocument;

			foreach(var from in itemsFrom.Where(i => i.TransferCount > 0))
			{
				int transfer = from.TransferCount;

				//Заполняем для краткости
				var nomenclature = from.DocumentItem.GoodsAccountingOperation.Nomenclature;
				var receiveType = from.DocumentItem.ReciveType;

				var to = itemsTo
					.FirstOrDefault(i => i.DocumentItem.GoodsAccountingOperation.Nomenclature.Id == nomenclature.Id);

				if(to == null)
				{
					var tetminalId = _nomenclatureSettings.NomenclatureIdForTerminal;
					toDoc.AddItem(receiveType, nomenclature, null, transfer, null, tetminalId);

					foreach(var item in toDoc.Items)
					{
						var exist = itemsTo.FirstOrDefault(i => i.DocumentItem.Id == item.Id);
						if(exist == null)
						{
							itemsTo.Add(new CarUnloadDocumentNode { DocumentItem = item });
						}
					}
				}
				else
				{
					to.DocumentItem.GoodsAccountingOperation.Amount += transfer;
					to.DocumentItem.DeliveryFreeBalanceOperation.Amount -= transfer;

					UoW.Save(to.DocumentItem.GoodsAccountingOperation);
					UoW.Save(to.DocumentItem.DeliveryFreeBalanceOperation);
				}

				from.DocumentItem.GoodsAccountingOperation.Amount -= transfer;
				from.DocumentItem.DeliveryFreeBalanceOperation.Amount += transfer;

				if(from.DocumentItem.GoodsAccountingOperation.Amount == 0)
				{
					var item = fromDoc.Items.First(i => i.Id == from.DocumentItem.Id);
					fromDoc.Items.Remove(item);

					UoW.Delete(from.DocumentItem.GoodsAccountingOperation);
					UoW.Delete(from.DocumentItem.DeliveryFreeBalanceOperation);
				}
				else
				{
					UoW.Save(from.DocumentItem.GoodsAccountingOperation);
					UoW.Save(from.DocumentItem.DeliveryFreeBalanceOperation);
				}

				from.TransferCount = 0;
			}
			UoW.Save(fromDoc);
			UoW.Save(toDoc);
			CheckSensitivities();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
			OnCloseTab(false);
		}

		protected void OnButtonCreateNewReceptionTicketClicked(object sender, EventArgs e)
		{
			TabParent.AddSlaveTab(
				this,
				new CarUnloadDocumentDlg(
					RouteListTo.Id,
					(ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument).Id,
					(ylistcomboReceptionTicketFrom.SelectedItem as CarUnloadDocument).TimeStamp
				)
			);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		#endregion

		#region Внутренние классы

		public enum OpenParameter { Sender, Receiver }

		private class CarUnloadDocumentNode
		{
			private int _transferCount = 0;

			public string Id => DocumentItem.Id.ToString();
			public string Nomenclature => DocumentItem.GoodsAccountingOperation.Nomenclature.OfficialName;
			public int ItemsCount => (int)DocumentItem.GoodsAccountingOperation.Amount;

			public int TransferCount
			{
				get => _transferCount;
				set
				{
					_transferCount = value;
					if(value < 0)
					{
						_transferCount = 0;
					}

					if(value > ItemsCount)
					{
						_transferCount = ItemsCount;
					}
				}
			}

			public int Residue => ItemsCount - TransferCount;

			public CarUnloadDocumentItem DocumentItem { get; set; }
		}

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			UoW?.Dispose();
			base.Dispose();
		}

		#endregion
	}
}

