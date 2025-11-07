using Autofac;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Presentation.Reports.Factories;
using Vodovoz.Settings.Common;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Infrastructure.Print;
using QS.Osrm;

namespace Vodovoz.Additions.Logistic
{
	public static class PrintRouteListHelper
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IRouteColumnRepository _routeColumnRepository = ScopeProvider.Scope.Resolve<IRouteColumnRepository>();
		private static readonly IGeneralSettings _generalSettingsSettings = ScopeProvider.Scope.Resolve<IGeneralSettings>();
		private static readonly ICachedDistanceRepository _cachedDistanceRepository = ScopeProvider.Scope.Resolve<ICachedDistanceRepository>();
		private static readonly IReportInfoFactory _reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
		private const string _orderCommentTagName = "OrderComment";
		private const string _orderPrioritizedTagName = "prioritized";
		private const string _waterTagNamePrefix = "Water";
		private const string _waterFactTagPrefix = "Water_fact";

		public static ReportInfo GetRDLRouteList(IUnitOfWork uow, RouteList routeList)
		{
			var RouteColumns = _routeColumnRepository.ActiveColumns(uow);

			if(RouteColumns.Count < 1)
			{
				MessageDialogHelper.RunErrorDialog("В справочниках не заполнены колонки маршрутного листа. Заполните данные и повторите попытку.");
			}

			string documentName = "RouteList";
			bool isClosed = false;
			var commentColSpanCount = 8; // число столбцов для объединения в строке с комментарием

			if(routeList.PrintAsClosed())
			{
				documentName = "ClosedRouteList";
				isClosed = true;
				commentColSpanCount = 9;
			}

			string RdlText = string.Empty;
			using(var rdr = new StreamReader(Path.Combine(Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl")))
			{
				RdlText = rdr.ReadToEnd();
			}
			//Для уникальности номеров Textbox.
			int TextBoxNumber = 1000;

			//Расширяем требуемые колонки на нужную ширину
			RdlText = RdlText.Replace("<!--colspan-->", $"<ColSpan>{ RouteColumns.Count }</ColSpan>");

			//Расширяем таблицу
			string columnsXml = "<TableColumn><Width>21pt</Width></TableColumn>";
			string columns = string.Empty;

			for(int i = 0; i < RouteColumns.Count; i++)
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
				CellColumnHeader += GetColumnHeader(TextBoxNumber++, column.Name);

				if(isClosed)
				{
					CellColumnValue += GetCellTag(
						TextBoxNumber++,
						$"=Iif(" +
							$"{{{_waterTagNamePrefix}{column.Id}}} + {{{_waterFactTagPrefix}{column.Id}}} = 0," +
							$" \"\"," +
							$" Iif(" +
								$"{{{_waterTagNamePrefix}{column.Id}}} = {{{_waterFactTagPrefix}{column.Id}}}," +
								$" Format({{{_waterTagNamePrefix}{column.Id}}}, '0')," +
								$" '' + {{{_waterFactTagPrefix}{column.Id}}} +" +
									$" '(' + Iif(" +
										$"{{{_waterFactTagPrefix}{column.Id}}} - {{{_waterTagNamePrefix}{column.Id}}} > 0," +
										$" '+'," +
										$" '') + ({{{_waterFactTagPrefix}{column.Id}}} - {{{_waterTagNamePrefix}{column.Id}}}) + ')'))",
						$"=Iif({{{_waterTagNamePrefix}{column.Id}}} = {{{_waterFactTagPrefix}{column.Id}}}, \"0\", \"\")",
						isClosed
					);
				}
				else
				{
					var columnShortName = string.IsNullOrEmpty(column.ShortName) ? "" : $"\n{column.ShortName}";

					CellColumnValue += GetCellTag(
						TextBoxNumber++,
						$"=Iif({{{_waterTagNamePrefix}{column.Id}}} = 0, \"\", {{{_waterTagNamePrefix}{column.Id}}} + '{columnShortName}')",
						"C",
						isClosed,
						true,
						column.IsHighlighted,
						column.IsHighlighted ? $"=Iif({{{_waterTagNamePrefix}{column.Id}}} = 0, 'White', 'Lightgrey')" : "",
						"0pt");
				}

				//Ячейка с запасом. Пока там пусто
				CellColumnStock += GetCellTag(TextBoxNumber++, "", "0", isClosed);

				//Ячейка с суммой по бутылям.

				string formula;
				if(isClosed)
				{
					formula = $"=Iif(Sum({{{_waterFactTagPrefix}{column.Id}}}) = 0, \"\", Sum({{{_waterFactTagPrefix}{column.Id}}}))";
				}
				else
				{
					formula = $"=Iif(Sum({{{_waterTagNamePrefix}{column.Id}}}) = 0, \"\", Sum({{{_waterTagNamePrefix}{column.Id}}}))";
				}

				CellColumnTotal += GetCellTag(TextBoxNumber++, formula, "0", isClosed);


				//Запрос..
				if(isFirstColumn)
				{
					SqlSelect += $", CONCAT_WS(' ', (CASE WHEN orders.call_before_arrival_minutes IS NOT NULL THEN CONCAT('Отзвон за: ',orders.call_before_arrival_minutes, ' минут.') ELSE  '' END), REPLACE(orders.comment,'\n',' '), CONCAT('Сдача с: ', orders.trifle, ' руб.')) AS { _orderCommentTagName }" +
						$", (SELECT EXISTS (" +
						$" SELECT * FROM guilty_in_undelivered_orders giuo" +
						$" INNER JOIN undelivered_orders uo ON giuo.undelivery_id = uo.id" +
						$" WHERE giuo.guilty_side IN('{ GuiltyTypes.Driver }','{ GuiltyTypes.Department }')" +
						"  AND uo.new_order_id = orders.id" +
						$")) AS { _orderPrioritizedTagName }";

					Fields +=
						$"<Field Name=\"{_orderPrioritizedTagName}\">" +
						$"<DataField>{_orderPrioritizedTagName}</DataField>" +
						"<TypeName>System.Boolean</TypeName>" +
						"</Field>";

					Fields +=
						$"<Field Name=\"{_orderCommentTagName}\">" +
						$"<DataField>{_orderCommentTagName}</DataField>" +
						"<TypeName>System.String</TypeName>" +
						"</Field>";

					isFirstColumn = false;
				}

				SqlSelect += $", IFNULL(wt_qry.{_waterTagNamePrefix}{column.Id}, 0) AS {_waterTagNamePrefix}{column.Id}";
				SqlSelectSubquery += $", SUM(IF(nomenclature_route_column.id = {column.Id}," +
					$" cast(order_items.count as DECIMAL), 0)) AS {_waterTagNamePrefix}{column.Id}";

				if(isClosed)
				{
					SqlSelect +=
						$", IF(route_list_addresses.status = '{RouteListItemStatus.Transfered}', 0," +
						$" cast(IFNULL(wt_qry.{_waterFactTagPrefix}{column.Id}, 0) as DECIMAL)) AS {_waterFactTagPrefix}{column.Id}";
					SqlSelectSubquery += $", SUM(IF(nomenclature_route_column.id = {column.Id}, cast(IFNULL(order_items.actual_count, 0) as DECIMAL)," +
						$" 0)) AS {_waterFactTagPrefix}{column.Id}";
				}

				//Линкуем запрос на переменные RDL
				Fields +=
					$"<Field Name=\"{_waterTagNamePrefix}{column.Id}\">" +
					$"<DataField>{_waterTagNamePrefix}{column.Id}</DataField>" +
					$"<TypeName>System.Decimal</TypeName>" +
					$"</Field>";
				if(isClosed)
				{
					Fields +=
						$"<Field Name=\"{_waterFactTagPrefix}{column.Id}\">" +
						$"<DataField>{_waterFactTagPrefix}{column.Id}</DataField>" +
						$"<TypeName>System.Decimal</TypeName>" +
						$"</Field>";
				}

				//Формула итоговой суммы по всем бутылям.
				if(_routeColumnRepository.NomenclaturesForColumn(uow, column).Any(x => x.Category == NomenclatureCategory.water && x.TareVolume == TareVolume.Vol19L))
				{
					if(isClosed)
					{
						TotalSum += $"+ Sum(Iif(Fields!Status.Value = \"{ RouteListItemStatus.Completed }\", {{{ _waterFactTagPrefix }{ column.Id }}}, 0))";
					}
					else
					{
						TotalSum += $"+ Sum({{{ _waterTagNamePrefix }{ column.Id }}})";
					}
				}
			}

			RdlText = RdlText.Replace("<!--table_cell_name-->", CellColumnHeader);
			RdlText = RdlText.Replace("<!--table_cell_value-->", CellColumnValue);
			RdlText = RdlText.Replace("<!--comment_colspan-->", $"<ColSpan>{RouteColumns.Count + commentColSpanCount}</ColSpan>");
			RdlText = RdlText.Replace("<!--table_cell_stock-->", CellColumnStock);
			RdlText = RdlText.Replace("<!--table_cell_total-->", CellColumnTotal);
			RdlText = RdlText.Replace("<!--sql_select-->", SqlSelect);
			RdlText = RdlText.Replace("<!--sql_select_subquery-->", SqlSelectSubquery);
			RdlText = RdlText.Replace("<!--fields-->", Fields);
			RdlText = RdlText.Replace("<!--table_cell_total_without_stock-->", TotalSum);

			if(!isClosed)
			{
				var qrPlacer = new EventsQrPlacer(
					new CustomReportFactory(new CustomPropertiesFactory(), new CustomReportItemFactory(), new RdlTextBoxFactory()),
					ScopeProvider.Scope.Resolve<IDriverWarehouseEventRepository>(), _reportInfoFactory);

				qrPlacer.AddQrEventForDocument(uow, routeList.Id, ref RdlText);
			}

			var TempFile = Path.GetTempFileName();
			using(StreamWriter sw = new StreamWriter(TempFile))
			{
				sw.Write(RdlText);
			}
#if DEBUG
			Console.WriteLine(RdlText);
#endif

			string printDatestr = $"Дата печати: { DateTime.Now:g}";
			var needTerminal = routeList.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Маршрутный лист № { routeList.Id }";
			reportInfo.Path = TempFile;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "RouteListId", routeList.Id },
				{ "Print_date", printDatestr},
				{ "RouteListDate", routeList.Date},
				{ "need_terminal", needTerminal },
				{ "phones", _generalSettingsSettings.GetRouteListPrintedFormPhones}
			};
			return reportInfo;
		}

