using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Journal.GtkUI;
using QS.Project.Services;
using QS.Views.GtkUI;
using System;
using System.Text;
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
			ViewModel.RemoveItemsFromStatusEnumAction = () => RemoveItemsFromEnums();

			lblInfo.Binding.AddBinding(ViewModel, vm => vm.Info, w => w.LabelProp).InitializeFromSource();

			#region Has permission or new

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

			tblUndeliveryFields.Binding.AddFuncBinding(ViewModel, 
				vm => vm.Entity.OldOrder != null 
				      && vm.HasPermissionOrNew,
				w => w.Sensitive).InitializeFromSource();

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
				e => e.UndeliveryStatus != UndeliveryStatus.Closed && e.OldOrder != null,
				w => w.Sensitive).InitializeFromSource();

			hbxForNewOrder.Binding.AddFuncBinding(ViewModel,
				vm => vm.Entity.UndeliveryStatus != UndeliveryStatus.Closed,
				w => w.Sensitive).InitializeFromSource();


			#region Result comments controls sensitive;

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

			#region Driver visibilites

			lblDriverCallPlace.Binding.AddFuncBinding(ViewModel, vm => !vm.RouteListDoesNotExist, w => w.Visible).InitializeFromSource();
			yEnumCMBDriverCallPlace.Binding.AddFuncBinding(ViewModel, vm => !vm.RouteListDoesNotExist, w => w.Visible).InitializeFromSource();
			lblDriverCallTime.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.DriverCallType != DriverCallType.NoCall, w => w.Visible).InitializeFromSource();
			yDateDriverCallTime.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.DriverCallType != DriverCallType.NoCall, w => w.Visible).InitializeFromSource();
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

			yDateDispatcherCallTime.Binding.AddBinding(ViewModel.Entity, t => t.DispatcherCallTime, w => w.DateOrNull).InitializeFromSource();
		
			entryNewDeliverySchedule.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryScheduleJournalFactory);
			entryNewDeliverySchedule.Binding.AddBinding(ViewModel.Entity, s => s.NewDeliverySchedule, w => w.Subject).InitializeFromSource();
			entryNewDeliverySchedule.Sensitive = false;

			lblTransferDate.Binding.AddBinding(ViewModel, vm => vm.TransferText, w => w.Text).InitializeFromSource();

			btnNewOrder.Binding.AddBinding(ViewModel, vm => vm.NewOrderText, w => w.Label).InitializeFromSource();

			yEnumCMBStatus.ItemsEnum = typeof(UndeliveryStatus);
			yEnumCMBStatus.Binding.AddBinding(ViewModel.Entity, e => e.UndeliveryStatus, w => w.SelectedItem).InitializeFromSource();
			ViewModel.RemoveItemsFromStatusEnumAction?.Invoke();

			yentInProcessAtDepartment.SubjectType = typeof(Subdivision);
			yentInProcessAtDepartment.Binding.AddBinding(ViewModel.Entity, d => d.InProcessAtDepartment, w => w.Subject).InitializeFromSource();
			yentInProcessAtDepartment.ChangedByUser += OnYentInProcessAtDepartmentChangedByUser;

			evmeRegisteredBy.SetEntityAutocompleteSelectorFactory(ViewModel.WorkingEmployeeAutocompleteSelectorFactory);
			evmeRegisteredBy.Binding.AddBinding(ViewModel.Entity, s => s.EmployeeRegistrator, w => w.Subject).InitializeFromSource();

			txtReason.Binding.AddBinding(ViewModel.Entity, u => u.Reason, w => w.Buffer.Text).InitializeFromSource();

			yenumcomboboxTransferType.ItemsEnum = typeof(TransferType);
			yenumcomboboxTransferType.Binding.AddBinding(ViewModel.Entity, u => u.OrderTransferType, w => w.SelectedItemOrNull);
			yenumcomboboxTransferType.Binding.AddFuncBinding(ViewModel.Entity, u => u.NewOrder != null, w => w.Visible).InitializeFromSource();

			comboTransferAbsenceReason.SetRenderTextFunc<UndeliveryTransferAbsenceReason>(u => u.Name);
			comboTransferAbsenceReason.Binding
				.AddBinding(ViewModel, vm => vm.UndeliveryTransferAbsenceReasonItems, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, u => u.UndeliveryTransferAbsenceReason, w => w.SelectedItem)
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
				.AddBinding(vm => vm.CanChangeUndeliveryKind, w => w.Sensitive)
				.AddBinding(vm => vm.UndeliveryKind, w => w.SelectedItem)
				.InitializeFromSource();

			cmbUndeliveryObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UndeliveryObjectSource, w => w.ItemsList)
				.AddBinding(vm => vm.UndeliveryObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			cmbUndeliveryObject.Changed += OnDetalizationParentObjectChanged;

			evmeUndeliveryDetalization.SetEntityAutocompleteSelectorFactory(ViewModel.UndeliveryDetalizationSelectorFactory);
			evmeUndeliveryDetalization.Binding
				.AddBinding(ViewModel.Entity, e => e.UndeliveryDetalization, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanChangeDetalization, w => w.Sensitive)
				.InitializeFromSource();

			SetResultCommentsControlsSettings();

			guiltyInUndeliveryView.ConfigureWidget(ViewModel.UoW, ViewModel.Entity, !ViewModel.RouteListDoesNotExist);
		}

		private void OnDetalizationParentObjectChanged(object sender, EventArgs e)
		{
			ViewModel.ClearDetalizationCommand.Execute();
		}

		private void OnYentInProcessAtDepartmentChangedByUser(object sender, EventArgs e)
		{
			ViewModel.AddCommentToTheFieldCommand.Execute();
		}

		private void SetResultCommentsControlsSettings()
		{
			_popupCopyCommentsMenu = new Menu();
			MenuItem copyCommentsMenuEntry = new MenuItem("Копировать");
			copyCommentsMenuEntry.ButtonPressEvent += CopyCommentsMenuEntryOnButtonPressEvent;
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
			ytreeviewResult.ButtonReleaseEvent += OnYtreeviewResultButtonReleaseEvent;

			ybuttonAddResult.Clicked += OnButtonAddResultClicked;

			ybuttonAddResult.Binding
				.AddFuncBinding(this, e => !string.IsNullOrWhiteSpace(ytextviewNewResult.Buffer.Text), b => b.Sensitive)
				.InitializeFromSource();
		}

		private void OnYtreeviewResultButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_popupCopyCommentsMenu.Show();

			if(_popupCopyCommentsMenu.Children.Length == 0)
			{
				return;
			}

			_popupCopyCommentsMenu.Popup();
		}

		private void CopyCommentsMenuEntryOnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach(UndeliveredOrderResultComment selected in ytreeviewResult.SelectedRows)
			{
				stringBuilder.AppendLine(selected.Comment);
			}

			GetClipboard(null).Text = stringBuilder.ToString();
		}

		private void OnButtonAddResultClicked(object sender, EventArgs e)
		{
			ViewModel.AddResultCommand.Execute();
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
			ViewModel.NewOrderCommand.Execute();
		}

		protected void OnBtnChooseOrderClicked(object sender, EventArgs e)
		{
			ViewModel.ChooseOrderCommand.Execute();
		}

		protected void OnButtonAddFineClicked(object sender, EventArgs e)
		{
			ViewModel.AddFineCommand.Execute();
		}

		private void OnUndeliveredOrderChanged(object sender, EventArgs e)
		{
			guiltyInUndeliveryView.ConfigureWidget(ViewModel.UoW, ViewModel.Entity, !ViewModel.RouteListDoesNotExist);
		}

		protected override void OnShown()
		{
			base.OnShown();
			yDateDriverCallTime.Binding.RefreshFromSource();
			yenumcomboboxTransferType.Binding.RefreshFromSource();
			lblDriverCallTime.Binding.RefreshFromSource();
			lblTransferDate.Binding.RefreshFromSource();
			btnChooseOrder.Binding.RefreshFromSource();
		}

		public override void Dispose()
		{
			yentInProcessAtDepartment.ChangedByUser -= OnYentInProcessAtDepartmentChangedByUser;
			evmeOldUndeliveredOrder.Changed -= OnUndeliveredOrderChanged;
			ytreeviewResult.ButtonReleaseEvent -= OnYtreeviewResultButtonReleaseEvent;
			cmbUndeliveryObject.Changed -= OnDetalizationParentObjectChanged;
			ytreeviewResult?.Destroy();
			yTreeFines?.Destroy();

			base.Dispose();
		}
	}
}
