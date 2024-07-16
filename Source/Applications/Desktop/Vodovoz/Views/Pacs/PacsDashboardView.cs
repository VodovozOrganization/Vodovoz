using Gamma.ColumnConfig;
using Gtk;
using QS.Journal.GtkUI;
using QS.ViewModels;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsDashboardView : WidgetViewBase<PacsDashboardViewModel>
	{
		private Widget _detailsWidget;
		private Menu _treeViewCallsPopup;

		public PacsDashboardView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ycheckbuttonShowDisconnected.Binding
				.AddBinding(ViewModel, vm => vm.ShowDisconnectedOperators, w => w.Active)
				.InitializeFromSource();

			labelOperatorsOnWorkshift.Binding
				.AddBinding(ViewModel, vm => vm.OperatorsOnWorkshiftTitle, w => w.Text)
				.InitializeFromSource();

			treeViewOperatorsOnBreak.ColumnsConfig = FluentColumnsConfig<DashboardOperatorOnBreakViewModel>.Create()
				.AddColumn("Имя").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Name)
				.AddColumn("Доб. тел.").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Осталось").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.TimeRemains)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRenderer>((cell, vm) =>
					{
						if(vm.BreakTimeGone)
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							cell.CellBackgroundGdk = GdkColors.PrimaryBG;
						}
					})
				.Finish();
			treeViewOperatorsOnBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnBreak, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeViewOperatorsOnBreak.RowActivated += OnActivateOperatorOnBreakRow;

			treeViewOperatorsOnWorkshift.ColumnsConfig = FluentColumnsConfig<DashboardOperatorViewModel>.Create()
				.AddColumn("#").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Model?.Employee?.Id.ToString() ?? "")
				.AddColumn("Имя").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Name)
				.AddColumn("Доб. тел.").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Статус").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.State)
				.AddColumn("Говорит с").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.ConnectedToCall)
				.AddColumn("№ Смены оператора").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Model?.CurrentState?.WorkShift?.Id.ToString() ?? "")
				.AddColumn("")
				.Finish();
			treeViewOperatorsOnWorkshift.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnWorkshift, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeViewOperatorsOnWorkshift.RowActivated += OnActivateOperatorRow;

			treeViewMissedCalls.ColumnsConfig = FluentColumnsConfig<DashboardMissedCallViewModel>.Create()
				.AddColumn("Время").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Time)
				.AddColumn("Телефон").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Могли принять").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.PossibleOperatorsCount)
				.AddColumn("")
				.Finish();
			treeViewMissedCalls.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MissedCalls, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeViewMissedCalls.RowActivated += OnActivateMissedCallRow;

			treeViewAllCalls.ColumnsConfig = FluentColumnsConfig<DashboardCallViewModel>.Create()
				.AddColumn("Время").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Time)
				.AddColumn("Телефон").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Оператор").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Operator)
				.AddColumn("Статус").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.State)
				.AddColumn("Результат").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Result)
				.AddColumn("")
				.Finish();
			treeViewAllCalls.ButtonReleaseEvent += TreeViewAllCalls_ButtonReleaseEvent;

			treeViewAllCalls.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Calls, w => w.ItemsDataSource)
				.InitializeFromSource();
			treeViewAllCalls.RowActivated += OnActivateCallRow;
			treeViewAllCalls.Vadjustment.Changed += Vadjustment_Changed;

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			hboxHeader.Visible = false;
		}

		private void TreeViewAllCalls_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			if(_treeViewCallsPopup == null)
			{
				_treeViewCallsPopup = new Menu();
				var copyPhoneItem = new MenuItem("Скопировать телефон");
				copyPhoneItem.ButtonPressEvent += CopyPhoneFromCall;
				_treeViewCallsPopup.Add(copyPhoneItem);
				_treeViewCallsPopup.ShowAll();
			}

			_treeViewCallsPopup.Popup();
		}

		private void CopyPhoneFromCall(object o, ButtonPressEventArgs args)
		{
			var selectedItem = treeViewAllCalls.GetSelectedObject<DashboardCallViewModel>();
			if(selectedItem == null)
			{
				return;
			}

			var clipBoard = treeViewAllCalls.GetClipboard(Gdk.Selection.Clipboard);
			clipBoard.Text = selectedItem.Phone;
		}

		private void Vadjustment_Changed(object sender, System.EventArgs e)
		{
			treeViewAllCalls.Vadjustment.Value = 0;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsDashboardViewModel.DetailsViewModel):
					ShowDetails();
					break;
				default:
					break;
			}
		}

		private void OnActivateOperatorOnBreakRow(object o, Gtk.RowActivatedArgs args)
		{
			var activatedRow = (ViewModelBase)treeViewOperatorsOnBreak.YTreeModel.NodeAtPath(args.Path);
			ViewModel.ActivatedRow = activatedRow;
		}

		private void OnActivateOperatorRow(object o, Gtk.RowActivatedArgs args)
		{
			var activatedRow = (ViewModelBase)treeViewOperatorsOnWorkshift.YTreeModel.NodeAtPath(args.Path);
			ViewModel.ActivatedRow = activatedRow;
		}

		private void OnActivateMissedCallRow(object o, Gtk.RowActivatedArgs args)
		{
			var activatedRow = (ViewModelBase)treeViewMissedCalls.YTreeModel.NodeAtPath(args.Path);
			ViewModel.ActivatedRow = activatedRow;
		}

		private void OnActivateCallRow(object o, Gtk.RowActivatedArgs args)
		{
			var activatedRow = (ViewModelBase)treeViewAllCalls.YTreeModel.NodeAtPath(args.Path);
			ViewModel.ActivatedRow = activatedRow;
		}

		private void ShowDetails()
		{
			if(_detailsWidget != null)
			{
				vboxDetails.Remove(_detailsWidget);
				_detailsWidget.Destroy();
			}

			if(ViewModel.DetailsViewModel == null)
			{
				return;
			}

			switch(ViewModel.DetailsViewModel.GetType().Name)
			{
				case nameof(DashboardCallDetailsViewModel):
					_detailsWidget = new CallDetailsView
					{
						ViewModel = (DashboardCallDetailsViewModel)ViewModel.DetailsViewModel
					};
					break;
				case nameof(DashboardMissedCallDetailsViewModel):
					_detailsWidget = new MissedCallDetailsView
					{
						ViewModel = (DashboardMissedCallDetailsViewModel)ViewModel.DetailsViewModel
					};
					break;
				case nameof(DashboardOperatorDetailsViewModel):
					_detailsWidget = new OperatorDetailsView
					{
						ViewModel = (DashboardOperatorDetailsViewModel)ViewModel.DetailsViewModel
					};
					break;
				default:
					break;
			}

			vboxDetails.Add(_detailsWidget);
			Box.BoxChild boxChild = ((Box.BoxChild)(vboxDetails[_detailsWidget]));
			boxChild.Position = 1;
			boxChild.Expand = true;
			boxChild.Fill = true;
			_detailsWidget.Show();
		}
	}
}
