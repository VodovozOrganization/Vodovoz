﻿using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

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
			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(ViewModel.CarSelectorFactory);
			entityviewmodelentryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			evmeDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, widget => widget.Subject).InitializeFromSource();

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

			yspinConfirmedDistance.Binding.AddBinding(ViewModel.Entity, e => e.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();

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

			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = MainClass.MainWin.MangoManager;
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
		}

		private Gdk.Color GetRowColorByStatus(RouteListItemStatus routeListItemStatus)
		{
			switch(routeListItemStatus)
			{
				case RouteListItemStatus.Overdue:
					return new Gdk.Color(0xee, 0x66, 0x66);
				case RouteListItemStatus.Completed:
					return new Gdk.Color(0x66, 0xee, 0x66);
				case RouteListItemStatus.Canceled:
					return new Gdk.Color(0xaf, 0xaf, 0xaf);
				default:
					return new Gdk.Color(0xff, 0xff, 0xff);
			}
		}
	}
}
