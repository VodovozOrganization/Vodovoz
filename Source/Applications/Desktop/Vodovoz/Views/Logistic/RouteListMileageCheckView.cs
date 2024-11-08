using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Logistic
{
	public partial class RouteListMileageCheckView : TabViewBase<RouteListMileageCheckViewModel>
	{
		public RouteListMileageCheckView(RouteListMileageCheckViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, widget => widget.Subject).InitializeFromSource();
			evmeDriver.Changed += (s, e) => ViewModel.CheckDriversRouteListsDebtCommand.Execute();

			evmeForwarder.SetEntityAutocompleteSelectorFactory(ViewModel.ForwarderSelectorFactory);
			evmeForwarder.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Forwarder, widget => widget.Subject)
				.AddBinding( e => e.CanAddForwarder, widget => widget.Sensitive)
				.InitializeFromSource();

			evmeLogistician.SetEntityAutocompleteSelectorFactory(ViewModel.LogisticianSelectorFactory);
			evmeLogistician.Binding.AddBinding(ViewModel.Entity, e => e.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = ViewModel.DeliveryShifts;
			speccomboShift.Binding.AddBinding(ViewModel.Entity, e => e.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(ViewModel.Entity, e => e.Date, widget => widget.Date).InitializeFromSource();

			yspinConfirmedDistance.Adjustment.Upper = (double)RouteList.ConfirmedDistanceLimit;
			yspinConfirmedDistance.Binding
				.AddBinding(ViewModel.Entity, e => e.ConfirmedDistance, widget => widget.ValueAsDecimal)
				.InitializeFromSource();

			yentryRecalculatedDistance.Binding.AddBinding(ViewModel.Entity, e => e.RecalculatedDistance, widget => widget.Text, new DecimalToStringConverter()).InitializeFromSource();

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingNode>()
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
				.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = GetRowColorByStatus(node.Status))
				.Finish();

			ytreeviewAddresses.ItemsDataSource = ViewModel.RouteListItems;			

			ytextviewMileageComment.Binding.AddBinding(ViewModel.Entity, e => e.MileageComment, w => w.Buffer.Text).InitializeFromSource();

			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = Startup.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(ViewModel.Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			ybuttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ytableMain.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yhboxMileageComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ytreeviewAddresses.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yhboxBottom.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAccept.Binding.AddFuncBinding(ViewModel, vm => vm.IsAcceptAvailable, w => w.Sensitive).InitializeFromSource();
			ybuttonAccept.Clicked += (s, e) => ViewModel.AcceptCommand.Execute();

			buttonOpenMap.Clicked += (sender, e) => ViewModel.OpenMapCommand.Execute();
			buttonFromTrack.Clicked += (sender, e) => ViewModel.FromTrackCommand.Execute();
			buttonAcceptFine.Clicked += (sender, e) => ViewModel.AcceptFineCommand.Execute();
			buttonMileageDistribution.Clicked += (sender, e) => ViewModel.DistributeMileageCommand.Execute();

			ybuttonSave.Clicked += (sender, e) => ViewModel.SaveWithClose();
			ybuttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			var deliveryFreeBalanceViewModel = new DeliveryFreeBalanceViewModel();
			var deliveryfreebalanceview = new DeliveryFreeBalanceView(deliveryFreeBalanceViewModel);
			deliveryfreebalanceview.Binding
				.AddBinding(ViewModel.Entity, e => e.ObservableDeliveryFreeBalanceOperations, w => w.ObservableDeliveryFreeBalanceOperations)
				.InitializeFromSource();
			deliveryfreebalanceview.ShowAll();
			yhboxDeliveryFreeBalance.PackStart(deliveryfreebalanceview, true, true, 0);

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}

		private Gdk.Color GetRowColorByStatus(RouteListItemStatus routeListItemStatus)
		{
			switch(routeListItemStatus)
			{
				case RouteListItemStatus.Overdue:
					return GdkColors.DangerBase;
				case RouteListItemStatus.Completed:
					return GdkColors.SuccessBase;
				case RouteListItemStatus.Canceled:
					return GdkColors.InsensitiveBase;
				default:
					return GdkColors.PrimaryBase;
			}
		}
	}
}
