using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gdk;
using Vodovoz.Additions.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class RouresAtDayInfoWnd : Gtk.Window
	{
		public RouresAtDayInfoWnd() : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			ConfigureWnd();
		}

		void ConfigureWnd(){
			var ms = new List<object[]> {
				new object[] {
					PointMarker.GetIconPixbuf("vodonos", PointMarkerShape.custom),
					PointMarker.GetIconPixbuf("vodonos", PointMarkerShape.custom),
					PointMarker.GetIconPixbuf("vodonos", PointMarkerShape.custom),
					PointMarker.GetIconPixbuf("vodonos", PointMarkerShape.custom),
					"База\\Склад погрузки-разгрузки"
				},
				new object[] {
					PointMarker.GetIconPixbuf("red", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("red", PointMarkerShape.overduetriangle),
					"Адрес с количеством бутылей не более 5 шт."
				},
				new object[] {
					PointMarker.GetIconPixbuf("red", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.overduecircle),
					"Адрес с количеством бутылей 6-9 шт."
				},
				new object[] {
					PointMarker.GetIconPixbuf("red", PointMarkerShape.square),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.square),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.square),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.overduesquare),
					"Адрес с количеством бутылей 10-19 шт."
				},
				new object[] {
					PointMarker.GetIconPixbuf("red", PointMarkerShape.cross),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.cross),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.cross),
					PointMarker.GetIconPixbuf("red", PointMarkerShape.overduecross),
					"Адрес с количеством бутылей 20-39 шт."
				},
				new object[] {
					PointMarker.GetIconPixbuf("red", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("green", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("orange", PointMarkerShape.overduestar),
					"Адрес с количеством бутылей не менее 40 шт."
				},
					new object[] {
					PointMarker.GetIconPixbuf("black", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("black", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("black", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("black", PointMarkerShape.overduecircle),
					"Адрес не в маршрутном листе"
				},
				new object[] {
					PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("black_and_red", PointMarkerShape.overduecircle),
					"График доставки продолжительностью менее часа"
				},
				new object[] {
					PointMarker.GetIconPixbuf("red_stripes", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("red_stripes", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("red_stripes", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("red_stripes", PointMarkerShape.overduecircle),
					"Адрес с временем доставки до 12:00"
				},
				new object[] {
					PointMarker.GetIconPixbuf("yellow_stripes", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("yellow_stripes", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("yellow_stripes", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("yellow_stripes", PointMarkerShape.overduecircle),
					"Адрес с временем доставки до 15:00"
				},
				new object[] {
					PointMarker.GetIconPixbuf("green_stripes", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("green_stripes", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("green_stripes", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("green_stripes", PointMarkerShape.overduecircle),
					"Адрес с временем доставки до 18:00"
				},
				new object[] {
					PointMarker.GetIconPixbuf("grey_stripes", PointMarkerShape.circle),
					PointMarker.GetIconPixbuf("grey_stripes", PointMarkerShape.triangle),
					PointMarker.GetIconPixbuf("grey_stripes", PointMarkerShape.star),
					PointMarker.GetIconPixbuf("grey_stripes", PointMarkerShape.overduecircle),
					"Адрес с временем доставки после 18:00"
				}
			};

			var logisticsRequrementsMarkers = new List<object[]> {
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_forwarder", PointMarkerShape.custom),
					"Требуется экспедитор на адресе: \"Э\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_documents", PointMarkerShape.custom),
					"Требуется наличие паспорта/документов у водителя: \"Д\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_nationality", PointMarkerShape.custom),
					"Требуется русский водитель: \"Р\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_pass", PointMarkerShape.custom),
					"Требуется пропуск: \"П\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_largus", PointMarkerShape.custom),
					"Требуется Ларгус (газель не проедет): \"Л\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("logistics_requirements_many", PointMarkerShape.custom),
					"Несколько требований в одном заказе: \"Восклицательный знак\" на карте"
				}
			};

			var orderInfoMarkers = new List<object[]> {
				new object[] {
					PointMarker.GetIconPixbuf("order_info_small_bottles", PointMarkerShape.custom),
					"Мелкотарная продукция в заказе: \"М\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("order_info_cooler", PointMarkerShape.custom),
					"Кулер в заказе: \"К\" на карте"
				},
				new object[] {
					PointMarker.GetIconPixbuf("order_info_many", PointMarkerShape.custom),
					"В заказе имеется и кулер и мелкотарная продукция: \"Восклицательный знак\" на карте"
				}
			};

			lblInfo.Text = "Перетаскивание карты, правой кнопкой мыши.\n" +
				"Обычное(прямоугольное) выделение адресов на карте осуществляется перемещением мыши с нажатой левой кнопкой.\n" +
				"Для выделения по одному маркеру, включите CAPS LOCK и левой кнопкой мыши для выделения\\удаления, кликните по нему\n" +
				"Для выделения полигоном(сложной формой), зажмите CTRL и левой кнопкой установите углы очерчивающие полигон. " +
				"В процессе работы CTRL можно отпускать и зажимат заново для добавления новых углов. " +
				"Уже зафиксированные углы полигона можно перетаскивать левой кнопкой мыши." + "\n\tОписание маркеров:";
			labelOrdersInfo.Text = "Маркеры в виде звезды - переносы из недовозов, возникших по вине водителей, отделов  ВВ, форс-мажора и ДЗЧ" +
				"\n -Оперативная сводка не включает в себя:" +
				"\n- Самовывозы" +
				"\n- Закрывающие документы" +
				"\n- Выезды мастера" +
				"\n- Заказы, закрытые без доставки" +
				"\n- Статусы Новый, Отменен, Ожидание оплаты";
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
				.AddPixbufRenderer(x => x[3] as Pixbuf)
					.AddTextRenderer(x => "...")
				.AddColumn("Описание")
					.AddTextRenderer(x => " - "+(string)x[4])
				.Finish();
			treeMarkers.SetItemsSource(ms);

			ytreeviewLogisticsRequrementsMarkers.HeadersVisible = false;
			ytreeviewLogisticsRequrementsMarkers.ColumnsConfig = FluentColumnsConfig<object[]>
				.Create()
				.AddColumn("Маркер")
					.AddPixbufRenderer(x => x[0] as Pixbuf)
				.AddColumn("Описание")
					.AddTextRenderer(x => " - " + (string)x[1])
				.Finish();
			ytreeviewLogisticsRequrementsMarkers.SetItemsSource(logisticsRequrementsMarkers);

			ytreeviewOrderInfoMarkers.HeadersVisible = false;
			ytreeviewOrderInfoMarkers.ColumnsConfig = FluentColumnsConfig<object[]>
				.Create()
				.AddColumn("Маркер")
					.AddPixbufRenderer(x => x[0] as Pixbuf)
				.AddColumn("Описание")
					.AddTextRenderer(x => " - " + (string)x[1])
				.Finish();
			ytreeviewOrderInfoMarkers.SetItemsSource(orderInfoMarkers);
		}

		protected void OnBtnOkClicked(object sender, EventArgs e)
		{
			this.Destroy();
		}
	}
}
