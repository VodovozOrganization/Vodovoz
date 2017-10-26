using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Xml;
using Gamma.Utilities;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Additions.Logistic
{
	public static class PrintRouteListHelper
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static ReportInfo GetRDLTimeList(int routeListId)
		{
			return new ReportInfo {
				Title = String.Format ("Лист времени для МЛ № {0}", routeListId),
				Identifier = "Documents.TimeList",
				Parameters = new Dictionary<string, object> {
					{ "route_list_id", routeListId }
				}
			};
		}

		public static ReportInfo GetRDLDailyList(int routeListId)
		{
			return new ReportInfo {
				Title = String.Format("Ежедневные номера МЛ № {0}", routeListId),
				Identifier = "Logistic.AddressesByDailyNumber",
				Parameters = new Dictionary<string, object> {
					{ "route_list", routeListId }
				}
			};
		}

		public static ReportInfo GetRDLRouteList(IUnitOfWork uow, RouteList routeList)
		{
			var RouteColumns = RouteColumnRepository.ActiveColumns (uow);

			if (RouteColumns.Count < 1)
				MessageDialogWorks.RunErrorDialog ("В справочниках не заполнены колонки маршрутного листа. Заполните данные и повторите попытку.");

			string documentName = "RouteList";
			bool isClosed = false;

			switch (routeList.Status)
			{
				case RouteListStatus.OnClosing:
				case RouteListStatus.MileageCheck:
				case RouteListStatus.Closed:
					documentName = "ClosedRouteList";
					isClosed = true;
					break;
			}

			string RdlText = String.Empty;
			using (var rdr = new StreamReader (System.IO.Path.Combine (Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl"))) {
				RdlText = rdr.ReadToEnd ();
			}
			//Для уникальности номеров Textbox.
			int TextBoxNumber = 100;

			//Шаблон стандартной ячейки
			string numericCellTemplate =
				"<TableCell><ReportItems>" +
			    "<Textbox Name=\"Textbox{0}\">" +
			    "<Value>{1}</Value>" +
			    "<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
			    "<BorderStyle><Default>Solid</Default></BorderStyle><BorderColor /><BorderWidth /><FontSize>8pt</FontSize>" +
				"<TextAlign>Center</TextAlign><Format>{2}</Format>";

			if (isClosed)
			{
				numericCellTemplate += "<BackgroundColor>=Iif((Fields!Status.Value = \"EnRoute\") or (Fields!Status.Value = \"Completed\"), White, Lightgrey)</BackgroundColor>";
			}				
			numericCellTemplate += "</Style></Textbox></ReportItems></TableCell>";

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
				//'' + {{Water_fact{0}}} + '(' + ({{Water_fact{0}}} - {{Water{0}}}) + ')'
				if(isClosed)
					CellColumnValue += String.Format (numericCellTemplate,
					                                  TextBoxNumber++, 
					                                  String.Format ("=Iif({{Water{0}}} + {{Water_fact{0}}} = 0, \"\", Iif( {{Water{0}}} = {{Water_fact{0}}}, Format({{Water{0}}}, '0'), '' + {{Water_fact{0}}} + '(' + ({{Water_fact{0}}} - {{Water{0}}}) + ')'))", column.Id),
					                                  String.Format ("=Iif({{Water{0}}} = {{Water_fact{0}}}, \"0\", \"\")", column.Id)
					                                 );
				else
					CellColumnValue += String.Format (numericCellTemplate,
					TextBoxNumber++, String.Format("=Iif({{Water{0}}} = 0, \"\", {{Water{0}}})", column.Id ), 'C');
				//Ячейка с запасом. Пока там пусто
				CellColumnStock += String.Format (numericCellTemplate,
				                                  TextBoxNumber++, "", 0);
				
				//Ячейка с суммой по бутылям.
				string formula;
				if (isClosed)
					formula = String.Format ("=Iif(Sum({{Water_fact{0}}}) = 0, \"\", Sum({{Water_fact{0}}}))", column.Id);
				else
					formula = String.Format ("=Iif(Sum({{Water{0}}}) = 0, \"\", Sum({{Water{0}}}))", column.Id);
				CellColumnTotal += String.Format (numericCellTemplate, TextBoxNumber++, formula, 0);

				//Запрос..
				SqlSelect += String.Format (", IFNULL(wt_qry.Water{0}, 0) AS Water{0}", column.Id.ToString ());
				SqlSelectSubquery += String.Format (", SUM(IF(nomenclature_route_column.id = {0}, order_items.count, 0)) AS {1}",
					column.Id, "Water" + column.Id.ToString ());
				if(isClosed)
				{
					SqlSelect += String.Format (", IFNULL(wt_qry.Water_fact{0}, 0) AS Water_fact{0}", column.Id.ToString ());
					SqlSelectSubquery += String.Format (", SUM(IF(nomenclature_route_column.id = {0}, order_items.actual_count, 0)) AS {1}",
						column.Id, "Water_fact" + column.Id.ToString ());
				}
				//Линкуем запрос на переменные RDL
				Fields += String.Format ("" +
					"<Field Name=\"{0}\">" +
					"<DataField>{0}</DataField>" +
					"<TypeName>System.Int32</TypeName>" +
					"</Field>", "Water" + column.Id.ToString ());
				if(isClosed)
				{
					Fields += String.Format ("" +
						"<Field Name=\"{0}\">" +
						"<DataField>{0}</DataField>" +
						"<TypeName>System.Int32</TypeName>" +
						"</Field>", "Water_fact" + column.Id.ToString ());
				}
				//Формула итоговой суммы по всем бутялым.
				if(RouteColumnRepository.NomenclaturesForColumn(uow, column).Any(x => x.Category == Vodovoz.Domain.Goods.NomenclatureCategory.water))
				{
					if(isClosed)
						TotalSum += $"+ Sum(Iif(Fields!Status.Value = \"Completed\", {{Water_fact{column.Id}}}, 0))";
					else
						TotalSum += $"+ Sum({{Water{column.Id}}})";
				}
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

			return new ReportInfo {
				Title = String.Format ("Маршрутный лист № {0}", routeList.Id),
				Path = TempFile,
				Parameters = new Dictionary<string, object> {
					{ "RouteListId", routeList.Id }
				}
			};
		}

		public static ReportInfo GetRDLRouteMap(IUnitOfWork uow, RouteList routeList)
		{
			string documentName = "RouteMap";

			XmlDocument rdlText = new XmlDocument();
			XmlNamespaceManager namespaces = new XmlNamespaceManager(rdlText.NameTable);
			namespaces.AddNamespace("r", "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition");
			rdlText.Load(System.IO.Path.Combine(Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl"));
			var imageData = rdlText.DocumentElement.SelectSingleNode("/r:Report/r:EmbeddedImages/r:EmbeddedImage[@Name=\"map\"]/r:ImageData", namespaces);

			var map = new GMapControl();
			map.MapProvider = GMapProviders.OpenCycleMap;
			map.MaxZoom = 18;
			map.RoutesEnabled = true;
			map.MarkersEnabled = true;

			GMapOverlay routeOverlay = new GMapOverlay("route");
			MapDrawingHelper.DrawRoute(routeOverlay, routeList, new Tools.Logistic.RouteGeometryCalculator(Tools.Logistic.DistanceProvider.Osrm));
			GMapOverlay addressesOverlay = new GMapOverlay("addresses");
			MapDrawingHelper.DrawAddressesOfRoute(addressesOverlay, routeList);
			map.Overlays.Add(routeOverlay);
			map.Overlays.Add(addressesOverlay);
			map.SetFakeAllocationSize(new Gdk.Rectangle(0, 0, 900, 900));
			map.ZoomAndCenterRoutes("route");
			byte[] img;
			using(var bitmap = map.ToBitmap((int count) => logger.Info("Загружаем плитки карты(осталось {0})...", count))) {
				using(MemoryStream stream = new MemoryStream()) {
					bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					img = stream.ToArray();
				}
			}

			string base64image = Convert.ToBase64String(img);
			imageData.InnerText = base64image;

			return new ReportInfo {
				Title = String.Format("Карта маршрута № {0}", routeList.Id),
				Source = rdlText.InnerXml,
				Parameters = new Dictionary<string, object> {
					{ "route_id", routeList.Id }
				}
			};
		}

		public static ReportInfo GetRDLLoadDocument(int routeListId)
		{
			return new ReportInfo {
				Title = String.Format ("Документ погрузки для МЛ № {0}", routeListId),
				Identifier = "RouteList.CarLoadDocument",
				Parameters = new Dictionary<string, object> {
					{ "route_list_id", routeListId },
				},
				UseUserVariables = true
			};
		}

		public static ReportInfo GetRDLFine(RouteList routeList)
		{
			
			return new ReportInfo {
				Title = String.Format("Штрафы сотрудника {0}", routeList.Driver.LastName),
				Identifier = "Employees.Fines",
				Parameters = new Dictionary<string, object> {
					{ "drivers", routeList.Driver.Id },
					{ "startDate", routeList.Date },
					{ "endDate", routeList.Date },
					{ "routelist", routeList.Id },
					{ "showbottom", true}
				},
				UseUserVariables = true
			};
		}

		public static ReportInfo GetRDL(RouteList routeList, RouteListPrintableDocuments type, IUnitOfWork uow = null)
		{
			switch(type) {
				case RouteListPrintableDocuments.LoadDocument:
					return GetRDLLoadDocument(routeList.Id);
				case RouteListPrintableDocuments.RouteList:
					return GetRDLRouteList(uow, routeList);
				case RouteListPrintableDocuments.RouteMap:
					return GetRDLRouteMap(uow, routeList);
				case RouteListPrintableDocuments.TimeList:
					return GetRDLTimeList(routeList.Id);
				case RouteListPrintableDocuments.DailyList:
					return GetRDLDailyList(routeList.Id);
				case RouteListPrintableDocuments.OrderOfAddresses:
					return routeList.OrderOfAddressesRep(routeList.Id);
				default:
					throw new NotImplementedException("Неизвестный тип документа");
			}
		}
	}

	public enum RouteListPrintableDocuments
	{
		[Display (Name = "Все")]
		All,
		[Display (Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Карта маршрута")]
		RouteMap,
		[Display(Name = "Адреса по ежедневным номерам")]
		DailyList,
		[Display (Name = "Лист времени")]
		TimeList,
		[Display (Name = "Документ погрузки")]
		LoadDocument,
		[Display(Name = "Отчёт по порядку адресов")]
		OrderOfAddresses
	}

	public class RouteListPrintableDocs : IPrintableDocument
	{
		public RouteListPrintableDocs(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments type)
		{
			this.UoW 		 = uow;
			this.routeList 	 = routeList;
			this.type 		 = type;
		}

		#region IPrintableDocument implementation

		public ReportInfo GetReportInfo()
		{
			return PrintRouteListHelper.GetRDL(routeList, type, UoW);
		}

		public ReportInfo GetReportInfoForPreview()
		{
			return GetReportInfo();
		}

		public PrinterType PrintType {
			get	{ return PrinterType.RDL; }
		}

		public DocumentOrientation Orientation {
			get
			{
				if (type == RouteListPrintableDocuments.RouteList)
					return DocumentOrientation.Landscape;
				return DocumentOrientation.Portrait;
			}
		}

		public string Name {
			get
			{
				return RouteListPrintableDocuments.LoadDocument.GetEnumTitle();
			}
		}

		#endregion

		private IUnitOfWork UoW;
		private RouteList routeList;
		private RouteListPrintableDocuments type;

	}
}

