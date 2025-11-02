using Gamma.Binding.Core;
using Gdk;
using Gtk;
using QS.Extensions;
using ReactiveUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;

namespace Vodovoz.ViewWidgets.GtkUI
{
	[ToolboxItem(true)]
	public partial class EntitySelection : Gtk.Bin
	{
		public static uint QueryDelay = 0;

		private readonly Color _dangerTextHtmlColor = GdkColors.DangerText;

		private readonly string _normalEntryToolTipMarkup;
		private readonly string _dangerEntryToolTipMarkup = "Введён текст для поиска, но не выбрана сущность из справочника или выпадающего списка.";

		private IEntitySelectionViewModel _viewModel;

		private bool _isInternalTextSet;
		private ListStore _completionListStore;
		uint _timerId;

		public EntitySelection()
		{
			Build();

			Binding = new BindingControler<EntitySelection>(this);

			ConfigureEntryComplition();

			yentryObject.FocusOutEvent += (s, e) => OnEntryObjectFocusOutEvent(s, e);
			yentryObject.Changed += (s, e) => OnEntryObjectChanged(s, e);
			yentryObject.WidgetEvent += (s, e) => OnEntryObjectWidgetEvent(s, e);

			_normalEntryToolTipMarkup = yentryObject.TooltipMarkup;
		}

		public BindingControler<EntitySelection> Binding { get; private set; }

		public IEntitySelectionViewModel ViewModel
		{
			get => _viewModel;
			set
			{
				if(_viewModel == value)
				{
					return;
				}

				_viewModel = value;

				if(_viewModel != null)
				{
					_viewModel.PropertyChanged += ViewModel_PropertyChanged;
				}

				ybuttonSelectEntity.Sensitive = _viewModel.CanSelectEntity;
				ybuttonClear.Sensitive = _viewModel.CanClearEntity;
				yentryObject.IsEditable = _viewModel.CanSelectEntityFromDialog;
				SetEntryText(_viewModel.EntityTitle);

				_viewModel.AutocompleteListSize = 20;
				_viewModel.AutoCompleteListUpdated += ViewModel_AutoCompleteListUpdated;

				ybuttonSelectEntity.BindCommand(ViewModel.SelectEntityCommand);
				ybuttonClear.BindCommand(ViewModel.ClearEntityCommand);
			}
		}

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(IEntitySelectionViewModel.CanSelectEntity):
					ybuttonSelectEntity.Sensitive = ViewModel.CanSelectEntity;
					break;
				case nameof(IEntitySelectionViewModel.CanClearEntity):
					ybuttonClear.Sensitive = ViewModel.CanClearEntity;
					break;
				case nameof(IEntitySelectionViewModel.CanSelectEntityFromDialog):
					yentryObject.IsEditable = ViewModel.CanSelectEntityFromDialog;
					break;
				case nameof(IEntitySelectionViewModel.EntityTitle):
					SetEntryText(ViewModel.EntityTitle);
					break;
				default:
					break;
			}
		}

		private void SetEntryText(string text)
		{
			_isInternalTextSet = true;

			yentryObject.Text = text ?? string.Empty;
			yentryObject.ModifyText(StateType.Normal);

			_isInternalTextSet = false;
		}

		#region AutoCompletion

		private void ConfigureEntryComplition()
		{
			yentryObject.Completion = new EntryCompletion();
			yentryObject.Completion.MatchSelected += Completion_MatchSelected;
			yentryObject.Completion.MatchFunc = Completion_MatchFunc;

			var cell = new CellRendererText();

			yentryObject.Completion.PackStart(cell, true);
			yentryObject.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = _viewModel.GetAutocompleteTitle(tree_model.GetValue(iter, 0)) ?? string.Empty;
			var words = yentryObject.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var word in words)
			{
				string pattern = string.Format("{0}", Regex.Escape(word));
				title = Regex.Replace(title, pattern, (match) => string.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			}
			(cell as CellRendererText).Markup = title;
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			var node = args.Model.GetValue(args.Iter, 0);
			_viewModel.AutocompleteSelectNode(node);
			args.RetVal = true;
		}

		void ViewModel_AutoCompleteListUpdated(object sender, AutocompleteUpdatedEventArgs e)
		{
			Gtk.Application.Invoke((s, arg) =>
			{
				FillAutocomplete(e.AutocompleteItems);
			});
		}

		private void FillAutocomplete(IList list)
		{
			_completionListStore = new ListStore(typeof(object));

			foreach(var item in list)
			{
				_completionListStore.AppendValues(item);
			}

			if(yentryObject?.Completion is null)
			{
				return;
			}

			yentryObject.Completion.Model = _completionListStore;
			yentryObject.Completion.PopupCompletion = true;
		}

		protected void OnEntryObjectFocusOutEvent(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(yentryObject.Text))
			{
				yentryObject.ModifyText(StateType.Normal);
				yentryObject.TooltipMarkup = _normalEntryToolTipMarkup;
				_viewModel.ClearEntityCommand.Execute();
			}
			else if(yentryObject.Text != ViewModel.EntityTitle)
			{
				yentryObject.ModifyText(StateType.Normal, _dangerTextHtmlColor);
				yentryObject.TooltipMarkup = _dangerEntryToolTipMarkup;
			}
		}

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			if(_isInternalTextSet)
			{
				return;
			}

			if(QueryDelay != 0)
			{
				GLib.Source.Remove(_timerId);
				_timerId = GLib.Timeout.Add(QueryDelay, new GLib.TimeoutHandler(RunSearch));
			}
			else
			{
				RunSearch();
			}
		}

		bool RunSearch()
		{
			_viewModel.AutocompleteTextEdited(yentryObject.Text);
			_timerId = 0;

			return false;
		}

		protected void OnEntryObjectWidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress && _timerId != 0)
			{
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();

				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter))
				{
					GLib.Source.Remove(_timerId);
					RunSearch();
				}
			}
		}
		#endregion

		protected override void OnDestroyed()
		{
			if(_viewModel != null)
			{
				_viewModel.PropertyChanged -= ViewModel_PropertyChanged;
				_viewModel.AutoCompleteListUpdated -= ViewModel_AutoCompleteListUpdated;

				if(_viewModel.DisposeViewModel)
				{
					_viewModel.Dispose();
					_viewModel = null;
				}
			}

			Binding.CleanSources();
			var selectImage = ybuttonSelectEntity.Image as Gtk.Image;
			selectImage.DisposeImagePixbuf();
			var clearImage = ybuttonClear.Image as Gtk.Image;
			clearImage.DisposeImagePixbuf();

			GLib.Source.Remove(_timerId);

			base.OnDestroyed();
		}
	}
}
