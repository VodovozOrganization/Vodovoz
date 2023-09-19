﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI.JournalActions;
using QS.Tdi;
using QSOrmProject;
using QSWidgetLib;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Core.Journal
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MultipleEntityJournal : SingleUowTabBase, ITdiJournal, IJournalDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		IMultipleEntityRepresentationModel representationModel;
		IMultipleEntityPermissionModel permissionModel;

		public MultipleEntityJournal(string title, IMultipleEntityRepresentationModel model, IMultipleEntityPermissionModel permissionModel)
		{
			this.Build();
			wraplabelSum.LineWrapMode = WrapMode.WordChar;
			this.RepresentationModel = model;
			UoW = model.UoW;
			this.permissionModel = permissionModel;
			TreeView.ColumnsConfig = model.ColumnsConfig;
			ConfigureDlg();
			TabName = title;
		}

		void ConfigureDlg()
		{
			Mode = JournalSelectMode.None;
			ConfigureActions();
			TreeView.Selection.Changed += TreeViewSelection_Changed;
			TreeView.ButtonReleaseEvent += OnOrmtableviewButtonReleaseEvent;
			RepresentationModel.UpdateNodes();
		}

		void TreeViewSelection_Changed(object sender, EventArgs e)
		{
			UpdateActionsSensitivity();
		}

		public IMultipleEntityRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {
				if(representationModel == value)
					return;
				if(representationModel != null)
					RepresentationModel.ItemsListUpdated -= RepresentationModel_ItemsListUpdated;
				representationModel = value;
				RepresentationModel.SearchStrings = null;
				RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
				tableview.RepresentationModel = RepresentationModel;
				if(RepresentationModel.JournalFilter != null) {
					Widget resolvedFilterWidget = DialogHelper.FilterWidgetResolver.Resolve(RepresentationModel.JournalFilter);
					SetFilter(resolvedFilterWidget);
				}
				hboxSearch.Visible = RepresentationModel.SearchFieldsExist;
				UpdatePopupItems();
			}
		}
		
		public virtual List<IJournalPopupAction> PopupActions { get; set; }
		
		private void UpdatePopupItems()
		{
			if(PopupActions == null) {
				PopupActions = new List<IJournalPopupAction>();
			} else {
				foreach(var pa in PopupActions) {
					pa.MenuItem.Destroy();
				}
				PopupActions.Clear();
			}

			foreach(var popupItem in RepresentationModel.PopupItems) {
				PopupActions.Add(new JournalPopupAction(popupItem));
			}
		}
		
		private Menu popupMenu;

		[GLib.ConnectBefore]
		protected void OnOrmtableviewButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && PopupActions.Any())
			{
				var selected = tableview.GetSelectedObjects();
				if(popupMenu == null) {
					popupMenu = new Menu();
					foreach(var popupAction in PopupActions) {
						popupMenu.Add(popupAction.MenuItem);
					}
				}
				foreach(var popupAction in PopupActions) {
					popupAction.SelectedItems = selected;
					popupAction.CheckSensitive(selected);
					popupAction.CheckVisibility(selected);
				}
				
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		private void ConfigureActions()
		{
			MenuButton addDocumentButton = new MenuButton();
			addDocumentButton.Label = "Добавить";
			Menu addDocumentActions = new Menu();
			foreach(var item in RepresentationModel.NewEntityActionsConfigs) {
				var menuItem = new MenuItem(item.Title);
				menuItem.Activated += (sender, e) => {
					TabParent.OpenTab(DialogHelper.GenerateDialogHashName(item.EntityType, 0),
						item.GetNewEntityDlg,
						this
					);
				};
				SetCreateActionsSensitiveFunc(menuItem, item.EntityType);
				addDocumentActions.Add(menuItem);
			}
			addDocumentButton.Menu = addDocumentActions;
			addDocumentActions.ShowAll();
			hboxButtonActions.Add(addDocumentButton);
			Box.BoxChild addDocumentButtonBox = (Box.BoxChild)hboxButtonActions[addDocumentButton];
			addDocumentButtonBox.Expand = false;
			addDocumentButtonBox.Fill = false;

			Button editDocumentbutton = new Button();
			editDocumentbutton.Label = "Редактировать";
			editDocumentbutton.Clicked += (sender, e) => {
				OpenDocument();
			};
			SetOpenActionSensitiveFunc(editDocumentbutton);
			hboxButtonActions.Add(editDocumentbutton);
			Box.BoxChild editDocumentbuttonBox = (Box.BoxChild)hboxButtonActions[editDocumentbutton];
			editDocumentbuttonBox.Expand = false;
			editDocumentbuttonBox.Fill = false;

			Button deleteDocumentbutton = new Button();
			deleteDocumentbutton.Label = "Удалить";
			deleteDocumentbutton.Clicked += (sender, e) => {
				var selectedObject = tableview.GetSelectedObject();
				if(OrmMain.DeleteObject(RepresentationModel.GetEntityType(selectedObject), RepresentationModel.GetDocumentId(selectedObject))) {
					RepresentationModel.UpdateNodes(); 
				}
			};
			SetDeleteActionSensitiveFunc(deleteDocumentbutton);
			hboxButtonActions.Add(deleteDocumentbutton);
			Box.BoxChild deleteDocumentbuttonBox = (Box.BoxChild)hboxButtonActions[deleteDocumentbutton];
			deleteDocumentbuttonBox.Expand = false;
			deleteDocumentbuttonBox.Fill = false;

			hboxButtonActions.ShowAll();
		}

		private List<System.Action> actionsSensitiveFunctons = new List<System.Action>();

		private void UpdateActionsSensitivity()
		{
			actionsSensitiveFunctons.ForEach(x => x.Invoke());
		}

		private void SetCreateActionsSensitiveFunc(MenuItem createMenuItem, Type entityType)
		{
			System.Action action = () =>  {
				createMenuItem.Sensitive = permissionModel.CanCreateNewEntity(entityType);
			};
			actionsSensitiveFunctons.Add(action);
		}

		private void SetOpenActionSensitiveFunc(Button button)
		{
			System.Action action = () => {
				var node = tableview.GetSelectedObject();
				button.Sensitive = permissionModel.CanOpenEntity(node);
			};
			actionsSensitiveFunctons.Add(action);
		}

		private void SetDeleteActionSensitiveFunc(Button button)
		{
			System.Action action = () => {
				var node = tableview.GetSelectedObject();
				button.Sensitive = permissionModel.CanDeleteEntity(node);
			};
			actionsSensitiveFunctons.Add(action);
		}

		private void OpenDocument()
		{
			TabParent.OpenTab(DialogHelper.GenerateDialogHashName(RepresentationModel.GetEntityType(tableview.GetSelectedObject()), 0),
					() => RepresentationModel.GetOpenEntityDlg(tableview.GetSelectedObject()),
					this
				);
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			RepresentationModel.UpdateNodes();
		}

		protected void OnTableviewRowActivated(object o, RowActivatedArgs args)
		{
			OpenDocument();
		}

		void RepresentationModel_ItemsListUpdated(object sender, EventArgs e)
		{
			UpdateActionsSensitivity();
			UpdateSum();
		}

		protected void UpdateSum()
		{
			Application.Invoke((e, args) => {
				var text = representationModel.GetSummaryInfo();
				if(text.Length > 180) {
					labelSum.Hide();
					wraplabelSum.Show();
					wraplabelSum.Text = text;
				}
				else {
					wraplabelSum.Hide();
					labelSum.Show();
					labelSum.Text = text;
				}
			});
		}

		#region ITdiJournal implementation

		public bool? UseSlider => null;

		#endregion

		#region Фильтр

		private Widget filterWidget;

		private void SetFilter(Widget filter)
		{
			if(filterWidget == filter)
				return;
			if(filterWidget != null) {
				hboxFilter.Remove(filterWidget);
				filterWidget.Destroy();
				checkShowFilter.Visible = true;
				filterWidget = null;
			}
			filterWidget = filter;
			checkShowFilter.Visible = filterWidget != null;
			hboxFilter.Add(filterWidget);
			filterWidget.ShowAll();
		}

		public bool ShowFilter {
			get {
				return checkShowFilter.Active;
			}
			set {
				checkShowFilter.Active = value;
			}
		}

		protected void OnCheckShowFilterToggled(object sender, EventArgs e)
		{
			hboxFilter.Visible = checkShowFilter.Active;
		}

		#endregion

		#region IJournalDialog implementation

		public yTreeView TreeView => tableview;

		private JournalSelectMode mode;

		public JournalSelectMode Mode {
			get { return mode; }
			set {
				mode = value;
				tableview.Selection.Mode = (mode == JournalSelectMode.Multiple) ? SelectionMode.Multiple : SelectionMode.Single;
			}
		}

		public object[] SelectedNodes => tableview.GetSelectedObjects();

		public void OnObjectSelected(params object[] selectedNodes)
		{
			throw new NotImplementedException("Не реализован выбор документов для журналов с документами различных типов");
		}

		#endregion

		#region Реализация поиска

		private int searchEntryShown = 1;

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			entrySearch.Text = entrySearch2.Text = entrySearch3.Text = entrySearch4.Text = String.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			UpdateSearchString();
		}

		void UpdateSearchString()
		{
			var searchList = new List<string>();
			if(!String.IsNullOrEmpty(entrySearch.Text))
				searchList.Add(entrySearch.Text);
			if(!String.IsNullOrEmpty(entrySearch2.Text))
				searchList.Add(entrySearch2.Text);
			if(!String.IsNullOrEmpty(entrySearch3.Text))
				searchList.Add(entrySearch3.Text);
			if(!String.IsNullOrEmpty(entrySearch4.Text))
				searchList.Add(entrySearch4.Text);

			RepresentationModel.SearchStrings = tableview.SearchHighlightTexts = searchList.ToArray();
		}

		protected void OnButtonAddAndClicked(object sender, EventArgs e)
		{
			SearchVisible(searchEntryShown + 1);
		}

		public void SetSearchTexts(params string[] strings)
		{
			int i = 0;
			foreach(var str in strings) {
				if(!String.IsNullOrWhiteSpace(str)) {
					i++;
					SetSearchText(i, str.Trim());
				}
			}
			SearchVisible(i);
		}

		private void SetSearchText(int n, string text)
		{
			switch(n) {
				case 1:
					entrySearch.Text = text;
					break;
				case 2:
					entrySearch2.Text = text;
					break;
				case 3:
					entrySearch3.Text = text;
					break;
				case 4:
					entrySearch4.Text = text;
					break;
			}
		}

		protected void SearchVisible(int count)
		{
			entrySearch.Visible = count > 0;
			ylabelSearchAnd.Visible = entrySearch2.Visible = count > 1;
			ylabelSearchAnd2.Visible = entrySearch3.Visible = count > 2;
			ylabelSearchAnd3.Visible = entrySearch4.Visible = count > 3;
			buttonAddAnd.Sensitive = count < 4;
			searchEntryShown = count;
		}

		public override void Destroy()
		{
			if(RepresentationModel != null) {
				RepresentationModel.Destroy();
			}
			base.Destroy();
		}



		#endregion
	}
}
