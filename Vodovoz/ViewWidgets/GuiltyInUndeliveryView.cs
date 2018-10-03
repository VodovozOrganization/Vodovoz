using System;
using Gamma.GtkWidgets;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyInUndeliveryView : WidgetOnDialogBase
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
			enumBtnGuiltySide.EnumItemClicked += (sender, e) => {
				var guilty = new GuiltyInUndelivery {
					GuiltySide = (GuiltyTypes)e.ItemEnum,
					UndeliveredOrder = undeliveredOrder
				};
				undeliveredOrder.ObservableGuilty.Add(guilty);
				SetWidgetApperance();
			};

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorGrey = new Gdk.Color(96, 96, 96);
			var colorWhite = new Gdk.Color(255, 255, 255);
			treeViewGuilty.ColumnsConfig = ColumnsConfigFactory.Create<GuiltyInUndelivery>()
				.AddColumn("Сторона")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(n => n.GuiltySide, true, !driverCanBeGuilty ? new Enum[] { GuiltyTypes.Driver } : new Enum[] { }).Editing()
				.AddColumn("Отдел ВВ")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(n => n.GuiltyDepartment)
					.SetDisplayFunc(x => x.Name)
					.FillItems(SubdivisionsRepository.GetAllDepartments(uow))
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
							}
							else{
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
