using System;
using Gamma.Binding;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Representations;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.JournalViewers
{
	public partial class UndeliveriesView : TdiTabBase, IUndeliveredOrdersInfoProvider//, ITdiJournal
	{
		IUnitOfWork uow;

		UndeliveredOrdersVMNode selectedNode;
		UndeliveredOrdersVM vm;

		#region Работа с боковыми панелями

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.UndeliveredOrdersPanelView };

		public DateTime StartDate => undeliveredOrdersFilter.RestrictOldOrderStartDate.Value;

		public DateTime EndDate => undeliveredOrdersFilter.RestrictOldOrderEndDate.Value;

		#endregion

		public IUnitOfWork UoW {
			get => uow;
			set {
				if(uow == value)
					return;
				uow = value;
				Configure();
			}
		}

		void Configure()
		{
			undeliveredOrdersFilter.UoW = UoW;
			undeliveredOrdersFilter.Refiltered += (sender, e) => Refresh();
			vm = new UndeliveredOrdersVM(UoW);
			vm.Filter = undeliveredOrdersFilter;
			Refresh();
		}

		public UndeliveriesView()
		{
			this.Build();
			this.TabName = "Журнал недовозов";
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			undeliveredOrdersFilter.Visible = checkShowFilter.Active;
			yTreeViewUndeliveries.Selection.Changed += OnYTreeViewUndeliveriesSelectionChanged; ;
		}

		public virtual void HideFilterAndControls(){
			undeliveredOrdersFilter.Visible = hbxDlgControls.Visible = false;
		}

		public UndeliveredOrdersFilter GetUndeliveryFilter => undeliveredOrdersFilter;

		protected void OnSearchEntityTextChanged(object sender, System.EventArgs e)
		{
			yTreeViewUndeliveries.SearchHighlightText = searchEntity.Text;
		}

		protected void OnButtonRefreshClicked(object sender, System.EventArgs e)
		{
			Refresh();
		}

		public virtual void Refresh()
		{
			vm.UpdateNodes();
			yTreeViewUndeliveries.ColumnsConfig = vm.ColumnsConfig;
			yTreeViewUndeliveries.YTreeModel = new RecursiveTreeModel<UndeliveredOrdersVMNode>(vm.Result, x => x.Parent, x => x.Children);
			yTreeViewUndeliveries.YTreeModel.EmitModelChanged();
			yTreeViewUndeliveries.ExpandAll();
			if(CurrentObjectChanged != null)
				CurrentObjectChanged(this, new CurrentObjectChangedArgs(undeliveredOrdersFilter));
		}

		#region Popup Menu
		public virtual Menu GetPopupMenu(UndeliveredOrdersVMNode selected)
		{
			menusSelected = selected;

			Menu popupMenu = new Menu();

			MenuItem menuItemOldOrder = new MenuItem("Перейти в недовезённый заказ");
			menuItemOldOrder.Activated += MenuItemOldOrder_Activated;
			menuItemOldOrder.Sensitive = selected != null;
			popupMenu.Add(menuItemOldOrder);

			MenuItem menuItemNewOrder = new MenuItem("Перейти в новый заказ");
			menuItemNewOrder.Activated += MenuItemNewOrder_Activated;
			menuItemNewOrder.Sensitive = selected.NewOrderId > 0;
			popupMenu.Add(menuItemNewOrder);

			MenuItem menuItemNewFine = new MenuItem("Создать новый штраф");
			menuItemNewFine.Activated += MenuItemNewFine_Activated;
			menuItemNewFine.Sensitive = selected != null;
			popupMenu.Add(menuItemNewFine);

			MenuItem menuItemCloseUndelivery = new MenuItem("Закрыть недовоз");
			menuItemCloseUndelivery.Activated += MenuItemCloseUndelivery_Activated;
			menuItemCloseUndelivery.Sensitive = QSMain.User.Permissions["can_close_undeliveries"] && selected.UndeliveryStatus != UndeliveryStatus.Closed;
			popupMenu.Add(menuItemCloseUndelivery);

			return popupMenu;
		}

		UndeliveredOrdersVMNode menusSelected;

		void MenuItemOldOrder_Activated(object sender, EventArgs e)
		{
			var oldOrdersId = menusSelected.OldOrderId;
			var dlg = new OrderDlg(oldOrdersId);
			dlg.EntitySaved += (s,ea) => Refresh();
			var tdiMain = MainClass.MainWin.TdiMain;
			tdiMain.OpenTab(
				OrmMain.GenerateDialogHashName<Domain.Orders.Order>(oldOrdersId),
				() => dlg
			);
		}

		void MenuItemNewOrder_Activated(object sender, EventArgs e)
		{
			var newOrdersId = menusSelected.NewOrderId;
			var dlg = new OrderDlg(newOrdersId);
			dlg.EntitySaved += (s, ea) => Refresh();
			var tdiMain = MainClass.MainWin.TdiMain;
			tdiMain.OpenTab(
				OrmMain.GenerateDialogHashName<Domain.Orders.Order>(newOrdersId),
				() => dlg
			);
		}

		void MenuItemNewFine_Activated(object sender, EventArgs e)
		{
			var tdiMain = MainClass.MainWin.TdiMain;
			FineDlg fineDlg = new FineDlg(UoW.GetById<UndeliveredOrder>(menusSelected.Id));
			tdiMain.OpenTab(
				OrmMain.GenerateDialogHashName<Fine>(menusSelected.Id),
				() => fineDlg
			);
			fineDlg.EntitySaved += (sndr, eArgs) => Refresh();
		}

		void MenuItemCloseUndelivery_Activated(object sender, EventArgs e)
		{
			UndeliveredOrder undeliveredOrder = UoW.GetById<UndeliveredOrder>(menusSelected.Id);
			undeliveredOrder.Close();
			UoW.Save(undeliveredOrder);
			UoW.Commit();
			Refresh();
		}

		#endregion

		protected void OnYTreeViewUndeliveriesButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				if(yTreeViewUndeliveries.GetSelectedObject() is UndeliveredOrderCommentsNode)
					selectedNode = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrderCommentsNode>().Parent;
				else
					selectedNode = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrdersVMNode>();
				
				if(selectedNode != null) {
					var menu = GetPopupMenu(selectedNode);
					if(menu != null) {
						menu.ShowAll();
						menu.Popup();
					}
				}
			}
		}

		protected void OnCheckShowFilterToggled(object sender, EventArgs e)
		{
			undeliveredOrdersFilter.Visible = checkShowFilter.Active;
		}

		void OnYTreeViewUndeliveriesSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewUndeliveries.Selection.CountSelectedRows() > 0;
			bool isNotComment = !(yTreeViewUndeliveries.GetSelectedObject() is UndeliveredOrderCommentsNode);
			buttonEdit.Sensitive = ButtonMode.HasFlag(ReferenceButtonMode.CanEdit) && selected;
			buttonDelete.Sensitive = ButtonMode.HasFlag(ReferenceButtonMode.CanDelete) && selected;
		}


		private ReferenceButtonMode buttonMode;

		public ReferenceButtonMode ButtonMode {
			get { return buttonMode; }
			set {
				buttonMode = value;
				buttonAdd.Sensitive = buttonMode.HasFlag(ReferenceButtonMode.CanAdd);
				OnYTreeViewUndeliveriesSelectionChanged(this, EventArgs.Empty);
				Image image = new Image();
				image.Pixbuf = Stetic.IconLoader.LoadIcon(
					this,
					buttonMode.HasFlag(ReferenceButtonMode.TreatEditAsOpen) ? "gtk-open" : "gtk-edit",
					IconSize.Menu);
				buttonEdit.Image = image;
				buttonEdit.Label = buttonMode.HasFlag(ReferenceButtonMode.TreatEditAsOpen) ? "Открыть" : "Изменить";
			}
		}

		public bool? UseSlider => null;

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var dlg = new UndeliveredOrderDlg();
			TabParent.AddTab(dlg, this, true);
			dlg.DlgSaved += dlg_DlgSaved;
			if(TabParent is TdiSliderTab) {
				((TdiSliderTab)TabParent).IsHideJournal = true;
			}
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			int selectedId = 0;
			if(yTreeViewUndeliveries.GetSelectedObject() is UndeliveredOrderCommentsNode)
				selectedId = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrderCommentsNode>().Parent.Id;
			else
				selectedId = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrdersVMNode>().Id;
			var dlg = new UndeliveredOrderDlg(selectedId);
			TabParent.OpenTab(
				OrmMain.GenerateDialogHashName<UndeliveredOrder>(selectedId),
				() => dlg
			);
			dlg.CommentAdded += (s, ea) => Refresh();
			dlg.DlgSaved += dlg_DlgSaved;
		}

		void dlg_DlgSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			Refresh();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			//yTreeViewUndeliveries.GetSelectedObjects().ForEach(id => OrmMain.DeleteObject(typeof(UndeliveredOrder), id));
		}

		protected void OnYTreeViewUndeliveriesRowActivated(object o, RowActivatedArgs args)
		{
			UndeliveredOrdersVMNode selectedObj = null;
			if(yTreeViewUndeliveries.GetSelectedObject() is UndeliveredOrderCommentsNode)
				selectedObj = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrderCommentsNode>()?.Parent;
			else
				selectedObj = yTreeViewUndeliveries.GetSelectedObject<UndeliveredOrdersVMNode>();

			if(selectedObj == null)
				return;

			//кусок старой реализации, когда по даблклику вычислялось кликнутое поле
			/*CommentedFields field = CommentedFields.None;
			string valueOfField = String.Empty;

			foreach(var cell in args.Column.Cells.OfType<NodeCellRendererText<UndeliveredOrdersVMNode>>().Where(c => c.DataPropertyInfo != null)) {
				if(Enum.TryParse<CommentedFields>(cell.DataPropertyName, out field)) {
					valueOfField = selectedObj.GetPropertyValue(cell.DataPropertyName).ToString();
					break;
				}
				field = CommentedFields.None;
			}

			if(field == CommentedFields.None)
				return;*/

			var dlg = new UndeliveredOrderDlg(selectedObj.Id);
			dlg.CommentAdded += (sender, e) => Refresh();
			dlg.DlgSaved += dlg_DlgSaved;
			TabParent.OpenTab(
				OrmMain.GenerateDialogHashName<UndeliveredOrder>(selectedObj.Id),
				() => dlg,
				this
			);

			/*TabParent.AddTab(dlg, this, true);
			if(TabParent is TdiSliderTab) {
				((TdiSliderTab)TabParent).IsHideJournal = true;
			}*/
		}

		protected void OnBtnPrintClicked(object sender, EventArgs e)
		{
			TabParent.AddSlaveTab(this, new UndeliveriesWithCommentsPrintDlg(undeliveredOrdersFilter));
		}
	}
}
