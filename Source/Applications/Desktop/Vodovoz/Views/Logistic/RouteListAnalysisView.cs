﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Navigation;
using QS.Validation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class RouteListAnalysisView : TabViewBase<RouteListAnalysisViewModel>
	{
		private Dictionary<RouteListItemStatus, Pixbuf> statusIcons = new Dictionary<RouteListItemStatus, Pixbuf>();

		public RouteListAnalysisView(RouteListAnalysisViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			table1.Sensitive = false;

			buttonSave.Clicked += (sender, e) => ViewModel.SaveWithClose();
			buttonSave.Sensitive = ViewModel.CanEditRouteList;
			
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			entityVMEntryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(MainClass.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory());
			entityVMEntryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityVMEntryCar.CompletionPopupSetWidth(false);

			entityVMEntryDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			entityVMEntryDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			entityVMEntryForwarder.SetEntityAutocompleteSelectorFactory(ViewModel.ForwarderSelectorFactory);
			entityVMEntryForwarder.Binding.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Subject).InitializeFromSource();

			entityVMEntryLogistician.SetEntityAutocompleteSelectorFactory(ViewModel.LogisticanSelectorFactory);
			entityVMEntryLogistician.Binding.AddBinding(ViewModel.Entity, rl => rl.Logistician, w => w.Subject).InitializeFromSource();

			speccomboShift.ItemsList = ViewModel.DeliveryShifts;
			speccomboShift.Binding.AddBinding(ViewModel.Entity, e => e.Shift, w => w.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date).InitializeFromSource();
			
			ytextviewLogisticianComment.Binding.AddBinding(ViewModel.Entity, e => e.LogisticiansComment, w => w.Buffer.Text).InitializeFromSource();

			ConfigureTreeViewAddresses();
			
			//Заполняем телефоны
			FillPhones();
			
			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			ViewModel.UpdateTreeAddresses += UpdateTreeAddresses;
		}

		private void ConfigureTreeViewAddresses()
		{
			//Заполняем иконки
			var assembly = Assembly.GetAssembly(typeof(MainClass));
			statusIcons.Add(RouteListItemStatus.EnRoute, new Pixbuf(assembly, "Vodovoz.icons.status.car.png"));
			statusIcons.Add(RouteListItemStatus.Completed, new Pixbuf(assembly, "Vodovoz.icons.status.face-smile-grin.png"));
			statusIcons.Add(RouteListItemStatus.Overdue, new Pixbuf(assembly, "Vodovoz.icons.status.face-angry.png"));
			statusIcons.Add(RouteListItemStatus.Canceled, new Pixbuf(assembly, "Vodovoz.icons.status.face-crying.png"));
			statusIcons.Add(RouteListItemStatus.Transfered, new Pixbuf(assembly, "Vodovoz.icons.status.face-uncertain.png"));

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListItem>()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.IndexInRoute + 1)
				.AddColumn("Заказ")
					.AddTextRenderer(n => n.Order.Id.ToString())
				.AddColumn("Адрес")
					.AddTextRenderer(n => n.Order.DeliveryPoint == null ? "Требуется точка доставки" : n.Order.DeliveryPoint.ShortAddress)
				.AddColumn("Время")
					.AddTextRenderer(n => n.Order.DeliverySchedule == null ? "" : n.Order.DeliverySchedule.Name)
				.AddColumn("Статус")
					.AddPixbufRenderer(x => statusIcons[x.Status])
					.AddEnumRenderer(n => n.Status, excludeItems: new Enum[] { RouteListItemStatus.Transfered })
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.Order.IsFastDelivery).Editing(false)
				.AddColumn("Статус изменен")
					.AddTextRenderer(n => 
						n.StatusLastUpdate.HasValue ? 
							(n.StatusLastUpdate.Value.Date == DateTime.Today ? 
								n.StatusLastUpdate.Value.ToShortTimeString() : n.StatusLastUpdate.Value.ToString()) 
							: string.Empty)
				.AddColumn("Опоздание")
					.AddTextRenderer(n => n.CalculateTimeLateArrival() != null ? 
						n.CalculateTimeLateArrival().ToString() : "-")
					.XAlign(0.5f)
				.AddColumn("Причина")
					.AddComboRenderer(n => n.LateArrivalReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<LateArrivalReason>().ToList())
					.Editing()
				.AddColumn("Автор причины")
					.AddTextRenderer(n => n.LateArrivalReasonAuthor != null ? 
						n.LateArrivalReasonAuthor.ShortName : string.Empty)
				.AddColumn("Штрафы")
					.AddTextRenderer(n => n.GetAllFines())
				.AddColumn("Комментарий")
					.AddTextRenderer(n => n.CommentForFine)
					.Editable()
					.EditedEvent(OnCommentForFineEdited)
				.AddColumn("Автор комментария")
					.AddTextRenderer(n => n.CommentForFineAuthor != null ?
						n.CommentForFineAuthor.ShortName : String.Empty)
				.AddColumn("Переносы")
					.AddTextRenderer(n => n.GetTransferText(n))
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => {

						switch(n.Status) {
							case RouteListItemStatus.Overdue:
								c.CellBackgroundGdk = new Color(0xee, 0x66, 0x66);
								break;
							case RouteListItemStatus.Completed:
								c.CellBackgroundGdk = new Color(0x66, 0xee, 0x66);
								break;
							case RouteListItemStatus.Canceled:
								c.CellBackgroundGdk = new Color(0xaf, 0xaf, 0xaf);
								break;
							default:
								c.CellBackgroundGdk = new Color(0xff, 0xff, 0xff);
								break;
						}
					})
				.Finish();

			if(ViewModel.CanEditRouteList)
			{
				ytreeviewAddresses.ButtonReleaseEvent += OnYtreeviewAddressesButtonReleaseEvent;
			}

			ytreeviewAddresses.ItemsDataSource = ViewModel.Entity.ObservableAddresses;
			ytreeviewAddresses.Selection.Changed += OnYtreeviewAddressesSelectionChanged;
			ytreeviewAddresses.RowActivated += OnYtreeviewAddressesRowActivated;
		}

		private void FillPhones()
		{
			string phones = null;
			if(ViewModel.Entity.Driver != null && ViewModel.Entity.Driver.Phones.Count > 0) {
				phones =
					$"<b>Водитель {ViewModel.Entity.Driver.FullName}:</b>\n{string.Join("\n", ViewModel.Entity.Driver.Phones)}";
			}
			if(ViewModel.Entity.Forwarder != null && ViewModel.Entity.Forwarder.Phones.Count > 0) {
				if(!string.IsNullOrWhiteSpace(phones))
					phones += "\n";
				phones +=
					$"<b>Экспедитор {ViewModel.Entity.Forwarder.FullName}:</b>\n{string.Join("\n", ViewModel.Entity.Forwarder.Phones)}";
			}

			if(string.IsNullOrWhiteSpace(phones))
				phones = "Нет телефонов";
			labelPhonesInfo.Markup = phones;
		}
		
		private void OnCommentForFineEdited(object o, EditedArgs args)
		{
			if(!string.IsNullOrEmpty(args.NewText))
				ViewModel.SelectedItem.CommentForFineAuthor = ViewModel.CurrentEmployee;
		}

		private void OnYtreeviewAddressesSelectionChanged(object sender, EventArgs e) => 
			ViewModel.SelectedItem = ytreeviewAddresses.GetSelectedObject<RouteListItem>();

		void OnYtreeviewAddressesButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
				ConfigureMenu();
		}

		void OnYtreeviewAddressesRowActivated(object o, RowActivatedArgs args) => 
			ViewModel.OpenOrderCommand.Execute();
		
		void ConfigureMenu()
		{
			if(ViewModel.SelectedItem == null)
				return;
				
			var menu = new Menu();
			
			var openOrder = new MenuItem($"Открыть заказ №{ViewModel.SelectedItem.Order.Id}");
			openOrder.Activated += (s, args) => ViewModel.OpenOrderCommand.Execute();
			menu.Add(openOrder);
			
			var openNotDeliveredOrder = new MenuItem($"Открыть недовоз");
			openNotDeliveredOrder.Activated += (s, args) => ViewModel.OpenUndeliveredOrderCommand.Execute();
			openNotDeliveredOrder.Sensitive = 
				ViewModel.SelectedItem.Status == RouteListItemStatus.Canceled
				&& ViewModel.UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(ViewModel.UoW, ViewModel.SelectedItem.Order.Id).Any();
			menu.Add(openNotDeliveredOrder);
			
			var createFine = new MenuItem($"Создать штраф");
			createFine.Activated += (s, args) => ViewModel.CreateFineCommand.Execute();
			menu.Add(createFine);
			
			var attachFine = new MenuItem($"Прикрепить штраф");
			attachFine.Activated += (s, args) => ViewModel.AttachFineCommand.Execute();
			menu.Add(attachFine);

			var detachAllFines = new MenuItem("Открепить все штрафы");
			detachAllFines.Activated += (s, args) => ViewModel.DetachAllFinesCommand.Execute();
			detachAllFines.Sensitive = ViewModel.SelectedItem.Fines.Any();

			var detachFine = new MenuItem("Открепить определенный штраф");
			detachFine.Activated += (s, args) => ViewModel.DetachFineCommand.Execute();
			detachFine.Sensitive = ViewModel.SelectedItem.Fines.Any();
			
			menu.Add(detachAllFines);
			menu.Add(detachFine);

			menu.ShowAll();
			menu.Popup();
		}

		private void UpdateBottlesSummaryInfo() => labelBottleInfo.Markup = ViewModel.UpdateBottlesSummaryInfo();

		private void UpdateTreeAddresses() => ytreeviewAddresses.YTreeModel.EmitModelChanged();
	}
}
