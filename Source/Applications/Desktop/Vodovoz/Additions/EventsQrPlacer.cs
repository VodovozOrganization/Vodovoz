using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.NHibernate.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Presentation.Reports.Factories;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Additions
{
	public class EventsQrPlacer : IEventsQrPlacer
	{
		private const string _leftItemWithQr = "LeftQrRectangle";
		private const string _rightItemWithQr = "RightQrRectangle";
		private const string _topItemWithQr = "TopQrRectangle";
		private const string _bottomItemWithQr = "BottomQrRectangle";
		private readonly ICustomReportFactory _customReportFactory;
		private readonly IDriverWarehouseEventRepository _driverWarehouseEventRepository;

		public EventsQrPlacer(
			ICustomReportFactory customReportFactory,
			IDriverWarehouseEventRepository driverWarehouseEventRepository)
		{
			_customReportFactory = customReportFactory ?? throw new ArgumentNullException(nameof(customReportFactory));
			_driverWarehouseEventRepository =
				driverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(driverWarehouseEventRepository));
		}
		
		public bool AddQrEventForDocument(
			IUnitOfWork uow,
			int documentId,
			EventQrDocumentType eventQrDocumentType,
			ref string rdlPath,
			EventNamePosition eventNamePosition = EventNamePosition.Bottom)
		{
			var events =
				_driverWarehouseEventRepository.GetActiveDriverWarehouseEventsForDocument(uow, eventQrDocumentType);

			if(!events.Any())
			{
				rdlPath = Path.GetFullPath(rdlPath);
				return false;
			}
			
			var serializer = new XmlSerializer(typeof(Report));
			Report report;

			using(var reader = new StreamReader(rdlPath))
			{
				report = (Report)serializer.Deserialize(reader);
			}

			switch(eventQrDocumentType)
			{
				case EventQrDocumentType.CarLoadDocument:
					var carLoadSuccess = AddQrsToCarLoadDocument(events, report, documentId, eventNamePosition);
					if(!carLoadSuccess)
					{
						rdlPath = Path.GetFullPath(rdlPath);
						return false;
					}
					break;
				case EventQrDocumentType.CarUnloadDocument:
					var carUnloadSuccess = AddQrsToBeginAndEndDocument(events, report, documentId, eventNamePosition);
					if(!carUnloadSuccess)
					{
						rdlPath = Path.GetFullPath(rdlPath);
						return false;
					}
					break;
			}
			
			rdlPath = Path.GetTempFileName();
			using(var sw = new StreamWriter(rdlPath))
			{
				serializer.Serialize(sw, report);
			}

			return true;
		}
		
		public bool AddQrEventForDocument(
			IUnitOfWork uow,
			int documentId,
			ref string rdlReport,
			EventQrDocumentType eventQrDocumentType = EventQrDocumentType.RouteList,
			EventNamePosition eventNamePosition = EventNamePosition.Right)
		{
			if(eventQrDocumentType != EventQrDocumentType.RouteList)
			{
				return false;
			}
			
			var events =
				_driverWarehouseEventRepository.GetActiveDriverWarehouseEventsForDocument(uow, eventQrDocumentType);

			if(!events.Any())
			{
				return false;
			}
			
			var serializer = new XmlSerializer(typeof(Report));
			Report report;

			using(var reader = new StringReader(rdlReport))
			{
				report = (Report)serializer.Deserialize(reader);
			}

			var result = AddQrsToBeginAndEndDocument(events, report, documentId, eventNamePosition);
			if(!result)
			{
				return false;
			}
			
			using(var writer = new StringWriter())
			{
				serializer.Serialize(writer, report);
				rdlReport = writer.ToString();
			}

			return true;
		}

		private bool AddQrsToCarLoadDocument(
			IEnumerable<DriverWarehouseEvent> events,
			Report report,
			int documentId,
			EventNamePosition eventNamePosition = EventNamePosition.Bottom)
		{
			var reportItems = report.Body.GetEnamedItemsValue<ReportItems>(nameof(ReportItems));
			var rectangles = reportItems.Items.OfType<Rectangle>();
			var leftRectangle = rectangles.SingleOrDefault(x => x.Name == _leftItemWithQr);
			var rightRectangle = rectangles.SingleOrDefault(x => x.Name == _rightItemWithQr);

			if(leftRectangle is null && rightRectangle is null)
			{
				return false;
			}
			
			var leftLeftQr = 0m;
			var topLeftQr = 0m;
			
			var leftRightQr = 0m;
			var topRightQr = 0m;

			foreach(var @event in events.OrderBy(x => x.QrPositionOnDocument))
			{
				switch(@event.QrPositionOnDocument)
				{
					case EventQrPositionOnDocument.Left:
						AddQrToElement(@event, leftRectangle, documentId, topLeftQr, ref leftLeftQr, eventNamePosition);
						break;
					case EventQrPositionOnDocument.Right:
						AddQrToElement(@event, rightRectangle, documentId, topRightQr, ref leftRightQr, eventNamePosition);
						break;
				}
			}

			return true;
		}
		
		private bool AddQrsToBeginAndEndDocument(
			IEnumerable<DriverWarehouseEvent> events,
			Report report,
			int documentId,
			EventNamePosition eventNamePosition = EventNamePosition.Bottom)
		{
			var reportItems = report.Body.GetEnamedItemsValue<ReportItems>(nameof(ReportItems));
			var rectangles = reportItems.Items.OfType<Rectangle>();
			var topRectangle = rectangles.SingleOrDefault(x => x.Name == _topItemWithQr);
			var bottomRectangle = rectangles.SingleOrDefault(x => x.Name == _bottomItemWithQr);

			if(topRectangle is null && bottomRectangle is null)
			{
				return false;
			}
			
			var leftTopQr = 0m;
			var topTopQr = 0m;
			
			var leftBottomQr = 0m;
			var topBottomQr = 0m;

			foreach(var @event in events.OrderBy(x => x.QrPositionOnDocument))
			{
				switch(@event.QrPositionOnDocument)
				{
					case EventQrPositionOnDocument.Top:
						AddQrToElement(@event, topRectangle, documentId, topTopQr, ref leftTopQr, eventNamePosition);
						break;
					case EventQrPositionOnDocument.Bottom:
						AddQrToElement(@event, bottomRectangle, documentId, topBottomQr, ref leftBottomQr, eventNamePosition);
						break;
				}
			}

			return true;
		}

		private void AddQrToElement(
			DriverWarehouseEvent @event,
			Rectangle rectangle,
			int documentId,
			decimal top,
			ref decimal left,
			EventNamePosition eventNamePosition)
		{
			if(rectangle is null)
			{
				return;
			}
			
			if(rectangle.ReportItems is null)
			{
				rectangle.ReportItems = new ReportItems();
			}

			PlaceQrWithEventName(@event, rectangle, documentId, top, ref left, eventNamePosition);
		}

		private void PlaceQrWithEventName(
			DriverWarehouseEvent @event,
			Rectangle rectangle,
			int documentId,
			decimal top,
			ref decimal left,
			EventNamePosition eventNamePosition)
		{
			var padding = 5m;
			var leftReportItem = left + "pt";
			var topReportItem = top + "pt";
			var qrString = @event.GenerateQrData(documentId);
			var qrReportItem = _customReportFactory.CreateDefaultQrReportItem(leftReportItem, topReportItem, qrString);

			if(eventNamePosition == EventNamePosition.Bottom)
			{
				top += decimal.Floor(qrReportItem.HeightSize);
				left += padding;
			}
			else
			{
				left += qrReportItem.WidthSize;
				top += 2 * padding;
			}
			
			var leftEventNameBox = left + "pt";
			var topEventNameBox = top + "pt";
			var eventNameBox = _customReportFactory.CreateTextBox(@event.EventName, leftEventNameBox, topEventNameBox);

			rectangle.ReportItems.ItemsList.Add(qrReportItem);
			rectangle.ReportItems.ItemsList.Add(eventNameBox);

			left += qrReportItem.WidthSize + padding;
		}
	}
}
