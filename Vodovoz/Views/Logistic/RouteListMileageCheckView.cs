using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageCheckView : TabViewBase<RouteListMileageCheckViewModel>
	{
		public RouteListMileageCheckView(RouteListMileageCheckViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{

		}

		#region Настройка конфигураций

		private void ConfigureDlg()
		{
			//buttonAcceptFine.Clicked += ButtonAcceptFineOnClicked;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(ViewModel.CarSelectorFactory);
			entityviewmodelentryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, widget => widget.Subject).InitializeFromSource();

			evmeForwarder.SetEntityAutocompleteSelectorFactory(ViewModel.ForwarderSelectorFactory);
			evmeForwarder.Binding
				.AddBinding(ViewModel.Entity, e => e.Forwarder, widget => widget.Subject)
				.AddBinding(ViewModel.Entity, e => e.CanAddForwarder, widget => widget.Sensitive)
				.InitializeFromSource();

			evmeLogistician.SetEntityAutocompleteSelectorFactory(ViewModel.LogisticianSelectorFactory);
			evmeLogistician.Binding.AddBinding(ViewModel.Entity, e => e.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = ViewModel.DeliveryShifts;
			speccomboShift.Binding.AddBinding(ViewModel.Entity, e => e.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(ViewModel.Entity, e => e.Date, widget => widget.Date).InitializeFromSource();

			yspinConfirmedDistance.Binding.AddBinding(ViewModel.Entity, e => e.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();

			yentryRecalculatedDistance.Binding.AddBinding(ViewModel.Entity, e => e.RecalculatedDistance, widget => widget.Text, new DecimalToStringConverter()).InitializeFromSource();

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())
				.AddColumn("Адрес")
					.AddTextRenderer(node => String.Format("{0} д.{1}", node.RouteListItem.Order.DeliveryPoint.Street, node.RouteListItem.Order.DeliveryPoint.Building))
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.Status).Editing(false)
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.RouteListItem.Order.IsFastDelivery).Editing(false)
				.AddColumn("Последнее редактирование")
					.AddTextRenderer(node => node.LastUpdate)
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();

			ytreeviewAddresses.ItemsDataSource = ViewModel.RouteListItems;

			ytextviewMileageComment.Binding.AddBinding(ViewModel.Entity, e => e.MileageComment, w => w.Buffer.Text).InitializeFromSource();

			//Телефон
			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = MainClass.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(ViewModel.Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			yvboxRouteList.Binding.AddFuncBinding(ViewModel, vm => !vm.CanEdit || vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();
			ytableMain.Binding.AddFuncBinding(ViewModel, vm => !vm.CanEdit || vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();
			
			ybuttonSave.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit && vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();
			yhboxMileageComment.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit && vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();
			ytreeviewAddresses.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit && vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();
			yhboxBottom.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit && vm.Entity.Status != RouteListStatus.Closed, w => w.Sensitive).InitializeFromSource();

			//buttonAccept.Binding.AddFuncBinding(ViewModel, vm => vm.IsAcceptAvailable, w => w.Sensitive).InitializeFromSource();
		}


		#endregion

		//#region implemented abstract members of OrmGtkDialogBase




		//#endregion

		//#region Обработка нажатий кнопок

		//protected void OnButtonAcceptClicked(object sender, EventArgs e)
		//{
		//	var validationContext = _validationContextFactory.CreateNewValidationContext(
		//		Entity,
		//		new Dictionary<object, object> {
		//			{ "NewStatus", RouteListStatus.Closed },
		//			{ nameof(IRouteListItemRepository), new RouteListItemRepository() }
		//		});
		//	validationContext.ServiceContainer.AddService(new OrderParametersProvider(_parametersProvider));
		//	validationContext.ServiceContainer.AddService(new DeliveryRulesParametersProvider(_parametersProvider));

		//	if(!ServicesConfig.ValidationService.Validate(Entity, validationContext))
		//	{
		//		return;
		//	}

		//	if(Entity.Status == RouteListStatus.Delivered)
		//	{
		//		ChangeStatusAndCreateTaskFromDelivered();
		//	}
		//	Entity.AcceptMileage(CallTaskWorker);

		//	UpdateStates();
		//	SaveAndClose();
		//}

		//private void UpdateStates()
		//{
		//	buttonAccept.Sensitive = Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck;
		//}

		//protected void OnButtonOpenMapClicked(object sender, EventArgs e)
		//{
		//	var trackWnd = new TrackOnMapWnd(UoWGeneric);
		//	trackWnd.Show();
		//}

		//protected void OnButtonFromTrackClicked(object sender, EventArgs e)
		//{
		//	var track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);
		//	if(track == null)
		//	{
		//		ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Невозможно расчитать растояние, так как в маршрутном листе нет трека", "");
		//		return;
		//	}
		//	Entity.ConfirmedDistance = (decimal)track.TotalDistance.Value;
		//}

		//private void ButtonAcceptFineOnClicked(object sender, EventArgs e)
		//{
		//	string fineReason = "Перерасход топлива";

		//	var fineDlg = new FineDlg(0, Entity, fineReason, Entity.Date, Entity.Driver);
		//	fineDlg.Entity.FineType = FineTypes.FuelOverspending;
		//	fineDlg.EntitySaved += OnFinesAdded;

		//	TabParent.AddSlaveTab(this, fineDlg);
		//}

		//#endregion

		//#region Обработка добавления долгов

		//protected void OnFinesAdded(object sender, EventArgs e)
		//{
		//	HasChanges = true;
		//}

		//#endregion
	}
}
