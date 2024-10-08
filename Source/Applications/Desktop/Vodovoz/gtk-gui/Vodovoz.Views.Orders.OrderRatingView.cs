
// This file has been generated by the GUI designer. Do not modify.
namespace Vodovoz.Views.Orders
{
	public partial class OrderRatingView
	{
		private global::Gamma.GtkWidgets.yVBox vboxMain;

		private global::Gamma.GtkWidgets.yHBox hboxHandleButtons;

		private global::Gamma.GtkWidgets.yButton btnSave;

		private global::Gamma.GtkWidgets.yButton btnCancel;

		private global::Gtk.VSeparator vseparator1;

		private global::Gamma.GtkWidgets.yButton btnProcess;

		private global::Gamma.GtkWidgets.yButton btnCreateComplaint;

		private global::Gtk.HSeparator hseparator1;

		private global::Gamma.GtkWidgets.yTable tableMain;

		private global::Gamma.GtkWidgets.yButton btnOpenOnlineOrder;

		private global::Gamma.GtkWidgets.yButton btnOpenOrder;

		private global::Gamma.GtkWidgets.yLabel lblId;

		private global::Gamma.GtkWidgets.yLabel lblIdTitle;

		private global::Gamma.GtkWidgets.yLabel lblOnlineOrderId;

		private global::Gamma.GtkWidgets.yLabel lblOnlineOrderlIdTitle;

		private global::Gamma.GtkWidgets.yLabel lblOrderId;

		private global::Gamma.GtkWidgets.yLabel lblOrderlIdTitle;

		private global::Gamma.GtkWidgets.yLabel lblProcessedBy;

		private global::Gamma.GtkWidgets.yLabel lblProcessedByTitle;

		private global::Gamma.GtkWidgets.yLabel lblRating;

		private global::Gamma.GtkWidgets.yLabel lblRatingTitle;

		private global::Gtk.ScrolledWindow commentScrolledWindow;

		private global::Gamma.GtkWidgets.yTextView txtViewComment;

		private global::Gamma.GtkWidgets.yLabel lblComment;

		private global::Gtk.ScrolledWindow reasonsScrolledWindow;

		private global::Gamma.GtkWidgets.yTreeView treeViewReasons;

