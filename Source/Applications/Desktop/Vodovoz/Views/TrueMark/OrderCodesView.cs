using Gamma.Binding;
using Gamma.Binding.Converters;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.ColumnConfig;
using Gtk;
using QS.Journal.GtkUI;
using QS.Views.Dialog;
using System;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.TrueMark;
namespace Vodovoz.Views.TrueMark
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderCodesView : DialogViewBase<OrderCodesViewModel>
	{
		//driver popup
		private Menu _driverPopup = new Menu();
		private MenuItem _driverCopySourceCodes = new MenuItem("Копировать исходные коды");
		private MenuItem _driverCopyResultCodes = new MenuItem("Копировать итоговые коды");
		private MenuItem _driverOpenDocument = new MenuItem("Открыть маршрутный лист");
		private MenuItem _driverOpenAuthor = new MenuItem("Открыть автора");

		//warehouse popup
		private Menu _warehousePopup = new Menu();
		private MenuItem _warehouseCopySourceCodes = new MenuItem("Копировать исходные коды");
		private MenuItem _warehouseCopyResultCodes = new MenuItem("Копировать итоговые коды");
		private MenuItem _warehouseOpenDocument = new MenuItem("Открыть талон погрузки");
		private MenuItem _warehouseOpenAuthor = new MenuItem("Открыть автора");

		//selfdelivery popup
		private Menu _selfdeliveryPopup = new Menu();
		private MenuItem _selfdeliveryCopySourceCodes = new MenuItem("Копировать исходные коды");
		private MenuItem _selfdeliveryCopyResultCodes = new MenuItem("Копировать итоговые коды");
		private MenuItem _selfdeliveryOpenDocument = new MenuItem("Открыть самовывоз");
		private MenuItem _selfdeliveryOpenAuthor = new MenuItem("Открыть автора");

		//pool popup
		private Menu _poolPopup = new Menu();
		private MenuItem _poolCopyCodes = new MenuItem("Копировать коды");

		public OrderCodesView(OrderCodesViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ybuttonRefresh.BindCommand(ViewModel.RefreshCommand);

			entryOrder.IsEditable = false;
			entryOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderId, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ylabelTotalCodesValue.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CodesRequired, w => w.LabelProp, new TextToBoldTextConverter())
				.InitializeFromSource();

			ylabelProvidedCodesValue.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CodesProvided, w => w.LabelProp, new TextToBoldTextConverter())
				.InitializeFromSource();

			ylabelScannedCodesValue.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CodesProvidedFromScan, w => w.LabelProp, new TextToBoldTextConverter())
				.InitializeFromSource();

			ylabelPooledCodesValue.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TotalAddedFromPool, w => w.LabelProp, new TextToBoldTextConverter())
				.InitializeFromSource();

			labelPageDriver.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => $"Водитель ({vm.TotalScannedByDriver})", w => w.LabelProp)
				.InitializeFromSource();

			labelPageWarehouse.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => $"Склад ({vm.TotalScannedByWarehouse})", w => w.LabelProp)
				.InitializeFromSource();

			labelPageSelfdelivery.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => $"Самовывоз ({vm.TotalScannedBySelfdelivery})", w => w.LabelProp)
				.InitializeFromSource();

			labelPagePool.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => $"Из пула ({vm.TotalAddedFromPool})", w => w.LabelProp)
				.InitializeFromSource();

			entrySearch.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SearchText, w => w.Text)
				.InitializeFromSource();

			var driverRecursiveConfig = new RecursiveConfig<OrderCodeItemViewModel>(
				x => x.Parent,
				x => x.Children);
			ytreeviewDriver.AfterModelChanged += TreeViewAfterModelChanged;
			ytreeviewDriver.Binding.AddSource(ViewModel)
				.AddFuncBinding(
					vm => new RecursiveTreeModel<OrderCodeItemViewModel>(vm.ScannedByDriverCodes, driverRecursiveConfig), 
					w => w.YTreeModel
				)
				.AddBinding(vm => vm.ScannedByDriverCodesSelected, w => w.SelectedRows, 
					new ArrayToEnumerableConverter<OrderCodeItemViewModel>())
				.InitializeFromSource();
			ytreeviewDriver.Selection.Mode = SelectionMode.Multiple;
			ytreeviewDriver.ColumnsConfig = FluentColumnsConfig<OrderCodeItemViewModel>.Create()
				.AddColumn("Тип")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Type)
					.Editable(false)
				.AddColumn("Исходный код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Итоговый код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.ResultIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Заменен из пула")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.ReplacedFromPool)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Проблема")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.Problem)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("МЛ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.SourceDocumentId)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Сканировал")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodeAuthor)
					.Editable(false)
				.AddColumn("")
				.Finish();
			ytreeviewDriver.Add(_driverPopup);
			_driverPopup.Add(_driverCopySourceCodes);
			_driverPopup.Add(_driverCopyResultCodes);
			_driverPopup.Add(_driverOpenDocument);
			_driverPopup.Add(_driverOpenAuthor);
			_driverCopySourceCodes.Show();
			_driverCopyResultCodes.Show();
			_driverOpenDocument.Show();
			_driverOpenAuthor.Show();
			_driverPopup.Show();
			_driverCopySourceCodes.Activated += (sender, e) => ViewModel.CopyDriverSourceCodesCommand.Execute(null);
			_driverCopyResultCodes.Activated += (sender, e) => ViewModel.CopyDriverResultCodesCommand.Execute(null);
			_driverOpenDocument.Activated += (sender, e) => ViewModel.OpenRouteListCommand.Execute(null);
			_driverOpenAuthor.Activated += (sender, e) => ViewModel.OpenFromDriverAuthorCommand.Execute(null);
			ytreeviewDriver.ButtonReleaseEvent += TableDriverRightClick;
			ytreeviewDriver.WidgetEvent += SuppressRightClickWithManyRowsSelected;

			var warehouseRecursiveConfig = new RecursiveConfig<OrderCodeItemViewModel>(
				x => x.Parent,
				x => x.Children);
			ytreeviewWarehouse.AfterModelChanged += TreeViewAfterModelChanged;
			ytreeviewWarehouse.Binding.AddSource(ViewModel)
				.AddFuncBinding(
					vm => new RecursiveTreeModel<OrderCodeItemViewModel>(vm.ScannedByWarehouseCodes, warehouseRecursiveConfig), 
					w => w.YTreeModel
				)
				.AddBinding(vm => vm.ScannedByWarehouseCodesSelected, w => w.SelectedRows,
					new ArrayToEnumerableConverter<OrderCodeItemViewModel>())
				.InitializeFromSource();
			ytreeviewWarehouse.Selection.Mode = SelectionMode.Multiple;
			ytreeviewWarehouse.ColumnsConfig = FluentColumnsConfig<OrderCodeItemViewModel>.Create()
				.AddColumn("Тип")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Type)
					.Editable(false)
				.AddColumn("Исходный код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Итоговый код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.ResultIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Заменен из пула")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.ReplacedFromPool)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Проблема")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.Problem)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Талон погрузки")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.SourceDocumentId)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Сканировал")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodeAuthor)
					.Editable(false)
				.AddColumn("")
				.Finish();

			ytreeviewWarehouse.Add(_warehousePopup);
			_warehousePopup.Add(_warehouseCopySourceCodes);
			_warehousePopup.Add(_warehouseCopyResultCodes);
			_warehousePopup.Add(_warehouseOpenDocument);
			_warehousePopup.Add(_warehouseOpenAuthor);
			_warehouseCopySourceCodes.Show();
			_warehouseCopyResultCodes.Show();
			_warehouseOpenDocument.Show();
			_warehouseOpenAuthor.Show();
			_warehousePopup.Show();
			_warehouseCopySourceCodes.Activated += (sender, e) => ViewModel.CopyWarehouseSourceCodesCommand.Execute(null);
			_warehouseCopyResultCodes.Activated += (sender, e) => ViewModel.CopyWarehouseResultCodesCommand.Execute(null);
			_warehouseOpenDocument.Activated += (sender, e) => ViewModel.OpenCarLoadDocumentCommand.Execute(null);
			_warehouseOpenAuthor.Activated += (sender, e) => ViewModel.OpenFromWarehouseAuthorCommand.Execute(null);
			ytreeviewWarehouse.ButtonReleaseEvent += TableWarehouseRightClick;
			ytreeviewWarehouse.WidgetEvent += SuppressRightClickWithManyRowsSelected;

			// selfdelivery table
			var selfdeliveryRecursiveConfig = new RecursiveConfig<OrderCodeItemViewModel>(
				x => x.Parent,
				x => x.Children);
			ytreeviewSelfdelivery.AfterModelChanged += TreeViewAfterModelChanged;
			ytreeviewSelfdelivery.Binding.AddSource(ViewModel)
				.AddFuncBinding(
					vm => new RecursiveTreeModel<OrderCodeItemViewModel>(vm.ScannedBySelfdeliveryCodes, selfdeliveryRecursiveConfig), 
					w => w.YTreeModel
				)
				.AddBinding(vm => vm.ScannedBySelfdeliveryCodesSelected, w => w.SelectedRows,
					new ArrayToEnumerableConverter<OrderCodeItemViewModel>())
				.InitializeFromSource();
			ytreeviewSelfdelivery.Selection.Mode = SelectionMode.Multiple;
			ytreeviewSelfdelivery.ColumnsConfig = FluentColumnsConfig<OrderCodeItemViewModel>.Create()
				.AddColumn("Тип")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Type)
					.Editable(false)
				.AddColumn("Исходный код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Итоговый код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.ResultIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Заменен из пула")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.ReplacedFromPool)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Проблема")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.Problem)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Самовывоз")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.SourceDocumentId)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Сканировал")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodeAuthor)
					.Editable(false)
				.AddColumn("")
				.Finish();
			ytreeviewSelfdelivery.Add(_selfdeliveryPopup);
			_selfdeliveryPopup.Add(_selfdeliveryCopySourceCodes);
			_selfdeliveryPopup.Add(_selfdeliveryCopyResultCodes);
			_selfdeliveryPopup.Add(_selfdeliveryOpenDocument);
			_selfdeliveryPopup.Add(_selfdeliveryOpenAuthor);
			_selfdeliveryCopySourceCodes.Show();
			_selfdeliveryCopyResultCodes.Show();
			_selfdeliveryOpenDocument.Show();
			_selfdeliveryOpenAuthor.Show();
			_selfdeliveryPopup.Show();
			_selfdeliveryCopySourceCodes.Activated += (sender, e) => ViewModel.CopySelfdeliverySourceCodesCommand.Execute(null);
			_selfdeliveryCopyResultCodes.Activated += (sender, e) => ViewModel.CopySelfdeliveryResultCodesCommand.Execute(null);
			_selfdeliveryOpenDocument.Activated += (sender, e) => ViewModel.OpenSelfdeliveryDocumentCommand.Execute(null);
			_selfdeliveryOpenAuthor.Activated += (sender, e) => ViewModel.OpenFromSelfdeliveryAuthorCommand.Execute(null);
			ytreeviewSelfdelivery.ButtonReleaseEvent += TableSelfdeliveryRightClick;
			ytreeviewSelfdelivery.WidgetEvent += SuppressRightClickWithManyRowsSelected;

			// pool table
			ytreeviewPool.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AddedFromPoolCodes, w => w.ItemsDataSource)
				.AddBinding(vm => vm.AddedFromPoolCodesSelected, w => w.SelectedRows,
					new ArrayToEnumerableConverter<OrderCodeItemViewModel>())
				.InitializeFromSource();
			ytreeviewPool.Selection.Mode = SelectionMode.Multiple;
			ytreeviewPool.ColumnsConfig = FluentColumnsConfig<OrderCodeItemViewModel>.Create()
				.AddColumn("Код")
					.AddTextRenderer(x => x.ResultIdentificationCode)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Причина не отсканированных кодов")
				.AddTextRenderer(x => x.UnscannedCodesReason)
				.AddColumn("")
				.Finish();
			ytreeviewPool.Add(_poolPopup);
			_poolPopup.Add(_poolCopyCodes);
			_poolCopyCodes.Show();
			_poolPopup.Show();
			_poolCopyCodes.Activated += (sender, e) => ViewModel.CopyPoolCodesCommand.Execute(null);
			ytreeviewPool.ButtonReleaseEvent += OnTablePoolRightClick;
			ytreeviewPool.WidgetEvent += SuppressRightClickWithManyRowsSelected;

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void TreeViewAfterModelChanged(object sender, EventArgs e)
		{
			var treeView = sender as TreeView;
			if(treeView == null)
			{
				return;
			}
			treeView.ExpandAll();
		}

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsValidSearchCodeText))
			{
				HighlightSearchText();
				SetColorForSearchEntry();
			}
		}

		private void SetColorForSearchEntry()
		{
			if(ViewModel.IsValidSearchCodeText == null)
			{
				entrySearch.SetColor(StateType.Normal, GdkColors.PrimaryText);
				entrySearch.SetColor(StateType.Active, GdkColors.PrimaryText);
				return;
			}

			if(ViewModel.IsValidSearchCodeText.Value)
			{
				entrySearch.SetColor(StateType.Normal, GdkColors.SuccessText);
				entrySearch.SetColor(StateType.Active, GdkColors.SuccessText);
			}
			else
			{
				entrySearch.SetColor(StateType.Normal, GdkColors.DangerText);
				entrySearch.SetColor(StateType.Active, GdkColors.DangerText);
			}
		}

		private void HighlightSearchText()
		{
			ytreeviewDriver.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
			ytreeviewWarehouse.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
			ytreeviewSelfdelivery.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
			ytreeviewPool.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
		}

		private void SuppressRightClickWithManyRowsSelected(object o, WidgetEventArgs args)
		{
			var treeView = o as TreeView;
			if(treeView == null)
			{
				args.RetVal = false;
				return;
			}
			var buttonEvent = args.Event as Gdk.EventButton;
			if(buttonEvent == null || buttonEvent.Type != Gdk.EventType.ButtonPress)
			{
				args.RetVal = false;
				return;
			}
			if(buttonEvent.Button == 3 && treeView.Selection.CountSelectedRows() > 1)
			{
				args.RetVal = true;
			}
		}

		private void TableDriverRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_driverCopySourceCodes.Sensitive = ViewModel.CopyDriverSourceCodesCommand.CanExecute(null);
			_driverCopyResultCodes.Sensitive = ViewModel.CopyDriverResultCodesCommand.CanExecute(null);
			_driverOpenDocument.Sensitive = ViewModel.OpenRouteListCommand.CanExecute(null);
			_driverOpenAuthor.Sensitive = ViewModel.OpenFromDriverAuthorCommand.CanExecute(null);
			_driverPopup.Popup();
		}

		private void TableWarehouseRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_warehouseCopySourceCodes.Sensitive = ViewModel.CopyWarehouseSourceCodesCommand.CanExecute(null);
			_warehouseCopyResultCodes.Sensitive = ViewModel.CopyWarehouseResultCodesCommand.CanExecute(null);
			_warehouseOpenDocument.Sensitive = ViewModel.OpenCarLoadDocumentCommand.CanExecute(null);
			_warehouseOpenAuthor.Sensitive = ViewModel.OpenFromWarehouseAuthorCommand.CanExecute(null);
			_warehousePopup.Popup();
		}

		private void TableSelfdeliveryRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_selfdeliveryCopySourceCodes.Sensitive = ViewModel.CopySelfdeliverySourceCodesCommand.CanExecute(null);
			_selfdeliveryCopyResultCodes.Sensitive = ViewModel.CopySelfdeliveryResultCodesCommand.CanExecute(null);
			_selfdeliveryOpenDocument.Sensitive = ViewModel.OpenSelfdeliveryDocumentCommand.CanExecute(null);
			_selfdeliveryOpenAuthor.Sensitive = ViewModel.OpenFromSelfdeliveryAuthorCommand.CanExecute(null);
			_selfdeliveryPopup.Popup();
		}

		private void OnTablePoolRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_poolCopyCodes.Sensitive = ViewModel.CopyPoolCodesCommand.CanExecute(null);
			_poolPopup.Popup();
		}

		protected override void OnDestroyed()
		{
			_driverPopup.Destroy();
			_driverCopySourceCodes.Destroy();
			_driverCopyResultCodes.Destroy();
			_driverOpenDocument.Destroy();
			_driverOpenAuthor.Destroy();
			_warehousePopup.Destroy();
			_warehouseCopySourceCodes.Destroy();
			_warehouseCopyResultCodes.Destroy();
			_warehouseOpenDocument.Destroy();
			_warehouseOpenAuthor.Destroy();
			_selfdeliveryPopup.Destroy();
			_selfdeliveryCopySourceCodes.Destroy();
			_selfdeliveryCopyResultCodes.Destroy();
			_selfdeliveryOpenDocument.Destroy();
			_selfdeliveryOpenAuthor.Destroy();
			_poolPopup.Destroy();
			_poolCopyCodes.Destroy();
			ytreeviewDriver.Destroy();
			ytreeviewWarehouse.Destroy();
			ytreeviewSelfdelivery.Destroy();
			ytreeviewPool.Destroy();

			base.OnDestroyed();
		}
	}
}
