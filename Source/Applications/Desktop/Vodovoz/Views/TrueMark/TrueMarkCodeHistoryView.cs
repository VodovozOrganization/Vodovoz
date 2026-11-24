using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz.Views.TrueMark
{
	/// <summary>
	/// Представление диалога поиска и истории кода маркировки.
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TrueMarkCodeHistoryView : DialogViewBase<TrueMarkCodeHistoryViewModel>
	{
		private Menu _historyPopup;
		private MenuItem _openOrderMenuItem;
		private MenuItem _openOrderCodesMenuItem;
		private MenuItem _openEdoDialogMenuItem;
		private MenuItem _openEdoTaskMenuItem;
		private MenuItem _openEdoProcessMenuItem;
		private MenuItem _openSourceDocumentMenuItem;
		private MenuItem _copyCodeMenuItem;
		private MenuItem _copyHistoryChildCodesMenuItem;
		private Menu _trueMarkInfoPopup;
		private MenuItem _copyInfoCodeMenuItem;
		private MenuItem _copyInfoChildCodesMenuItem;
		private bool _isDestroyed;

		public TrueMarkCodeHistoryView(TrueMarkCodeHistoryViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureUi();
		}

		private void ConfigureUi()
		{
			ytreeviewTrueMarkInfo.ColumnsConfig = FluentColumnsConfig<TrueMarkCodeInfoNode>.Create()
				.AddColumn("Параметр")
					.AddTextRenderer(x => x.Name)
					.Editable(false)
				.AddColumn("Значение")
					.AddTextRenderer(x => x.Value)
					.Editable(false)
					.SearchHighlight()
				.Finish();

			ytreeviewTrueMarkInfo.Selection.Mode = SelectionMode.Single;
			ytreeviewTrueMarkInfo.Binding.AddSource(ViewModel)
				.AddFuncBinding(
					vm => new RecursiveTreeModel<TrueMarkCodeInfoNode>(
						vm.TrueMarkInfoNodes,
						new RecursiveConfig<TrueMarkCodeInfoNode>(x => x.Parent, x => x.Children)),
					w => w.YTreeModel)
				.AddBinding(vm => vm.SelectedTrueMarkInfoNode, w => w.SelectedRow)
				.InitializeFromSource();
			ytreeviewTrueMarkInfo.AfterModelChanged += (sender, args) => ScheduleTreeRefresh(ytreeviewTrueMarkInfo);

			ytreeviewHistory.ColumnsConfig = FluentColumnsConfig<TrueMarkCodeHistoryNode>.Create()
				.AddColumn("Событие")
					.AddTextRenderer(x => x.Title)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Детали")
					.AddTextRenderer(x => x.Details)
					.Editable(false)
					.SearchHighlight()
				.AddColumn("Дата")
					.AddTextRenderer(x => x.Date)
					.Editable(false)
				.Finish();

			ytreeviewHistory.Selection.Mode = SelectionMode.Single;
			ytreeviewHistory.Binding.AddSource(ViewModel)
				.AddFuncBinding(
					vm => new RecursiveTreeModel<TrueMarkCodeHistoryNode>(
						vm.HistoryNodes,
						new RecursiveConfig<TrueMarkCodeHistoryNode>(x => x.Parent, x => x.Children)),
					w => w.YTreeModel)
				.AddBinding(vm => vm.SelectedHistoryNode, w => w.SelectedRow)
				.InitializeFromSource();
			ytreeviewHistory.AfterModelChanged += (sender, args) => ScheduleTreeRefresh(ytreeviewHistory);

			entrySearch.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SearchText, w => w.Text)
				.InitializeFromSource();

			ylabelStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StatusMessage, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonSearch.Clicked += (sender, args) => ExecuteSearch();

			CreateHistoryPopup();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			SetSearchEntryColor();
		}

		private void CreateHistoryPopup()
		{
			_historyPopup = new Menu();

			_openOrderMenuItem = new MenuItem("Открыть заказ");
			_openOrderCodesMenuItem = new MenuItem("Открыть коды заказа");
			_openEdoDialogMenuItem = new MenuItem("Открыть диалог ЭДО");
			_openEdoTaskMenuItem = new MenuItem("Перейти в задачу (диалог ЭДО)");
			_openEdoProcessMenuItem = new MenuItem("Открыть документооборот в журнале");
			_openSourceDocumentMenuItem = new MenuItem("Открыть документ");
			_copyCodeMenuItem = new MenuItem("Копировать код");
			_copyHistoryChildCodesMenuItem = new MenuItem("Копировать дочерние коды");

			_historyPopup.Append(_openOrderMenuItem);
			_historyPopup.Append(_openOrderCodesMenuItem);
			_historyPopup.Append(_openEdoDialogMenuItem);
			_historyPopup.Append(_openEdoTaskMenuItem);
			_historyPopup.Append(_openEdoProcessMenuItem);
			_historyPopup.Append(new SeparatorMenuItem());
			_historyPopup.Append(_openSourceDocumentMenuItem);
			_historyPopup.Append(new SeparatorMenuItem());
			_historyPopup.Append(_copyCodeMenuItem);
			_historyPopup.Append(_copyHistoryChildCodesMenuItem);

			_openOrderMenuItem.Activated += (sender, args) => ViewModel.OpenOrderCommand.Execute(null);
			_openOrderCodesMenuItem.Activated += (sender, args) => ViewModel.OpenOrderCodesCommand.Execute(null);
			_openEdoDialogMenuItem.Activated += (sender, args) => ViewModel.OpenEdoDialogCommand.Execute(null);
			_openEdoTaskMenuItem.Activated += (sender, args) => ViewModel.OpenEdoDialogCommand.Execute(null);
			_openEdoProcessMenuItem.Activated += (sender, args) => ViewModel.OpenEdoProcessCommand.Execute(null);
			_openSourceDocumentMenuItem.Activated += (sender, args) => ViewModel.OpenSourceDocumentCommand.Execute(null);
			_copyCodeMenuItem.Activated += (sender, args) => ViewModel.CopyCodeCommand.Execute(null);
			_copyHistoryChildCodesMenuItem.Activated += (sender, args) => ViewModel.CopyHistoryChildCodesCommand.Execute(null);

			_historyPopup.ShowAll();

			ytreeviewHistory.Add(_historyPopup);
			ytreeviewHistory.ButtonReleaseEvent += OnTreeViewHistoryButtonRelease;

			CreateTrueMarkInfoPopup();
		}

		private void CreateTrueMarkInfoPopup()
		{
			_trueMarkInfoPopup = new Menu();

			_copyInfoCodeMenuItem = new MenuItem("Копировать код");
			_copyInfoChildCodesMenuItem = new MenuItem("Копировать дочерние коды");

			_trueMarkInfoPopup.Append(_copyInfoCodeMenuItem);
			_trueMarkInfoPopup.Append(_copyInfoChildCodesMenuItem);

			_copyInfoCodeMenuItem.Activated += (sender, args) => ViewModel.CopyTrueMarkInfoCodeCommand.Execute(null);
			_copyInfoChildCodesMenuItem.Activated += (sender, args) => ViewModel.CopyTrueMarkInfoChildCodesCommand.Execute(null);

			_trueMarkInfoPopup.ShowAll();

			ytreeviewTrueMarkInfo.Add(_trueMarkInfoPopup);
			ytreeviewTrueMarkInfo.ButtonReleaseEvent += OnTreeViewTrueMarkInfoButtonRelease;
		}

		private void ExecuteSearch()
		{
			if(ViewModel.SearchCommand.CanExecute(null))
			{
				ViewModel.SearchCommand.Execute(null);
			}
		}

		private void ScheduleTreeRefresh(yTreeView treeView)
		{
			if(_isDestroyed || treeView is null)
			{
				return;
			}

			// Раскрытие и перерисовку откладываем на idle,
			// чтобы не менять модель дерева внутри обработки выбора строки.
			GLib.Idle.Add(() =>
			{
				if(_isDestroyed || treeView is null)
				{
					return false;
				}

				try
				{
					if(treeView.Selection != null)
					{
						treeView.Selection.UnselectAll();
					}

					treeView.ExpandAll();
					treeView.QueueDraw();
				}
				catch
				{
					// Виджет мог быть уничтожен до выполнения отложенного обновления.
				}

				return false;
			});
		}

		private void OnTreeViewHistoryButtonRelease(object sender, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_openOrderMenuItem.Sensitive = ViewModel.OpenOrderCommand.CanExecute(null);
			_openOrderCodesMenuItem.Sensitive = ViewModel.OpenOrderCodesCommand.CanExecute(null);
			_openEdoDialogMenuItem.Sensitive = ViewModel.OpenEdoDialogCommand.CanExecute(null);
			_openEdoTaskMenuItem.Sensitive = ViewModel.SelectedHistoryNode?.NodeType == TrueMarkCodeHistoryNodeType.Task
				&& ViewModel.OpenEdoDialogCommand.CanExecute(null);
			_openEdoProcessMenuItem.Sensitive = ViewModel.OpenEdoProcessCommand.CanExecute(null);
			_openSourceDocumentMenuItem.Sensitive = ViewModel.OpenSourceDocumentCommand.CanExecute(null);
			_copyCodeMenuItem.Sensitive = ViewModel.CopyCodeCommand.CanExecute(null);
			_copyHistoryChildCodesMenuItem.Sensitive = ViewModel.CopyHistoryChildCodesCommand.CanExecute(null);

			_historyPopup.Popup();
		}

		private void OnTreeViewTrueMarkInfoButtonRelease(object sender, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_copyInfoCodeMenuItem.Sensitive = ViewModel.CopyTrueMarkInfoCodeCommand.CanExecute(null);
			_copyInfoChildCodesMenuItem.Sensitive = ViewModel.CopyTrueMarkInfoChildCodesCommand.CanExecute(null);

			_trueMarkInfoPopup.Popup();
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if(args.PropertyName == nameof(TrueMarkCodeHistoryViewModel.IsValidSearchCodeText))
			{
				SetSearchEntryColor();
			}

			if(args.PropertyName == nameof(TrueMarkCodeHistoryViewModel.ParsedSearchCodeSerialNumber))
			{
				ytreeviewTrueMarkInfo.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
				ytreeviewHistory.SearchHighlightText = ViewModel.ParsedSearchCodeSerialNumber;
			}
		}

		private void SetSearchEntryColor()
		{
			if(ViewModel.IsValidSearchCodeText is null)
			{
				entrySearch.SetColor(StateType.Normal, GdkColors.PrimaryText);
				entrySearch.SetColor(StateType.Active, GdkColors.PrimaryText);
				return;
			}

			var color = ViewModel.IsValidSearchCodeText.Value
				? GdkColors.SuccessText
				: GdkColors.DangerText;

			entrySearch.SetColor(StateType.Normal, color);
			entrySearch.SetColor(StateType.Active, color);
		}

		protected override void OnDestroyed()
		{
			_isDestroyed = true;
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;

			_historyPopup?.Destroy();
			_openOrderMenuItem?.Destroy();
			_openOrderCodesMenuItem?.Destroy();
			_openEdoDialogMenuItem?.Destroy();
			_openEdoTaskMenuItem?.Destroy();
			_openEdoProcessMenuItem?.Destroy();
			_openSourceDocumentMenuItem?.Destroy();
			_copyCodeMenuItem?.Destroy();
			_copyHistoryChildCodesMenuItem?.Destroy();

			_trueMarkInfoPopup?.Destroy();
			_copyInfoCodeMenuItem?.Destroy();
			_copyInfoChildCodesMenuItem?.Destroy();

			ytreeviewTrueMarkInfo?.Destroy();
			ytreeviewHistory?.Destroy();

			base.OnDestroyed();
		}
	}
}