		private static string GetColumnHeader(int id, string name)
		{
			return "<TableCell><ReportItems>" +
				   $"<Textbox Name=\"Textbox{ id }\">" +
				   $"<Value>{ name }</Value>" +
				   "<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
				   "<BorderStyle><Default>Solid</Default><Top>Solid</Top><Bottom>Solid</Bottom></BorderStyle>" +
				   "<BorderColor /><BorderWidth /><FontSize>8pt</FontSize><TextAlign>Center</TextAlign></Style>" +
				   "<CanGrow>false</CanGrow></Textbox></ReportItems></TableCell>";
		}

		private static string GetCellTag(int id, string value, string formatString, bool isClosed, bool canGrow = false, bool isBoldText = false, string cellBackgroundString = "", string paddingValue = "10pt")
		{
			var canGrowText = canGrow ? "true" : "false";
			return $"<TableCell><ReportItems>" +
				   $"<Textbox Name=\"Textbox{ id }\">" +
				   $"<Value>{ value }</Value>" +
				   $"<Style xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition\">" +
				   $"<BorderStyle><Default>Solid</Default></BorderStyle><BorderColor /><BorderWidth /><FontSize>8pt</FontSize>" +
				   $"<TextAlign>Center</TextAlign><Format>{ formatString }</Format><VerticalAlign>Middle</VerticalAlign>" +
				   (isClosed
				   ? $"<BackgroundColor>=Iif((Fields!Status.Value = \"{ RouteListItemStatus.EnRoute }\") or (Fields!Status.Value = \"{ RouteListItemStatus.Completed }\"), White, Lightgrey)</BackgroundColor>"
				   : !string.IsNullOrEmpty(cellBackgroundString)
						? $"<BackgroundColor>{cellBackgroundString}</BackgroundColor>"
						: "") +
				   (isBoldText ? $"<FontWeight >Bold</FontWeight>" : "") +
				   $"<PaddingTop>{paddingValue}</PaddingTop><PaddingBottom>{paddingValue}</PaddingBottom></Style>" +
				   $"<CanGrow>{canGrowText}</CanGrow></Textbox></ReportItems></TableCell>";
		}

