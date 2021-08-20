using Gamma.Utilities;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Xml;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public static class PrintRouteListHelper
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IRouteColumnRepository _routeColumnRepository = new RouteColumnRepository();
		private const string _orderCommentTagName = "OrderComment";
		private const string _orderPrioritizedTagName = "prioritized";

		public static ReportInfo GetRDLTimeList(int routeListId)
		{
			return new ReportInfo
			{
				Title = $"Лист времени для МЛ № { routeListId }",
				Identifier = "Documents.TimeList",
				Parameters = new Dictionary<string, object>
				{
					{ "route_list_id", routeListId }
				}
			};
		}

		public static ReportInfo GetRDLDailyList(int routeListId)
		{
			return new ReportInfo
			{
				Title = $"Ежедневные номера МЛ № { routeListId }",
				Identifier = "Logistic.AddressesByDailyNumber",
				Parameters = new Dictionary<string, object>
				{
					{ "route_list", routeListId }
				}
			};
		}

		public static ReportInfo GetRDLRouteList(IUnitOfWork uow, RouteList routeList)
		{
			var RouteColumns = _routeColumnRepository.ActiveColumns(uow);

			if(RouteColumns.Count < 1)
			{
				MessageDialogHelper.RunErrorDialog("В справочниках не заполнены колонки маршрутного листа. Заполните данные и повторите попытку.");
			}

			string documentName = "RouteList";
			bool isClosed = false;

			if(routeList.PrintAsClosed())
			{
				documentName = "ClosedRouteList";
				isClosed = true;
			}

			string RdlText = string.Empty;
			using(var rdr = new StreamReader(Path.Combine(Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl")))
			{
				RdlText = rdr.ReadToEnd();
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
				"<TextAlign>Center</TextAlign><Format>{2}</Format><VerticalAlign>Middle</VerticalAlign>";

			if(isClosed)
			{
				numericCellTemplate += "<BackgroundColor>=Iif((Fields!Status.Value = \"EnRoute\") or (Fields!Status.Value = \"Completed\"), White, Lightgrey)</BackgroundColor>";
			}
			numericCellTemplate += "<PaddingTop>10pt</PaddingTop><PaddingBottom>10pt</PaddingBottom></Style>" +
								   "<CanGrow>true</CanGrow></Textbox></ReportItems></TableCell>";

			//Расширяем требуемые колонки на нужную ширину
			RdlText = RdlText.Replace("<!--colspan-->", $"<ColSpan>{ RouteColumns.Count }</ColSpan>");

			//Расширяем таблицу
			string columnsXml = "<TableColumn><Width>18pt</Width></TableColumn>";
			string columns = string.Empty;

			columns += "<TableColumn><Width>100pt</Width></TableColumn>"; // Первая колонка шире тк кк это коммент
			for(int i = 1; i < RouteColumns.Count; i++)
			{
				columns += columnsXml;
			}
			RdlText = RdlText.Replace("<!--table_column-->", columns);

			//Создаем колонки, дополняем запрос и тд.
			string CellColumnHeader = string.Empty;
			string CellColumnValue = string.Empty;
			string CellColumnStock = string.Empty;
			string CellColumnTotal = string.Empty;
			string SqlSelect = string.Empty;
			string SqlSelectSubquery = string.Empty;
			string Fields = string.Empty;
			string TotalSum = "= 0";

			bool isFirstColumn = true;

			foreach(var column in RouteColumns)
			{
				//Заголовки колонок
				CellColumnHeader += string.Format(
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

				if(isFirstColumn)
				{
					CellColumnValue += string.Format(numericCellTemplate, TextBoxNumber++, $"=Iif({{{ _orderPrioritizedTagName }}}, \'Приоритет! \', \"\") + {{{ _orderCommentTagName }}}", "0");
				}
				else
				{
					if(isClosed)
						CellColumnValue += string.Format(numericCellTemplate,
							TextBoxNumber++,
							string.Format(
								"=Iif({{Water{0}}} + {{Water_fact{0}}} = 0, \"\", Iif( {{Water{0}}} = {{Water_fact{0}}}, Format({{Water{0}}}, '0'), ''" +
								" + {{Water_fact{0}}} + '(' + Iif({{Water_fact{0}}} - {{Water{0}}} > 0, '+', '') + ({{Water_fact{0}}} - {{Water{0}}}) + ')'))",
								column.Id),
							string.Format("=Iif({{Water{0}}} = {{Water_fact{0}}}, \"0\", \"\")", column.Id)
						);
					else
					{
						CellColumnValue += string.Format(numericCellTemplate,
							TextBoxNumber++, $"=Iif({{Water{ column.Id }}} = 0, \"\", {{Water{ column.Id }}})",
							'C');
					}
				}
				//Ячейка с запасом. Пока там пусто
				CellColumnStock += string.Format(numericCellTemplate, TextBoxNumber++, "", 0);

				//Ячейка с суммой по бутылям.
				if(isFirstColumn)
				{
					CellColumnTotal += string.Format(numericCellTemplate, TextBoxNumber++, "", 0);
				}
				else
				{
					string formula;
					if(isClosed)
					{
						formula = $"=Iif(Sum({{Water_fact{ column.Id }}}) = 0, \"\", Sum({{Water_fact{ column.Id }}}))";
					}
					else
					{
						formula = $"=Iif(Sum({{Water{ column.Id }}}) = 0, \"\", Sum({{Water{ column.Id }}}))";
					}

					CellColumnTotal += string.Format(numericCellTemplate, TextBoxNumber++, formula, 0);
				}

				//Запрос..
				if(isFirstColumn)
				{
					SqlSelect += $", orders.comment AS { _orderCommentTagName }" +
						$", (SELECT EXISTS (" +
						$"SELECT * FROM undelivered_orders uo" +
						" WHERE uo.guilty_is IN('Driver','Department')" +
						" AND uo.new_order_id = orders.id" +
						$")) AS { _orderPrioritizedTagName }";

					Fields +=
						$"<Field Name=\"{ _orderPrioritizedTagName }\">" +
						$"<DataField>{ _orderPrioritizedTagName }</DataField>" +
						"<TypeName>System.Boolean</TypeName>" +
						"</Field>";

					Fields +=
						$"<Field Name=\"{ _orderCommentTagName }\">" +
						$"<DataField>{ _orderCommentTagName }</DataField>" +
						"<TypeName>System.String</TypeName>" +
						"</Field>";

					isFirstColumn = false;
				}
				else
				{
					SqlSelect += string.Format(", IFNULL(wt_qry.Water{0}, 0) AS Water{0}", column.Id.ToString());
					SqlSelectSubquery += string.Format(
						", SUM(IF(nomenclature_route_column.id = {0}, cast(order_items.count as DECIMAL), 0)) AS {1}",
						column.Id, "Water" + column.Id.ToString());
					if(isClosed)
					{
						SqlSelect +=
							string.Format(
								", IF(route_list_addresses.status = 'Transfered', 0, cast(IFNULL(wt_qry.Water_fact{0}, 0) as DECIMAL)) AS Water_fact{0}",
								column.Id.ToString());
						SqlSelectSubquery += string.Format(
							", SUM(IF(nomenclature_route_column.id = {0}, cast(IFNULL(order_items.actual_count, 0) as DECIMAL), 0)) AS {1}",
							column.Id, "Water_fact" + column.Id.ToString());
					}

					//Линкуем запрос на переменные RDL
					Fields += string.Format("" +
											"<Field Name=\"{0}\">" +
											"<DataField>{0}</DataField>" +
											"<TypeName>System.Decimal</TypeName>" +
											"</Field>", "Water" + column.Id.ToString());
					if(isClosed)
					{
						Fields += string.Format("<Field Name=\"{0}\">" +
												"<DataField>{0}</DataField>" +
												"<TypeName>System.Decimal</TypeName>" +
												"</Field>", "Water_fact" + column.Id.ToString());
					}
				}
				//Формула итоговой суммы по всем бутылям.
				if(_routeColumnRepository.NomenclaturesForColumn(uow, column).Any(x => x.Category == NomenclatureCategory.water && x.TareVolume == TareVolume.Vol19L))
				{
					if(isClosed)
					{
						TotalSum += $"+ Sum(Iif(Fields!Status.Value = \"Completed\", {{Water_fact{ column.Id }}}, 0))";
					}
					else
					{
						TotalSum += $"+ Sum({{Water{ column.Id }}})";
					}
				}
			}
			RdlText = RdlText.Replace("<!--table_cell_name-->", CellColumnHeader);
			RdlText = RdlText.Replace("<!--table_cell_value-->", CellColumnValue);
			RdlText = RdlText.Replace("<!--table_cell_stock-->", CellColumnStock);
			RdlText = RdlText.Replace("<!--table_cell_total-->", CellColumnTotal);
			RdlText = RdlText.Replace("<!--sql_select-->", SqlSelect);
			RdlText = RdlText.Replace("<!--sql_select_subquery-->", SqlSelectSubquery);
			RdlText = RdlText.Replace("<!--fields-->", Fields);
			RdlText = RdlText.Replace("<!--table_cell_total_without_stock-->", TotalSum);

			var TempFile = Path.GetTempFileName();
			using(StreamWriter sw = new StreamWriter(TempFile))
			{
				sw.Write(RdlText);
			}
#if DEBUG
			Console.WriteLine(RdlText);
#endif

			string printDatestr = $"Дата печати: { DateTime.Now.ToString("g") }";
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			return new ReportInfo
			{
				Title = $"Маршрутный лист № { routeList.Id }",
				Path = TempFile,
				Parameters = new Dictionary<string, object>
				{
					{ "RouteListId", routeList.Id },
					{ "Print_date", printDatestr},
					{ "RouteListDate", routeList.Date},
					{ "need_terminal", needTerminal }
				}
			};
		}

		public static ReportInfo GetRDLRouteMap(IUnitOfWork uow, RouteList routeList, bool batchPrint)
		{
			string documentName = "RouteMap";

			XmlDocument rdlText = new XmlDocument();
			XmlNamespaceManager namespaces = new XmlNamespaceManager(rdlText.NameTable);
			namespaces.AddNamespace("r", "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition");
			rdlText.Load(Path.Combine(Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl"));
			var imageData = rdlText.DocumentElement.SelectSingleNode("/r:Report/r:EmbeddedImages/r:EmbeddedImage[@Name=\"map\"]/r:ImageData", namespaces);

			var map = new GMapControl
			{
				MapProvider = GMapProviders.OpenCycleMap,
				MaxZoom = 18,
				RoutesEnabled = true,
				MarkersEnabled = true
			};

			GMapOverlay routeOverlay = new GMapOverlay("route");
			using(var calc = new RouteGeometryCalculator(DistanceProvider.Osrm))
			{
				MapDrawingHelper.DrawRoute(routeOverlay, routeList, calc);
			}

			GMapOverlay addressesOverlay = new GMapOverlay("addresses");
			MapDrawingHelper.DrawAddressesOfRoute(addressesOverlay, routeList);
			map.Overlays.Add(routeOverlay);
			map.Overlays.Add(addressesOverlay);
			map.SetFakeAllocationSize(new Gdk.Rectangle(0, 0, 900, 900));
			map.ZoomAndCenterRoutes("route");
			byte[] img;
			using(var bitmap = map.ToBitmap((int count) => _logger.Info("Загружаем плитки карты(осталось {0})...", count), !batchPrint))
			{
				using(MemoryStream stream = new MemoryStream())
				{
					bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					img = stream.ToArray();
				}
			}

			string base64image = Convert.ToBase64String(img);
			imageData.InnerText = base64image;

			return new ReportInfo
			{
				Title = $"Карта маршрута № { routeList.Id }",
				Source = rdlText.InnerXml,
				Parameters = new Dictionary<string, object>
				{
					{ "route_id", routeList.Id }
				}
			};
		}

		public static ReportInfo GetRDLLoadDocument(int routeListId)
		{
			return new ReportInfo
			{
				Title = $"Документ погрузки для МЛ № { routeListId }",
				Identifier = "RouteList.CarLoadDocument",
				Parameters = new Dictionary<string, object>
				{
					{ "route_list_id", routeListId },
				},
				UseUserVariables = true
			};
		}

		public static ReportInfo GetRDLLoadSofiyskaya(int routeListId)
		{
			return new ReportInfo
			{
				Title = $"Погрузка Софийская для МЛ № { routeListId }",
				Identifier = "RouteList.CarLoadDocSofiyskaya",
				Parameters = new Dictionary<string, object>
				{
					{ "id", routeListId },
				},
				UseUserVariables = true
			};
		}

		public static ReportInfo GetRDLFine(RouteList routeList)
		{

			return new ReportInfo
			{
				Title = $"Штрафы сотрудника { routeList.Driver.LastName }",
				Identifier = "Employees.Fines",
				Parameters = new Dictionary<string, object>
				{
					{ "drivers", routeList.Driver.Id },
					{ "startDate", routeList.Date },
					{ "endDate", routeList.Date },
					{ "routelist", routeList.Id },
					{ "showbottom", true}
				},
				UseUserVariables = true
			};
		}

		public static ReportInfo GetRDL(RouteList routeList, RouteListPrintableDocuments type, IUnitOfWork uow = null, bool batchPrint = false)
		{
			switch(type)
			{
				case RouteListPrintableDocuments.LoadDocument:
					return GetRDLLoadDocument(routeList.Id);
				case RouteListPrintableDocuments.LoadSofiyskaya:
					return GetRDLLoadSofiyskaya(routeList.Id);
				case RouteListPrintableDocuments.RouteList:
					return GetRDLRouteList(uow, routeList);
				case RouteListPrintableDocuments.RouteMap:
					return GetRDLRouteMap(uow, routeList, batchPrint);
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

	[Flags]
	public enum RouteListPrintableDocuments
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Карта маршрута")]
		RouteMap,
		[Display(Name = "Адреса по ежедневным номерам")]
		DailyList,
		[Display(Name = "Лист времени")]
		TimeList,
		[Display(Name = "Документ погрузки")]
		LoadDocument,
		[Display(Name = "Погрузка Софийская")]
		LoadSofiyskaya,
		[Display(Name = "Отчёт по порядку адресов")]
		OrderOfAddresses
	}

	public class RouteListPrintableDocs : IPrintableRDLDocument
	{
		IUnitOfWork _uow;
		public RouteList routeList;
		RouteListPrintableDocuments _type;

		public RouteListPrintableDocs(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments type)
		{
			_uow = uow;
			this.routeList = routeList;
			_type = type;
			CopiesToPrint = DefaultCopies;
		}

		#region IPrintableRDLDocument implementation 
		public ReportInfo GetReportInfo() => PrintRouteListHelper.GetRDL(routeList, _type, _uow, IsBatchPrint);
		public Dictionary<object, object> Parameters { get; set; } = new Dictionary<object, object>();
		#endregion

		#region IPrintableDocument implementation 
		public PrinterType PrintType => PrinterType.RDL;

		public string Name => _type.GetEnumTitle();

		public int CopiesToPrint { get; set; }
		#endregion
		
		bool IsBatchPrint => Parameters.ContainsKey("IsBatchPrint") && (bool)Parameters["IsBatchPrint"];
		
		public DocumentOrientation Orientation
		{
			get
			{
				switch(_type)
				{
					case RouteListPrintableDocuments.RouteList:
					case RouteListPrintableDocuments.OrderOfAddresses:
						return DocumentOrientation.Landscape;
					default:
						return DocumentOrientation.Portrait;
				}
			}
		}

		int DefaultCopies
		{
			get
			{
				switch(_type)
				{
					case RouteListPrintableDocuments.RouteList:
					case RouteListPrintableDocuments.RouteMap:
					case RouteListPrintableDocuments.DailyList:
					case RouteListPrintableDocuments.TimeList:
					case RouteListPrintableDocuments.LoadDocument:
					case RouteListPrintableDocuments.LoadSofiyskaya:
					case RouteListPrintableDocuments.OrderOfAddresses:
						return 1;
					default:
						throw new NotImplementedException("Документ не поддерживается");
				}
			}
		}
	}
}