		private global::Gamma.GtkWidgets.yLabel lblOrderRatingReasons;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget Vodovoz.Views.Orders.OrderRatingView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Orders.OrderRatingView";
			// Container child Vodovoz.Views.Orders.OrderRatingView.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gamma.GtkWidgets.yVBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hboxHandleButtons = new global::Gamma.GtkWidgets.yHBox();
			this.hboxHandleButtons.Name = "hboxHandleButtons";
			this.hboxHandleButtons.Spacing = 6;
			// Container child hboxHandleButtons.Gtk.Box+BoxChild
			this.btnSave = new global::Gamma.GtkWidgets.yButton();
			this.btnSave.CanFocus = true;
			this.btnSave.Name = "btnSave";
			this.btnSave.UseUnderline = true;
			this.btnSave.Label = global::Mono.Unix.Catalog.GetString("Сохранить");
			this.hboxHandleButtons.Add(this.btnSave);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hboxHandleButtons[this.btnSave]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hboxHandleButtons.Gtk.Box+BoxChild
			this.btnCancel = new global::Gamma.GtkWidgets.yButton();
			this.btnCancel.CanFocus = true;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseUnderline = true;
			this.btnCancel.Label = global::Mono.Unix.Catalog.GetString("Отменить");
			this.hboxHandleButtons.Add(this.btnCancel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hboxHandleButtons[this.btnCancel]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hboxHandleButtons.Gtk.Box+BoxChild
			this.vseparator1 = new global::Gtk.VSeparator();
			this.vseparator1.Name = "vseparator1";
			this.hboxHandleButtons.Add(this.vseparator1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hboxHandleButtons[this.vseparator1]));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hboxHandleButtons.Gtk.Box+BoxChild
			this.btnProcess = new global::Gamma.GtkWidgets.yButton();
			this.btnProcess.CanFocus = true;
			this.btnProcess.Name = "btnProcess";
			this.btnProcess.UseUnderline = true;
			this.btnProcess.Label = global::Mono.Unix.Catalog.GetString("Обработать");
			this.hboxHandleButtons.Add(this.btnProcess);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hboxHandleButtons[this.btnProcess]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hboxHandleButtons.Gtk.Box+BoxChild
			this.btnCreateComplaint = new global::Gamma.GtkWidgets.yButton();
			this.btnCreateComplaint.CanFocus = true;
			this.btnCreateComplaint.Name = "btnCreateComplaint";
			this.btnCreateComplaint.UseUnderline = true;
			this.btnCreateComplaint.Label = global::Mono.Unix.Catalog.GetString("Создать рекламацию");
			this.hboxHandleButtons.Add(this.btnCreateComplaint);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hboxHandleButtons[this.btnCreateComplaint]));
			w5.Position = 4;
			w5.Expand = false;
			w5.Fill = false;
			this.vboxMain.Add(this.hboxHandleButtons);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hboxHandleButtons]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator();
			this.hseparator1.Name = "hseparator1";
			this.vboxMain.Add(this.hseparator1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.hseparator1]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.tableMain = new global::Gamma.GtkWidgets.yTable();
			this.tableMain.Name = "tableMain";
			this.tableMain.NRows = ((uint)(4));
			this.tableMain.NColumns = ((uint)(6));
			this.tableMain.RowSpacing = ((uint)(6));
			this.tableMain.ColumnSpacing = ((uint)(6));
			// Container child tableMain.Gtk.Table+TableChild
			this.btnOpenOnlineOrder = new global::Gamma.GtkWidgets.yButton();
			this.btnOpenOnlineOrder.CanFocus = true;
			this.btnOpenOnlineOrder.Name = "btnOpenOnlineOrder";
			this.btnOpenOnlineOrder.UseUnderline = true;
			this.btnOpenOnlineOrder.Label = global::Mono.Unix.Catalog.GetString("Открыть онлайн заказ");
			this.tableMain.Add(this.btnOpenOnlineOrder);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.tableMain[this.btnOpenOnlineOrder]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.LeftAttach = ((uint)(2));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.btnOpenOrder = new global::Gamma.GtkWidgets.yButton();
			this.btnOpenOrder.CanFocus = true;
			this.btnOpenOrder.Name = "btnOpenOrder";
			this.btnOpenOrder.UseUnderline = true;
			this.btnOpenOrder.Label = global::Mono.Unix.Catalog.GetString("Открыть заказ");
			this.tableMain.Add(this.btnOpenOrder);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.tableMain[this.btnOpenOrder]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblId = new global::Gamma.GtkWidgets.yLabel();
			this.lblId.Name = "lblId";
			this.lblId.Xalign = 0F;
			this.lblId.LabelProp = global::Mono.Unix.Catalog.GetString("@Id@");
			this.tableMain.Add(this.lblId);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblId]));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblIdTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblIdTitle.Name = "lblIdTitle";
			this.lblIdTitle.Xalign = 1F;
			this.lblIdTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Код:");
			this.tableMain.Add(this.lblIdTitle);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblIdTitle]));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblOnlineOrderId = new global::Gamma.GtkWidgets.yLabel();
			this.lblOnlineOrderId.Name = "lblOnlineOrderId";
			this.lblOnlineOrderId.Xalign = 0F;
			this.lblOnlineOrderId.LabelProp = global::Mono.Unix.Catalog.GetString("@OnlineOrderId@");
			this.tableMain.Add(this.lblOnlineOrderId);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblOnlineOrderId]));
			w12.TopAttach = ((uint)(1));
			w12.BottomAttach = ((uint)(2));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblOnlineOrderlIdTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblOnlineOrderlIdTitle.Name = "lblOnlineOrderlIdTitle";
			this.lblOnlineOrderlIdTitle.Xalign = 1F;
			this.lblOnlineOrderlIdTitle.LabelProp = global::Mono.Unix.Catalog.GetString("№ онлайн заказа:");
			this.tableMain.Add(this.lblOnlineOrderlIdTitle);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblOnlineOrderlIdTitle]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblOrderId = new global::Gamma.GtkWidgets.yLabel();
			this.lblOrderId.Name = "lblOrderId";
			this.lblOrderId.Xalign = 0F;
			this.lblOrderId.LabelProp = global::Mono.Unix.Catalog.GetString("@OrderId@");
			this.tableMain.Add(this.lblOrderId);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblOrderId]));
			w14.TopAttach = ((uint)(2));
			w14.BottomAttach = ((uint)(3));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblOrderlIdTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblOrderlIdTitle.Name = "lblOrderlIdTitle";
			this.lblOrderlIdTitle.Xalign = 1F;
			this.lblOrderlIdTitle.LabelProp = global::Mono.Unix.Catalog.GetString("№ заказа:");
			this.tableMain.Add(this.lblOrderlIdTitle);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblOrderlIdTitle]));
			w15.TopAttach = ((uint)(2));
			w15.BottomAttach = ((uint)(3));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblProcessedBy = new global::Gamma.GtkWidgets.yLabel();
			this.lblProcessedBy.Name = "lblProcessedBy";
			this.lblProcessedBy.Xalign = 1F;
			this.lblProcessedBy.LabelProp = global::Mono.Unix.Catalog.GetString("@Обработана@");
			this.tableMain.Add(this.lblProcessedBy);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblProcessedBy]));
			w16.LeftAttach = ((uint)(5));
			w16.RightAttach = ((uint)(6));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblProcessedByTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblProcessedByTitle.Name = "lblProcessedByTitle";
			this.lblProcessedByTitle.Xalign = 1F;
			this.lblProcessedByTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Обработана:");
			this.tableMain.Add(this.lblProcessedByTitle);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblProcessedByTitle]));
			w17.LeftAttach = ((uint)(4));
			w17.RightAttach = ((uint)(5));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblRating = new global::Gamma.GtkWidgets.yLabel();
			this.lblRating.Name = "lblRating";
			this.lblRating.Xalign = 0F;
			this.lblRating.LabelProp = global::Mono.Unix.Catalog.GetString("@Rating@");
			this.tableMain.Add(this.lblRating);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblRating]));
			w18.TopAttach = ((uint)(3));
			w18.BottomAttach = ((uint)(4));
			w18.LeftAttach = ((uint)(1));
			w18.RightAttach = ((uint)(2));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableMain.Gtk.Table+TableChild
			this.lblRatingTitle = new global::Gamma.GtkWidgets.yLabel();
			this.lblRatingTitle.Name = "lblRatingTitle";
			this.lblRatingTitle.Xalign = 1F;
			this.lblRatingTitle.LabelProp = global::Mono.Unix.Catalog.GetString("Оценка:");
			this.tableMain.Add(this.lblRatingTitle);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.tableMain[this.lblRatingTitle]));
			w19.TopAttach = ((uint)(3));
			w19.BottomAttach = ((uint)(4));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vboxMain.Add(this.tableMain);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.tableMain]));
			w20.Position = 2;
			w20.Expand = false;
			w20.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.commentScrolledWindow = new global::Gtk.ScrolledWindow();
			this.commentScrolledWindow.Name = "commentScrolledWindow";
			this.commentScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child commentScrolledWindow.Gtk.Container+ContainerChild
			this.txtViewComment = new global::Gamma.GtkWidgets.yTextView();
			this.txtViewComment.CanFocus = true;
			this.txtViewComment.Name = "txtViewComment";
			this.commentScrolledWindow.Add(this.txtViewComment);
			this.vboxMain.Add(this.commentScrolledWindow);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.commentScrolledWindow]));
			w22.PackType = ((global::Gtk.PackType)(1));
			w22.Position = 3;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.lblComment = new global::Gamma.GtkWidgets.yLabel();
			this.lblComment.Name = "lblComment";
			this.lblComment.Xalign = 0F;
			this.lblComment.LabelProp = global::Mono.Unix.Catalog.GetString("Комментарий:");
			this.vboxMain.Add(this.lblComment);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.lblComment]));
			w23.PackType = ((global::Gtk.PackType)(1));
			w23.Position = 4;
			w23.Expand = false;
			w23.Fill = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.reasonsScrolledWindow = new global::Gtk.ScrolledWindow();
			this.reasonsScrolledWindow.Name = "reasonsScrolledWindow";
			this.reasonsScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child reasonsScrolledWindow.Gtk.Container+ContainerChild
			this.treeViewReasons = new global::Gamma.GtkWidgets.yTreeView();
			this.treeViewReasons.CanFocus = true;
			this.treeViewReasons.Name = "treeViewReasons";
			this.reasonsScrolledWindow.Add(this.treeViewReasons);
			this.vboxMain.Add(this.reasonsScrolledWindow);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.reasonsScrolledWindow]));
			w25.PackType = ((global::Gtk.PackType)(1));
			w25.Position = 5;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.lblOrderRatingReasons = new global::Gamma.GtkWidgets.yLabel();
			this.lblOrderRatingReasons.Name = "lblOrderRatingReasons";
			this.lblOrderRatingReasons.Xalign = 0F;
			this.lblOrderRatingReasons.LabelProp = global::Mono.Unix.Catalog.GetString("Причины:");
			this.vboxMain.Add(this.lblOrderRatingReasons);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.vboxMain[this.lblOrderRatingReasons]));
			w26.PackType = ((global::Gtk.PackType)(1));
			w26.Position = 6;
			w26.Expand = false;
			w26.Fill = false;
			this.Add(this.vboxMain);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