		public static ReportInfo GetRDLTimeList(int routeListId)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Лист времени для МЛ № { routeListId }";
			reportInfo.Identifier = "Documents.TimeList";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "route_list_id", routeListId }
			};
			return reportInfo;
		}

		public static ReportInfo GetRDLDailyList(int routeListId)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Ежедневные номера МЛ № { routeListId }";
			reportInfo.Identifier = "Logistic.AddressesByDailyNumber";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "route_list", routeListId }
			};
			return reportInfo;
		}

		public static ReportInfo GetRDLRouteMap(IUnitOfWork uow, RouteList routeList, ICachedDistanceRepository cachedDistanceRepository, bool batchPrint)
		{
			string documentName = "RouteMap";

			XmlDocument rdlText = new XmlDocument();
			XmlNamespaceManager namespaces = new XmlNamespaceManager(rdlText.NameTable);
			namespaces.AddNamespace("r", "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition");
			rdlText.Load(Path.Combine(Environment.CurrentDirectory, "Reports/Logistic/" + documentName + ".rdl"));
			var imageData = rdlText.DocumentElement.SelectSingleNode("/r:Report/r:EmbeddedImages/r:EmbeddedImage[@Name=\"map\"]/r:ImageData", namespaces);

			var map = new GMapControl
			{
				MapProvider = GMapProviders.GoogleMap,
				MaxZoom = 18,
				RoutesEnabled = true,
				MarkersEnabled = true
			};

			GMapOverlay routeOverlay = new GMapOverlay("route");
			var uowFactory = ScopeProvider.Scope.Resolve<IUnitOfWorkFactory>();
			var osrmSettings = ScopeProvider.Scope.Resolve<IOsrmSettings>();
			var osrmClient = ScopeProvider.Scope.Resolve<IOsrmClient>();
			using(var calc = new RouteGeometryCalculator(uowFactory, osrmSettings, osrmClient, cachedDistanceRepository))
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

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Карта маршрута № { routeList.Id }";
			reportInfo.Source = rdlText.InnerXml;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "route_id", routeList.Id }
			};
			return reportInfo;
		}

		[Obsolete("Удалить метод вместе с rdl, если не будет запросов на использование после 10.10.2022")]
		public static ReportInfo GetRDLLoadDocument(int routeListId)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Документ погрузки для МЛ № { routeListId }";
			reportInfo.Identifier = "RouteList.CarLoadDocument";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "route_list_id", routeListId }
			};
			return reportInfo;
		}

		public static ReportInfo GetRDLFine(RouteList routeList, IUnitOfWork uow)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Штрафы сотрудника { routeList.Driver.LastName }";
			reportInfo.Identifier = "Employees.Fines";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "drivers", routeList.Driver.Id },
				{ "startDate", routeList.Date },
				{ "endDate", routeList.Date },
				{ "routelist", routeList.Id },
				{ "showbottom", true},
				{ "fineCategories", uow.GetAll<FineCategory>().Where(x => !x.IsArchive).Select(x => x.Id).ToList() }
			};
			return reportInfo;
		}

		public static ReportInfo GetRDLForwarderReceipt(RouteList routeList)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Экспедиторская расписка для МЛ №{routeList.Id}";
			reportInfo.Identifier = "Documents.ForwarderReceipt";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "routeListId", routeList.Id },
				{ "routeListDate", routeList.Date },
				{ "driverId", routeList.Driver.Id }
			};
			return reportInfo;
		}

		public static ReportInfo GetRDL(RouteList routeList, RouteListPrintableDocuments type, IUnitOfWork uow = null, bool batchPrint = false)
		{
			switch(type)
			{
				case RouteListPrintableDocuments.RouteList:
					return GetRDLRouteList(uow, routeList);
				case RouteListPrintableDocuments.RouteMap:
					return GetRDLRouteMap(uow, routeList, _cachedDistanceRepository, batchPrint);
				case RouteListPrintableDocuments.TimeList:
					return GetRDLTimeList(routeList.Id);
				case RouteListPrintableDocuments.DailyList:
					return GetRDLDailyList(routeList.Id);
				case RouteListPrintableDocuments.OrderOfAddresses:
					return routeList.OrderOfAddressesRep(routeList.Id);
				case RouteListPrintableDocuments.ForwarderReceipt:
					return GetRDLForwarderReceipt(routeList);
				case RouteListPrintableDocuments.ChainStoreNotification:
					return GetRDLChainStoreNotification(routeList);
				default:
					throw new NotImplementedException("Неизвестный тип документа");
			}
		}

		private static ReportInfo GetRDLChainStoreNotification(RouteList routeList)
		{
			var reportInfo = _reportInfoFactory.Create();

			var chainStoreOrderIds = routeList.Addresses
				.Where(address => address.RouteList.Id == routeList.Id
				                  && address.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale
				                  && address.Order.Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute)
				.Select(address => address.Order.Id);

			var orderIds = chainStoreOrderIds.Any() ? string.Join(", ", chainStoreOrderIds) : "Отсутствуют сетевые заказы";

			reportInfo.Title = $"Уведомление о наличии сетевого заказа  № {orderIds}";
			reportInfo.Identifier = "Documents.ChainStoreNotification";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				
				{ "orderIds", orderIds }
			};
			
			return reportInfo;
		}
	}
}
