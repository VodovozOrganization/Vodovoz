using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Services;
using QS.Tdi;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Printing;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class PrintRouteDocumentsDlg : QS.Dialog.Gtk.TdiTabBase, ITDICloseControlTab
	{
		private readonly IRouteListRepository _routeListRepository = ScopeProvider.Scope.Resolve<IRouteListRepository>();

		private Gdk.Pixbuf _vodovozCarIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory = new EntityDocumentsPrinterFactory();
		private List<SelectablePrintDocument> _routes = new List<SelectablePrintDocument>();
		private GenericObservableList<OrderDocTypeNode> _orderDocTypesToPrint = new GenericObservableList<OrderDocTypeNode>();
		private GenericObservableList<GeoGroup> _geographicGroups;
		private GenericObservableList<string> _warnings;

		private IUnitOfWork _uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

		public PrintRouteDocumentsDlg()
		{
			this.Build();

			TabName = "Массовая печать документов МЛ";

			chkDocumentsOfOrders.Toggled += ChkDocumentsOfOrders_Toggled;
			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<SelectablePrintDocument>.Create()
				.AddColumn("Печатать")
					.AddToggleRenderer(x => x.Selected)
					.AddSetter((c, n) => c.Visible = (n.Document as RouteListPrintableDocs).routeList.Status >= RouteListStatus.Confirmed)
				.AddColumn("Номер")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Id.ToString())
				.AddColumn("Статус")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Status.GetEnumTitle())
				.AddColumn("Водитель")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Driver.ShortName)
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x =>
						(x.Document as RouteListPrintableDocs).routeList.Car != null
						&& (x.Document as RouteListPrintableDocs).routeList.GetCarVersion.IsCompanyCar
							? _vodovozCarIcon
							: null)
					.AddTextRenderer(x =>
						(x.Document as RouteListPrintableDocs).routeList.Car != null
							? (x.Document as RouteListPrintableDocs).routeList.Car.RegistrationNumber
							: "нет")
				.AddColumn("Часть города")
					.AddTextRenderer(x => string.Join(", ", (x.Document as RouteListPrintableDocs).routeList.GeographicGroups.Select(g => g.Name)))
				.AddColumn("Время печати")
					.AddTextRenderer(x =>
					(x.Document as RouteListPrintableDocs).routeList.PrintsHistory != null &&
					(x.Document as RouteListPrintableDocs).routeList.PrintsHistory.Any() ?
						(x.Document as RouteListPrintableDocs).routeList.PrintsHistory.LastOrDefault().PrintingTime.ToString()
						: "МЛ не распечатан")
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
					c.ForegroundGdk = (n.Document as RouteListPrintableDocs).routeList.PrintsHistory?.Any() ?? false ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();

			geograficGroup.UoW = _uow;
			geograficGroup.Label = "Часть города:";
			_geographicGroups = new GenericObservableList<GeoGroup>();
			geograficGroup.Items = _geographicGroups;
			geograficGroup.ListContentChanged += OnYdatePrintDateChanged;
			
			foreach(var gg in _uow.Session.QueryOver<GeoGroup>().List())
			{
				_geographicGroups.Add(gg);
			}

			ydatePrint.Date = DateTime.Today;

			OrderDocumentType[] selectedByDefault = {
				OrderDocumentType.Contract,
				OrderDocumentType.InvoiceContractDoc,
				OrderDocumentType.Bill,
				OrderDocumentType.UPD,
				OrderDocumentType.SpecialBill,
				OrderDocumentType.SpecialUPD,
				OrderDocumentType.M2Proxy,
				OrderDocumentType.EquipmentTransfer,
				OrderDocumentType.DoneWorkReport,
				OrderDocumentType.EquipmentReturn,
				OrderDocumentType.Torg12,
				OrderDocumentType.ShetFactura,
				OrderDocumentType.ProductCertificate
			};

			foreach(OrderDocumentType t in Enum.GetValues(typeof(OrderDocumentType)))
			{
				_orderDocTypesToPrint.Add(new OrderDocTypeNode(t, selectedByDefault.Contains(t)));
			}

			yTreeOrderDocumentTypes.ColumnsConfig = FluentColumnsConfig<OrderDocTypeNode>.Create()
				.AddColumn("Печатать")
					.AddToggleRenderer(x => x.Selected)
					.Editing()
					.ChangeSetProperty(PropertyUtil.GetPropertyInfo<OrderDocTypeNode>(x => x.Selected))
				.AddColumn("Название").AddTextRenderer(x => x.Type.GetEnumTitle())
				.AddColumn("")
				.Finish();
			yTreeOrderDocumentTypes.SetItemsSource(_orderDocTypesToPrint);
			yTreeOrderDocumentTypes.HeadersVisible = false;
			ChkDocumentsOfOrders_Toggled(this, null);

			_warnings = new GenericObservableList<string>();
			yTreeViewWarnings.HeadersVisible = false;
			yTreeViewWarnings.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("")
					.AddTextRenderer(x => x)
				.RowCells()
					.AddSetter<CellRendererText>((c,n) => c.ForegroundGdk = GdkColors.DangerText)
				.Finish();
			yTreeViewWarnings.SetItemsSource(_warnings);

			ycheckOnlyNonPrinted.Clicked += YcheckOnlyNonPrinted_Clicked;
		}

		private void YcheckOnlyNonPrinted_Clicked(object sender, EventArgs e)
		{
			UpdateRouteList();
		}

		class OrderDocTypeNode : PropertyChangedBase
		{
			bool selected;

			public OrderDocTypeNode(OrderDocumentType type, bool selected)
			{
				Type = type;
				Selected = selected;
			}

			public virtual bool Selected {
				get => selected;
				set => SetField(ref selected, value, () => Selected);
			}
			public OrderDocumentType Type { get; set; }
		}

		void ChkDocumentsOfOrders_Toggled(object sender, EventArgs e)
		{
			gtkScrlWnd.Visible = chkDocumentsOfOrders.Active;
		}

		protected void OnYdatePrintDateChanged(object sender, EventArgs e) => UpdateRouteList();

		private void UpdateRouteList()
		{
			checkSelectAll.Active = false;
			gtkScrollWndWarnings.Visible = false;
			var ggIds = _geographicGroups.Select(x => x.Id).ToList();
			var routeQuery = _routeListRepository.GetRoutesAtDay(ydatePrint.Date, ggIds, ycheckOnlyNonPrinted.Active).GetExecutableQueryOver(_uow.Session);
			_routes = routeQuery.Fetch(SelectMode.Fetch, x => x.Driver)
							   .Fetch(SelectMode.Fetch, x => x.Car)
							   .List()
							   .Select(x => new SelectablePrintDocument(new RouteListPrintableDocs(_uow, x, RouteListPrintableDocuments.RouteList)))
							   .OrderBy(x => (x.Document as RouteListPrintableDocs).routeList.Driver.LastName)
							   .ToList()
							   .Distinct()
							   .ToList()
							   ;
			ytreeRoutes.SetItemsSource(new GenericObservableList<SelectablePrintDocument>(_routes));
			var notPrintedRoutes = _routes.Where(x => (x.Document as RouteListPrintableDocs).routeList.Status < RouteListStatus.Confirmed).ToList();

			if(notPrintedRoutes.Any())
			{
				MessageDialogHelper.RunWarningDialog(
					String.Format(
						"Маршрутные листы {0} не могут быть напечатаны, так как еще не подтверждены.",
						String.Join(", ", notPrintedRoutes.Select(x => $"{(x.Document as RouteListPrintableDocs).routeList.Id}({(x.Document as RouteListPrintableDocs).routeList.Driver.ShortName})"))
					)
				);
			}
		}

		protected void OnCheckSelectAllToggled(object sender, EventArgs e)
		{
			_routes.Where(x => (x.Document as RouteListPrintableDocs).routeList.Status >= RouteListStatus.Confirmed).ToList().ForEach(x => x.Selected = checkSelectAll.Active);
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения печати и повторите");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonPrint.Sensitive = isSensetive;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			try
			{
				SetSensetivity(false);
				var routeCount = _routes.Count(x => x.Selected);
				progressPrint.Adjustment.Upper = routeCount;
				progressPrint.Adjustment.Value = 0;

				var selectedRoutesIds = _routes
					.Where(x => x.Selected)
					.Select(x => (x.Document as RouteListPrintableDocs).routeList.Id)
					.ToList();

				var selectedRoutesWithFastDelivery = _uow.GetAll<RouteList>()
					.Where(r => selectedRoutesIds.Contains(r.Id) && r.AdditionalLoadingDocument != null)
					.FetchMany(x => x.Addresses)
					.ThenFetch(x => x.Order)
					.ThenFetch(x => x.Contract)
					.ThenFetch(x => x.Organization)
					.Select(r => r.Id)
					.ToList();

				var selectedRouteListWithChainStore = _routes
					.Where(x => x.Selected)
					.Select(x => (x.Document as RouteListPrintableDocs).routeList)
					.SelectMany(x => x.Addresses)
					.Where(address => selectedRoutesIds.Contains(address.RouteList.Id)
					                  && address.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale
					                  && address.Order.Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute)
					.Select(address => address.RouteList.Id)
					.ToArray();

				foreach(var item in _routes.Where(x => x.Selected))
				{
					if(item.Document is RouteListPrintableDocs rlPrintableDoc)
					{
						progressPrint.Text = $"Печатаем МЛ {rlPrintableDoc.routeList.Id} - {rlPrintableDoc.routeList.Driver.ShortName}";
						QSMain.WaitRedraw();
						var rlDocTypesToPrint = new List<RouteListPrintableDocuments>();
						OrderDocumentType[] oDocTypesToPrint = null;

						if(checkRoute.Active)
						{
							rlDocTypesToPrint.Add(RouteListPrintableDocuments.RouteList);
						}

						if(checkRouteMap.Active)
						{
							rlDocTypesToPrint.Add(RouteListPrintableDocuments.RouteMap);
						}

						if(chkDocumentsOfOrders.Active)
						{
							oDocTypesToPrint = _orderDocTypesToPrint.Where(n => n.Selected)
								.Select(n => n.Type)
								.ToArray();
						}

						if(chkForwarderReceipt.Active && selectedRoutesWithFastDelivery.Contains(rlPrintableDoc.routeList.Id))
						{
							rlDocTypesToPrint.Add(RouteListPrintableDocuments.ForwarderReceipt);
						}
						
						if(chkChainStoreNotification.Active && selectedRouteListWithChainStore.Contains(rlPrintableDoc.routeList.Id))
						{
							rlDocTypesToPrint.Add(RouteListPrintableDocuments.ChainStoreNotification);
						}

						bool cancelPrinting = false;
						var printer = _entityDocumentsPrinterFactory.CreateRouteListWithOrderDocumentsPrinter(
							_uow,
							rlPrintableDoc.routeList,
							rlDocTypesToPrint.ToArray(),
							oDocTypesToPrint
						);
						
						printer.DocumentsPrinted += (o, args) =>
						{
							if(!rlDocTypesToPrint.Contains(RouteListPrintableDocuments.RouteList | RouteListPrintableDocuments.All))
							{
								return;
							}

							rlPrintableDoc.routeList.AddPrintHistory();
							_uow.Save(rlPrintableDoc.routeList);
							_uow.Commit();
						};
						
						printer.PrintingCanceled += (s, ea) =>
						{
							cancelPrinting = true;
						};
						
						printer.Print();
						
						if(!string.IsNullOrEmpty(printer.ODTTemplateNotFoundMessages))
						{
							gtkScrollWndWarnings.Visible = true;
							_warnings.Add(string.Format("МЛ №{0} - {1}:", rlPrintableDoc.routeList.Id, rlPrintableDoc.routeList.Driver.ShortName));
							_warnings.Add(printer.ODTTemplateNotFoundMessages);
						}
						
						if(cancelPrinting)
						{
							progressPrint.Text = "Печать отменена";
							break;
						}
					}
					progressPrint.Text = "Готово";

					progressPrint.Adjustment.Value++;
				}
				
				EntityDocumentsPrinter.PrinterSettings = null;
			}
			finally
			{
				SetSensetivity(true);
			}
		}

		public override void Destroy()
		{
			_uow?.Dispose();
			base.Destroy();
		}
	}
}
