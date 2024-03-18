using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;

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

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

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

		private void ConfigureTreeViewAddresses()
		{
			//Заполняем иконки
			var assembly = Assembly.GetAssembly(typeof(Startup));
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
				.AddColumn("Ожидает до")
					.AddTimeRenderer(n => n.Order.WaitUntilTime)
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
					.AddTextRenderer(n => n.GetTransferText(false))
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => {

						switch(n.Status) {
							case RouteListItemStatus.Overdue:
								c.CellBackgroundGdk = GdkColors.DangerBase;
								break;
							case RouteListItemStatus.Completed:
								c.CellBackgroundGdk = GdkColors.SuccessBase;
								break;
							case RouteListItemStatus.Canceled:
								c.CellBackgroundGdk = GdkColors.InsensitiveBase;
								break;
							default:
								c.CellBackgroundGdk = GdkColors.PrimaryBase;
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
