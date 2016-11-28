using System;
using System.Collections.Generic;
using System.IO;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSTDI;
using Vodovoz.Repository.Logistics;
using System.Linq;

namespace Vodovoz.Additions.Logistic
{
	public static class PrintRouteListHelper
	{
		public static void Print(IUnitOfWork uow, int routeListId, ITdiTab myTab)
		{
			ShowAndPrintRouteList(uow, routeListId, myTab);
			ShowAndPrintTimeList(routeListId, myTab);
		}

		public static void ShowAndPrintTimeList(int routeListId, ITdiTab myTab)
		{
			var document = new QSReport.ReportInfo {
				Title = String.Format ("Лист времени для МЛ№ {0}", routeListId),
				Identifier = "Documents.TimeList",
				Parameters = new Dictionary<string, object> {
					{ "route_list_id", routeListId }
				}
			};

			myTab.TabParent.OpenTab(
				TdiTabBase.GenerateHashName<ReportViewDlg>(),
				() => new QSReport.ReportViewDlg(document));
		}

		public static void ShowAndPrintRouteList(IUnitOfWork uow, int routeListId, ITdiTab myTab)
		{
			var RouteColumns = RouteColumnRepository.ActiveColumns (uow);

			if (RouteColumns.Count < 1)
				MessageDialogWorks.RunErrorDialog ("В справочниках не заполнены колонки маршрутного листа. Заполните данные и повторите попытку.");

			string RdlText = String.Empty;
			using (var rdr = new StreamReader (System.IO.Path.Combine (Environment.CurrentDirectory, "Reports/RouteList.rdl"))) {
				RdlText = rdr.ReadToEnd ();
			}
			//Для уникальности номеров Textbox.
			int TextBoxNumber = 100;

			//Шаблон стандартной ячейки
			const string CellTemplate = "<TableCell><ReportItems>" +
				"<Textbox Name=\"Textbox{0}\">" +
				"<Value>{1}</Value>" +
				"<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
				"<BorderStyle><Default>Solid</Default></BorderStyle><BorderColor /><BorderWidth />" +
				"<TextAlign>Center</TextAlign></Style></Textbox></ReportItems></TableCell>";

			//Расширяем требуемые колонки на нужную ширину
			RdlText = RdlText.Replace ("<!--colspan-->", String.Format ("<ColSpan>{0}</ColSpan>", RouteColumns.Count));

			//Расширяем таблицу
			string columnsXml = "<TableColumn><Width>20pt</Width></TableColumn>";
			string columns = String.Empty;
			for (int i = 0; i < RouteColumns.Count; i++) {
				columns += columnsXml;
			}
			RdlText = RdlText.Replace ("<!--table_column-->", columns);

			//Создаем колонки, дополняем запрос и тд.
			string CellColumnHeader = String.Empty;
			string CellColumnValue = String.Empty;
			string CellColumnStock = String.Empty;
			string CellColumnTotal = String.Empty;
			string SqlSelect = String.Empty;
			string SqlSelectSubquery = String.Empty;
			string Fields = String.Empty;
			string TotalSum = "= 0";
			foreach (var column in RouteColumns) {
				//Заголовки колонок
				CellColumnHeader += String.Format (
					"<TableCell><ReportItems>" +
					"<Textbox Name=\"Textbox{0}\">" +
					"<Value>{1}</Value>" +
					"<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
					"<BorderStyle><Default>Solid</Default><Top>Solid</Top><Bottom>Solid</Bottom></BorderStyle>" +
					"<BorderColor /><BorderWidth /><FontSize>8pt</FontSize><TextAlign>Center</TextAlign></Style>" +
					"<CanGrow>true</CanGrow></Textbox></ReportItems></TableCell>", 
					TextBoxNumber++, column.Name);
				//Формула для колонки с водой для информации из запроса
				CellColumnValue += String.Format (CellTemplate,
					TextBoxNumber++, String.Format("=Iif({{Water{0}}} = 0, \"\", {{Water{0}}})", column.Id ));
				//Ячейка с запасом. Пока там пусто
				CellColumnStock += String.Format (CellTemplate,
					TextBoxNumber++, "");
				//Ячейка с суммой по бутылям + запасы.
				CellColumnTotal += String.Format (CellTemplate,
					TextBoxNumber++, String.Format("=Iif(Sum({{Water{0}}}) = 0, \"\", Sum({{Water{0}}}))", column.Id ));
				//Запрос..
				SqlSelect += String.Format (", IFNULL(wt_qry.Water{0}, 0) AS Water{0}", column.Id.ToString ());
				SqlSelectSubquery += String.Format (", SUM(IF(nomenclature_route_column.id = {0}, order_items.count, 0)) AS {1}",
					column.Id, "Water" + column.Id.ToString ());
				//Линкуем запрос на переменные RDL
				Fields += String.Format ("" +
					"<Field Name=\"{0}\">" +
					"<DataField>{0}</DataField>" +
					"<TypeName>System.Int32</TypeName>" +
					"</Field>", "Water" + column.Id.ToString ());
				//Формула итоговой суммы по всем бутялым.
				if(RouteColumnRepository.NomenclaturesForColumn(uow, column).Any(x => x.Category == Vodovoz.Domain.Goods.NomenclatureCategory.water))
					TotalSum += "+ Sum({Water" + column.Id.ToString () + "})";
			}
			RdlText = RdlText.Replace ("<!--table_cell_name-->", CellColumnHeader);
			RdlText = RdlText.Replace ("<!--table_cell_value-->", CellColumnValue);
			RdlText = RdlText.Replace ("<!--table_cell_stock-->", CellColumnStock);
			RdlText = RdlText.Replace ("<!--table_cell_total-->", CellColumnTotal);
			RdlText = RdlText.Replace ("<!--sql_select-->", SqlSelect);
			RdlText = RdlText.Replace ("<!--sql_select_subquery-->", SqlSelectSubquery);
			RdlText = RdlText.Replace ("<!--fields-->", Fields);
			RdlText = RdlText.Replace ("<!--table_cell_total_without_stock-->", TotalSum);

			var TempFile = System.IO.Path.GetTempFileName ();
			using (StreamWriter sw = new StreamWriter (TempFile)) {
				sw.Write (RdlText);
			}
			#if DEBUG
			Console.WriteLine(RdlText);
			#endif

			var document = new QSReport.ReportInfo {
				Title = String.Format ("Маршрутный лист № {0}", routeListId),
				Path = TempFile,
				Parameters = new Dictionary<string, object> {
					{ "RouteListId", routeListId }
				}
			};

			myTab.TabParent.OpenTab(
				TdiTabBase.GenerateHashName<ReportViewDlg>(),
				() => new QSReport.ReportViewDlg(document));
		}

		public static void ShowAndPrintWater(int routeListId, ITdiTab myTab)
		{
			var document = new QSReport.ReportInfo {
				Title = String.Format ("Выгрузка для МЛ№ {0}", routeListId),
				Identifier = "RouteList.CarLoadDocument",
				Parameters = new Dictionary<string, object> {
					{ "route_list_id", routeListId },
					{ "nomenclature_category", "water" }
				}
			};

			myTab.TabParent.OpenTab(
				TdiTabBase.GenerateHashName<ReportViewDlg>(),
				() => new QSReport.ReportViewDlg(document, true));
		}
	}
}

