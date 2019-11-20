using System;
using System.Linq;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyInUndeliveryView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		IUnitOfWork uow;
		UndeliveredOrder undeliveredOrder;

		public GuiltyInUndeliveryView()
		{
			this.Build();
		}

		public void ConfigureWidget(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, bool driverCanBeGuilty){
			this.uow = uow;
			this.undeliveredOrder = undeliveredOrder;
			enumBtnGuiltySide.ItemsEnum = typeof(GuiltyTypes);
			enumBtnGuiltySide.SetSensitive(GuiltyTypes.Driver, driverCanBeGuilty);
			enumBtnGuiltySide.SetSensitive(GuiltyTypes.None, !undeliveredOrder.ObservableGuilty.Any());
			undeliveredOrder.ObservableGuilty.ElementAdded += ObservableGuilty_ElementAdded;
			undeliveredOrder.ObservableGuilty.ElementRemoved += ObservableGuilty_ElementRemoved;
			enumBtnGuiltySide.EnumItemClicked += (sender, e) => {
				undeliveredOrder.AddGuilty(
					new GuiltyInUndelivery {
						GuiltySide = (GuiltyTypes)e.ItemEnum,
						UndeliveredOrder = undeliveredOrder
					}
				);
				SetWidgetApperance();
			};

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorGrey = new Gdk.Color(96, 96, 96);
			var colorWhite = new Gdk.Color(255, 255, 255);
			var hideEnums = !driverCanBeGuilty ? new Enum[] { GuiltyTypes.Driver } : new Enum[] { };
			var allDepartments = SubdivisionsRepository.GetAllDepartments(uow);
			treeViewGuilty.ColumnsConfig = ColumnsConfigFactory.Create<GuiltyInUndelivery>()
				.AddColumn("Сторона")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(n => n.GuiltySide, true, hideEnums)
				.AddColumn("Отдел ВВ")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(n => n.GuiltyDepartment)
					.SetDisplayFunc(x => x.Name)
					.FillItems(allDepartments)
					.AddSetter(
						(c, n) => {
							c.Editable = n.GuiltySide == GuiltyTypes.Department;
							if(n.GuiltySide != GuiltyTypes.Department)
								n.GuiltyDepartment = null;
							if(n.GuiltySide == GuiltyTypes.Department && n.GuiltyDepartment == null) {
								c.ForegroundGdk = colorGrey;
								c.Style = Pango.Style.Italic;
								c.Text = "(Нажмите для выбора отдела)";
								c.BackgroundGdk = colorWhite;
							} else {
								c.ForegroundGdk = colorBlack;
								c.Style = Pango.Style.Normal;
								c.Background = null;
							}
						}
					)
				.Finish();
			treeViewGuilty.HeadersVisible = false;
			treeViewGuilty.ItemsDataSource = undeliveredOrder.ObservableGuilty;
			SetWidgetApperance();
		}

		void ObservableGuilty_ElementAdded(object aList, int[] aIdx)
		{
			enumBtnGuiltySide.SetSensitive(GuiltyTypes.None, !undeliveredOrder.ObservableGuilty.Any());
		}

		void ObservableGuilty_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			enumBtnGuiltySide.SetSensitive(GuiltyTypes.None, !undeliveredOrder.ObservableGuilty.Any());
		}

		protected void OnBtnRemoveClicked(object sender, EventArgs e)
		{
			var guilty = treeViewGuilty.GetSelectedObject<GuiltyInUndelivery>();
			undeliveredOrder.ObservableGuilty.Remove(guilty);
			SetWidgetApperance();
		}

		void SetWidgetApperance()
		{
			if(undeliveredOrder.ObservableGuilty.Count > 3) {
				GtkScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Always;
				this.HeightRequest = 120;
			} else {
				GtkScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Never;
				this.HeightRequest = 0;
			}
		}
	}
}
