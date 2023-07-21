using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Journal.GtkUI;
using QS.Project.Services;
using QS.Views.GtkUI;
using System;
using System.Text;
using QS.Dialog.Gtk;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Widgets;
using CurrencyWorks = QSProjectsLib.CurrencyWorks;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrderView : WidgetViewBase<UndeliveredOrderViewModel>
	{
		private Menu _popupCopyCommentsMenu;

		public UndeliveredOrderView(UndeliveredOrderViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		public void ConfigureDlg()
		{
			if(ViewModel.UoW.IsNew)
			{
				Sensitive = false;
			}

			lblInfo.Binding.AddBinding(ViewModel, vm => vm.Info, w => w.LabelProp).InitializeFromSource();

			ViewModel.RemoveItemsFromStatusEnumAction = () => RemoveItemsFromEnums();
            ViewModel.CreateOrderAction = () => CreateNewOrder();

            #region HasPermissionOrNew

            //основные поля доступны если есть разрешение или это новый недовоз,
            //выбран старый заказ и статус недовоза не "Закрыт"

            yEnumCMBDriverCallPlace.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder != null
					      && vm.HasPermissionOrNew
					      && vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
					w => w.Sensitive)
				.InitializeFromSource();

			yDateDriverCallTime.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder != null
					      && vm.HasPermissionOrNew
					      && vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
					w => w.Sensitive)
				.InitializeFromSource();

			yDateDispatcherCallTime.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder != null
					      && vm.HasPermissionOrNew
					      && vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
					w => w.Sensitive)
				.InitializeFromSource();

			evmeRegisteredBy.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder != null
					      && vm.HasPermissionOrNew
					      && vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
					w => w.Sensitive)
				.InitializeFromSource();

			vbxReasonAndFines.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder != null
					      && vm.HasPermissionOrNew
					      && vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
					w => w.Sensitive)
				.InitializeFromSource();

			#endregion

			//выбор старого заказа доступен, если есть разрешение или это новый недовоз и не выбран старый заказ
			hbxUndelivery.Binding.AddFuncBinding(ViewModel,
					vm => vm.Entity.OldOrder == null
					      && vm.HasPermissionOrNew,
					w => w.Sensitive)
				.InitializeFromSource();

			//можем менять статус, если есть права или нет прав и статус не "закрыт"
			hbxStatus.Binding.AddFuncBinding(ViewModel,
					vm => (vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed
					       || vm.CanCloseUndeliveries)
					      && vm.Entity.OldOrder != null,
					w => w.Sensitive)
				.InitializeFromSource();

			//кнопки для выбора/создания нового заказа и группа "В работе у отдела"
			//доступны всегда, если статус недовоза не "Закрыт"
			hbxInProcessAtDepartment.Binding.AddFuncBinding(ViewModel.Entity,
				e => e.UndeliveryStatus != UndeliveryStatus.Closed,
				w => w.Sensitive).InitializeFromSource();

			hbxForNewOrder.Binding.AddFuncBinding(ViewModel,
				vm => vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
				w => w.Sensitive).InitializeFromSource();

			#region Result Comments Controls Sensitive;

			ytreeviewResult.Binding.AddBinding(ViewModel,
				vm => vm.CanEditUndeliveries,
				w => w.Sensitive).InitializeFromSource();

			ytextviewNewResult.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditUndeliveries, w => w.Sensitive)
				.AddBinding(vm => vm.NewResultText, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonAddResult.Binding.AddFuncBinding(ViewModel,
				vm => vm.CanEditUndeliveries
				      && !string.IsNullOrWhiteSpace(vm.NewResultText),
				w => w.Sensitive).InitializeFromSource();

			#endregion

			#region Visibilites

			lblDriverCallPlace.Binding.AddFuncBinding(ViewModel, vm => !vm.RouteListDoesNotExist, w => w.Visible).InitializeFromSource();
			yEnumCMBDriverCallPlace.Binding.AddFuncBinding(ViewModel, vm => !vm.RouteListDoesNotExist, w => w.Visible)
				.InitializeFromSource();
			lblDriverCallTime.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.DriverCallType != DriverCallType.NoCall, w => w.Visible)
				.InitializeFromSource();
			yDateDriverCallTime.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.DriverCallType != DriverCallType.NoCall, w => w.Visible)
				.InitializeFromSource();
			btnChooseOrder.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.NewOrder == null, w => w.Visible).InitializeFromSource();
			lblTransferDate.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.NewOrder != null, w => w.Visible).InitializeFromSource();

			#endregion


			yTreeFines.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();

			evmeOldUndeliveredOrder.SetEntityAutocompleteSelectorFactory(ViewModel.OrderSelector);
			evmeOldUndeliveredOrder.Binding
				.AddBinding(ViewModel.Entity, e => e.OldOrder, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEditReference, w => w.CanEditReference)
				.InitializeFromSource();

			evmeOldUndeliveredOrder.Changed += OnUndeliveredOrderChanged;

			yDateDriverCallTime.Binding.AddBinding(ViewModel.Entity, t => t.DriverCallTime, w => w.DateOrNull).InitializeFromSource();

			yEnumCMBDriverCallPlace.ItemsEnum = typeof(DriverCallType);
			yEnumCMBDriverCallPlace.Binding.AddBinding(ViewModel.Entity, p => p.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			yDateDispatcherCallTime.Binding.AddBinding(ViewModel.Entity, t => t.DispatcherCallTime, w => w.DateOrNull)
				.InitializeFromSource();
		
			entryNewDeliverySchedule.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryScheduleJournalFactory);
			entryNewDeliverySchedule.Binding
				.AddBinding(ViewModel.Entity, s => s.NewDeliverySchedule, w => w.Subject)
				.InitializeFromSource();
			entryNewDeliverySchedule.Sensitive = false;

			lblTransferDate.Binding.AddBinding(ViewModel, vm => vm.TransferText, w => w.Text).InitializeFromSource();

			btnNewOrder.Binding.AddBinding(ViewModel, vm => vm.NewOrderText, w => w.Label).InitializeFromSource();

			yEnumCMBStatus.ItemsEnum = typeof(UndeliveryStatus);
			yEnumCMBStatus.Binding.AddBinding(ViewModel.Entity, e => e.UndeliveryStatus, w => w.SelectedItem).InitializeFromSource();

			yentInProcessAtDepartment.SubjectType = typeof(Subdivision);
			yentInProcessAtDepartment.Binding.AddBinding(ViewModel.Entity, d => d.InProcessAtDepartment, w => w.Subject)
				.InitializeFromSource();
			yentInProcessAtDepartment.ChangedByUser += (s, e) => { ViewModel.AddCommentToTheField(); };

			evmeRegisteredBy.SetEntityAutocompleteSelectorFactory(ViewModel.WorkingEmployeeAutocompleteSelectorFactory);
			evmeRegisteredBy.Binding.AddBinding(ViewModel.Entity, s => s.EmployeeRegistrator, w => w.Subject).InitializeFromSource();

			txtReason.Binding.AddBinding(ViewModel.Entity, u => u.Reason, w => w.Buffer.Text).InitializeFromSource();

			yenumcomboboxTransferType.ItemsEnum = typeof(TransferType);
			yenumcomboboxTransferType.Binding.AddBinding(ViewModel.Entity, u => u.OrderTransferType, w => w.SelectedItemOrNull);
			yenumcomboboxTransferType.Binding.AddFuncBinding(ViewModel.Entity, u => u.NewOrder != null, w => w.Visible)
				.InitializeFromSource();

			comboTransferAbsenceReason.SetRenderTextFunc<UndeliveryTransferAbsenceReason>(u => u.Name);
			comboTransferAbsenceReason.Binding.AddBinding(ViewModel, vm => vm.UndeliveryTransferAbsenceReasonItems, w => w.ItemsList)
				.InitializeFromSource();
			comboTransferAbsenceReason.Binding.AddBinding(ViewModel.Entity, u => u.UndeliveryTransferAbsenceReason, w => w.SelectedItem)
				.InitializeFromSource();
			comboTransferAbsenceReason.Sensitive = ViewModel.CanChangeProblemSource;

			yTreeFines.ColumnsConfig = ColumnsConfigFactory.Create<FineItem>()
				.AddColumn("Номер").AddTextRenderer(node => node.Fine.Id.ToString())
				.AddColumn("Сотудники").AddTextRenderer(node => node.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Money))
				.Finish();

			cmbUndeliveryKind.SetRenderTextFunc<UndeliveryKind>(k => k.GetFullName);
			cmbUndeliveryKind.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryKindSource, w => w.ItemsList)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.UndeliveryKind, w => w.SelectedItem)
				.InitializeFromSource();

			cmbUndeliveryObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryObjectSource, w => w.ItemsList)
				.AddBinding(vm => vm.UndeliveryObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			evmeUndeliveryDetalization.SetEntityAutocompleteSelectorFactory(ViewModel.UndeliveryDetalizationSelectorFactory);
			evmeUndeliveryDetalization.Binding
				.AddBinding(ViewModel.Entity, u => u.UndeliveryDetalization, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanChangeDetalization, w => w.Sensitive)
				.InitializeFromSource();

			entryUndeliveryDetalization.Visible = false;

			SetResultCommentsControlsSettings();

			guiltyInUndeliveryView.ConfigureWidget(ViewModel.UoW, ViewModel.Entity, !ViewModel.RouteListDoesNotExist);

			Application.Invoke((s, arg) =>
			{
				yDateDriverCallTime.Binding.RefreshFromSource();
				yenumcomboboxTransferType.Binding.RefreshFromSource();
				lblDriverCallTime.Binding.RefreshFromSource();
				lblTransferDate.Binding.RefreshFromSource();
				btnChooseOrder.Binding.RefreshFromSource();
			});
		}

		private void SetResultCommentsControlsSettings()
		{
			_popupCopyCommentsMenu = new Menu();
			MenuItem copyCommentsMenuEntry = new MenuItem("Копировать");
			copyCommentsMenuEntry.ButtonPressEvent += (s, e) =>
			{
				StringBuilder stringBuilder = new StringBuilder();

				foreach(UndeliveredOrderResultComment selected in ytreeviewResult.SelectedRows)
				{
					stringBuilder.AppendLine(selected.Comment);
				}

				GetClipboard(null).Text = stringBuilder.ToString();
			};
			copyCommentsMenuEntry.Visible = true;
			_popupCopyCommentsMenu.Add(copyCommentsMenuEntry);

			ytreeviewResult.Selection.Mode = SelectionMode.Multiple;
			ytreeviewResult.ColumnsConfig = FluentColumnsConfig<UndeliveredOrderResultComment>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Author.FullName)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Comment)
						.WrapWidth(250)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>((c, o) => c.CellBackgroundGdk = new Gdk.Color(230, 230, 245))
				.Finish();

			ytreeviewResult.ItemsDataSource = ViewModel.Entity.ObservableResultComments;
			ytreeviewResult.ButtonReleaseEvent += (s, e) =>
			{
				if(e.Event.Button != (uint)GtkMouseButton.Right)
				{
					return;
				}

				_popupCopyCommentsMenu.Show();

				if(_popupCopyCommentsMenu.Children.Length == 0)
				{
					return;
				}

				_popupCopyCommentsMenu.Popup();
			};

			ybuttonAddResult.Clicked += OnButtonAddResultClicked;

			ybuttonAddResult.Binding
				.AddFuncBinding(this, e => !string.IsNullOrWhiteSpace(ytextviewNewResult.Buffer.Text), b => b.Sensitive)
				.InitializeFromSource();
		}

		private void OnButtonAddResultClicked(object sender, EventArgs e)
		{
			ViewModel.OnAddResult();
		}

		void RemoveItemsFromEnums()
		{
			//удаляем статус "закрыт" из списка, если недовоз не закрыт и нет прав на их закрытие
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_undeliveries") && ViewModel.Entity.UndeliveryStatus != UndeliveryStatus.Closed)
			{
				yEnumCMBStatus.AddEnumToHideList(new Enum[] { UndeliveryStatus.Closed });
				yEnumCMBStatus.SelectedItem = (UndeliveryStatus)ViewModel.Entity.UndeliveryStatus;
			}
		}

		protected void OnBtnNewOrderClicked(object sender, EventArgs e)
		{
			ViewModel.OnNewOrder();
		}

		protected void OnBtnChooseOrderClicked(object sender, EventArgs e)
		{
			ViewModel.OnChooseOrder();
		}

		protected void OnButtonAddFineClicked(object sender, EventArgs e)
		{
			ViewModel.OnAddFine();
		}

		private void OnUndeliveredOrderChanged(object sender, EventArgs e)
		{
			Sensitive = true;
			guiltyInUndeliveryView.ConfigureWidget(ViewModel.UoW, ViewModel.Entity, !ViewModel.RouteListDoesNotExist);
		}

		/// <summary>
		/// Создаёт новый заказ, копируя поля существующего.
		/// </summary>
		/// <param name="order">Заказ, из которого копируются свойства.</param>
		protected void CreateNewOrder()
		{
			var order = ViewModel.Entity.OldOrder;
			var dlg = new OrderDlg();
			dlg.CopyOrderFrom(order.Id);

			ViewModel.Tab.TabParent.OpenTab(
                DialogHelper.GenerateDialogHashName<Order>(dlg.Entity.Id),
                () => dlg
            );

            dlg.TabClosed += (sender, e) => {
				if(sender is OrderDlg)
				{
					Order o = (sender as OrderDlg).Entity;
					if(o.Id > 0)
					{
						ViewModel.Entity.NewOrder = o;
						ViewModel.Entity.NewDeliverySchedule = o.DeliverySchedule;
					}
				}
			};
		}

        public override void Dispose()
		{
			ytreeviewResult?.Destroy();
			yTreeFines?.Destroy();

			base.Dispose();
		}
	}
}
