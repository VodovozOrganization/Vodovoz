using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gdk;
using NHibernate.Util;
using Vodovoz.Additions.Logistic;
using System;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class RouresAtDayInfoWnd : Gtk.Window
	{
		public RouresAtDayInfoWnd() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			ConfigureWnd();
		}

		void ConfigureWnd(){
			var ms = new List<object[]>();
			ms.Add(new object[] { 
				PointMarker.GetIconPixbuf("red", PointMarkerShape.triangle),
				PointMarker.GetIconPixbuf("orange", PointMarkerShape.triangle),
				PointMarker.GetIconPixbuf("green", PointMarkerShape.triangle),
				"Адрес с количеством бутылей менее 6 шт." 
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("red", PointMarkerShape.circle),
				PointMarker.GetIconPixbuf("orange", PointMarkerShape.circle),
				PointMarker.GetIconPixbuf("green", PointMarkerShape.circle),
				"Адрес с количеством бутылей 6-10 шт."
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("red", PointMarkerShape.square),
				PointMarker.GetIconPixbuf("orange", PointMarkerShape.square),
				PointMarker.GetIconPixbuf("green", PointMarkerShape.square),
				"Адрес с количеством бутылей 10-20 шт."
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("red", PointMarkerShape.cross),
				PointMarker.GetIconPixbuf("orange", PointMarkerShape.cross),
				PointMarker.GetIconPixbuf("green", PointMarkerShape.cross),
				"Адрес с количеством бутылей 20-40 шт."
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("red", PointMarkerShape.star),
				PointMarker.GetIconPixbuf("orange", PointMarkerShape.star),
				PointMarker.GetIconPixbuf("green", PointMarkerShape.star),
				"Адрес с количеством бутылей более 40 шт." 
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("black", PointMarkerShape.circle),
				PointMarker.GetIconPixbuf("black", PointMarkerShape.triangle),
				PointMarker.GetIconPixbuf("black", PointMarkerShape.star),
				"Адрес не в маршрутном листе"
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("blue_stripes", PointMarkerShape.circle),
				PointMarker.GetIconPixbuf("blue_stripes", PointMarkerShape.triangle),
				PointMarker.GetIconPixbuf("blue_stripes", PointMarkerShape.star),
				"Адрес с временем доставки после 18:00"
			});
			ms.Add(new object[] {
				PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.circle),
				PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.triangle),
				PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.star),
				"График доставки продолжительностью менее часа"
			});

			lblInfo.Text = "Перетаскивание карты, правой кнопкой мыши.\n" +
				"Обычное(прямоугольное) выделение адресов на карте осуществляется перемещением мыши с нажатой левой кнопкой.\n" +
				"Для выделения по одному маркеру, зажмите Alt и левой кнопкой мыши для выделения\\удаления, кликните по нему\n" +
				"Для выделения полигоном(сложной формой), зажмите CTRL и левой кнопкой установите углы очерчивающие полигон. " +
				"В процессе работы CTRL можно отпускать и зажимат заново для добавления новых углов. " +
				"Уже зафиксированные углы полигона можно перетаскивать левой кнопкой мыши." + "\n\tОписание маркеров:";
			lblInfo.LineWrap = true;
			lblInfo.WidthRequest = 400;
			lblInfo.LineWrapMode = Pango.WrapMode.WordChar;

			treeMarkers.HeadersVisible = false;
			treeMarkers.ColumnsConfig = FluentColumnsConfig<object[]>
				.Create()
				.AddColumn("Маркер")
					.AddPixbufRenderer(x => x[0] as Pixbuf)
					.AddPixbufRenderer(x => x[1] as Pixbuf)
					.AddPixbufRenderer(x => x[2] as Pixbuf)
					.AddTextRenderer(x => "...")
				.AddColumn("Описание")
					.AddTextRenderer(x => " - "+(string)x[3])
				.Finish();
			treeMarkers.SetItemsSource(ms);
		}

		protected void OnBtnOkClicked(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}
