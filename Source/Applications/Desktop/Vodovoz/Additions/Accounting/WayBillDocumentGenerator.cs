using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using Gtk;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;
using VodovozInfrastructure.Utils;
using VodovozInfrastructure.Utils.NHibernate;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.DocTemplates;
using QS.Print;
using QSDocTemplates;
using QS.DocTemplates;
using System.IO;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Core.Domain.Employees;
using QS.Extensions.Observable.Collections.List;
using FileWorker = QSDocTemplates.FileWorker;

namespace Vodovoz.Additions.Accounting
{
	public class WayBillDocumentGenerator : PropertyChangedBase
	{
		private readonly IWayBillDocumentRepository _wayBillDocumentRepository;
		private readonly RouteGeometryCalculator _distanceCalculator;
		private readonly IDocTemplateRepository _docTemplateRepository;
		private readonly IUnitOfWork _uow;

		private string _mechanicFIO;
		private string _mechanicLastName;
		private DateTime _startDate;
		private DateTime _endDate;
		private bool _cancelPrinting;

		private readonly TimeSpan[,] _timeSpans =
		{
			{
				TimeSpan.FromHours(9),
				TimeSpan.FromHours(18)
			},
			{
				TimeSpan.FromHours(11),
				TimeSpan.FromHours(21)
			},
			{
				TimeSpan.FromHours(14),
				TimeSpan.FromHours(23)
			}
		};

		public WayBillDocumentGenerator(
			IUnitOfWork unitOfWork,
			IWayBillDocumentRepository repository,
			RouteGeometryCalculator calculator,
			IDocTemplateRepository docTemplateRepository)
		{
			_uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_wayBillDocumentRepository = repository ?? throw new ArgumentNullException(nameof(repository));
			_distanceCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
			_docTemplateRepository = docTemplateRepository ?? throw new ArgumentNullException(nameof(docTemplateRepository));
			DocPrinterInit();
		}

		public string MechanicFIO
		{
			get => _mechanicFIO;
			set => SetField(ref _mechanicFIO, value);
		}

		public string MechanicLastName
		{
			get => _mechanicLastName;
			set => SetField(ref _mechanicLastName, value);
		}

		public event EventHandler PrintingCanceled;

		public GenericObservableList<SelectablePrintDocument> WayBillSelectableDocuments { get; set; }
			= new GenericObservableList<SelectablePrintDocument>();

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public static PrintSettings PrinterSettings { get; set; }
		public string ODTTemplateNotFoundMessages { get; set; }
		public MultipleDocumentPrinter MultiDocPrinter { get; set; }

		#region Printing

		private void DocPrinterInit()
		{
			MultiDocPrinter = new MultipleDocumentPrinter
			{
				PrintableDocuments = new ObservableList<SelectablePrintDocument>(WayBillSelectableDocuments)
			};
			MultiDocPrinter.PrintingCanceled += (o, args) => PrintingCanceled?.Invoke(o, args);
		}

		public void PrintSelected(SelectablePrintDocument document = null)
		{
			if(Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
			{
				var settingsOperaation = new PrintOperation();
				settingsOperaation.Run(PrintOperationAction.PrintDialog, null);
				PrinterSettings = settingsOperaation.PrintSettings;
			}

			if(!_cancelPrinting)
			{
				MultiDocPrinter.PrinterSettings = PrinterSettings;
				if(document == null)
				{
					MultiDocPrinter.PrintSelectedDocuments();
				}
				else
				{
					MultiDocPrinter.PrintDocument(document);
				}
				PrinterSettings = MultiDocPrinter.PrinterSettings;
			}
			else
			{
				PrintingCanceled?.Invoke(this, EventArgs.Empty);
			}
		}

		public void PrintDocuments()
		{
			if(WayBillSelectableDocuments.Count == WayBillSelectableDocuments.Count(x => x.Document is WayBillDocument))
			{
				QSMain.WaitRedraw();

				PrintingCanceled += (sender, args) => { _cancelPrinting = true; };

				PrintSelected();
				if(!string.IsNullOrEmpty(ODTTemplateNotFoundMessages))
				{
					MessageDialogHelper.RunWarningDialog(ODTTemplateNotFoundMessages);
				}
			}
		}

		#endregion

		#region Generation

		public void GenerateDocuments()
		{
			WayBillSelectableDocuments.Clear();
			var currentDateTime = DateTime.Now;
			EmployeeRegistrationVersion employeeRegistrationVersionAlias = null;
			EmployeeRegistration employeeRegistrationAlias = null;

			var manOfficialWithCarEmployees = _uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.EmployeeRegistrationVersions, () => employeeRegistrationVersionAlias, JoinType.InnerJoin,
					Restrictions.Where(() => employeeRegistrationVersionAlias.StartDate <= currentDateTime
						&& (employeeRegistrationVersionAlias.EndDate == null || employeeRegistrationVersionAlias.EndDate >= currentDateTime)))
				.JoinAlias(() => employeeRegistrationVersionAlias.EmployeeRegistration, () => employeeRegistrationAlias)
				.Where(x => x.Gender == Gender.male)
				.WhereStringIsNotNullOrEmpty(x => x.DrivingLicense)
				.And(() => employeeRegistrationAlias.RegistrationType == RegistrationType.LaborCode)
				.List();

			Car carAlias = null;
			CarVersion carVersionAlias = null;
			CarFuelVersion carFuelVersionAlias = null;
			CarModel carModelAlias = null;
			CarNode resultAlias = null;

			var carNodes = _uow.Session.QueryOver<Car>(() => carAlias)
				.Inner.JoinAlias(c => c.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= currentDateTime &&
						(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= currentDateTime))
				.JoinEntityAlias(() => carFuelVersionAlias,
					() => carFuelVersionAlias.CarModel.Id == carModelAlias.Id
						&& carFuelVersionAlias.StartDate <= currentDateTime &&
						(carFuelVersionAlias.EndDate == null || carFuelVersionAlias.EndDate >= currentDateTime))
				.Where(() => carVersionAlias.CarOwnType == CarOwnType.Company)
				.And(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse),
					new[] { CarTypeOfUse.Largus, CarTypeOfUse.GAZelle, CarTypeOfUse.Minivan }))
				.SelectList(list => list
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.RegistrationNumber)
					.Select(() => carAlias.FuelType).WithAlias(() => resultAlias.FuelType)
					.Select(() => carFuelVersionAlias.FuelConsumption).WithAlias(() => resultAlias.FuelConsumption)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
					.Select(() => carAlias.DocPTSSeries).WithAlias(() => resultAlias.DocPTSSeries)
					.Select(() => carAlias.DocPTSNumber).WithAlias(() => resultAlias.DocPTSNumber))
				.TransformUsing(Transformers.AliasToBean<CarNode>())
				.List<CarNode>();

			//Распределяем автомобили на сотрудников
			var employeeToCars = new Dictionary<Employee, CarNode>();
			Stack<CarNode> carsStack = new Stack<CarNode>(carNodes);

			if(carNodes.Count < manOfficialWithCarEmployees.Count)
			{
				MessageDialogHelper.RunWarningDialog("Количество водителей больше количества автомобилей");
				return;
			}

			foreach(var employee in manOfficialWithCarEmployees)
			{
				employeeToCars[employee] = carsStack.Pop();
			}

			var randomizer = new Random();

			foreach(var day in _startDate.Range(_endDate))
			{
				var currentDayOrders =
					_wayBillDocumentRepository.GetOrdersForWayBillDocuments(_uow, day, day.AddHours(23).AddMinutes(59).AddSeconds(59));

				foreach(var employeeToCarPair in employeeToCars)
				{
					var routesCount = Math.Min(randomizer.Next(12, 15), currentDayOrders.Count);
					var randomTimeInterval = GenerateRandomRouteTime();
					GenerateWayBill(currentDayOrders.Take(routesCount).ToList(), routesCount, day, randomTimeInterval,
						employeeToCarPair.Key, employeeToCarPair.Value);

					currentDayOrders = currentDayOrders.Skip(routesCount).ToList();
				}
			}
		}

		private void GenerateWayBill(IList<Order> orders, int waypointsCount, DateTime generationDate, TimeSpan[] timeInterval,
			Employee employee, CarNode carNode)
		{
			var wayBillDocument = new WayBillDocument();

			if(orders.IsEmpty())
			{
				return;
			}

			for(var i = 0; i < orders.Count && i < waypointsCount; i++)
			{
				var order = orders[i];
				var wayBillDocumentItem = new WayBillDocumentItem
				{
					CounterpartyName = order.Client.Name,
					DriverLastName = employee.LastName,
					HoursFrom = timeInterval[0],
					HoursTo = timeInterval[1],
					AddressTo = order.DeliveryPoint.ShortAddress
				};

				wayBillDocument.WayBillDocumentItems.Add(wayBillDocumentItem);
			}

			wayBillDocument.WayBillDocumentItems.First().AddressFrom = employee.Subdivision.Name;
			wayBillDocument.WayBillDocumentItems.Last().AddressTo = employee.Subdivision.Name;

			string lastAddressTo = "";
			DeliveryPoint deliveryPointFrom = null;

			var lastId = wayBillDocument.WayBillDocumentItems.Count - 2;

			for(int i = 0; i < wayBillDocument.WayBillDocumentItems.Count && i < orders.Count; i++)
			{
				var wayBill = wayBillDocument.WayBillDocumentItems[i];
				var order = orders[i];

				if(i != 0 && i != waypointsCount)
				{
					wayBill.AddressFrom = lastAddressTo;
				}
				lastAddressTo = wayBill.AddressTo;
				wayBill.SequenceNumber = i + 1;

				if(i == 0)
				{
					var geoGroupVersion = GetActualGeoGroupVersion(employee, generationDate);
					wayBill.Mileage = _distanceCalculator.DistanceFromBaseMeter(
						geoGroupVersion.PointCoordinates,
						order.DeliveryPoint.PointCoordinates) * 2 / 1000m;
					
					wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(geoGroupVersion.PointCoordinates));
					deliveryPointFrom = order.DeliveryPoint;
				}
				else if(i == lastId)
				{
					var geoGroupVersion = GetActualGeoGroupVersion(employee, generationDate);
					wayBill.Mileage = _distanceCalculator.DistanceToBaseMeter(
						order.DeliveryPoint.PointCoordinates,
						geoGroupVersion.PointCoordinates) * 2 / 1000m;

					if(order.DeliveryPoint.CoordinatesExist)
					{
						wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(order.DeliveryPoint.PointCoordinates));
					}
					wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(geoGroupVersion.PointCoordinates));
				}
				else
				{
					wayBill.Mileage = _distanceCalculator.DistanceMeter(
						deliveryPointFrom.PointCoordinates,
						order.DeliveryPoint.PointCoordinates) * 2 / 1000m;

					if(order.DeliveryPoint.CoordinatesExist)
					{
						wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(order.DeliveryPoint.PointCoordinates));
					}
					deliveryPointFrom = order.DeliveryPoint;
				}
			}

			if(wayBillDocument.WayBillDocumentItems.IsEmpty())
			{
				return;
			}

			wayBillDocument.Date = generationDate.Date;
			wayBillDocument.CarModelName = carNode.CarModelName;
			wayBillDocument.CarRegistrationNumber = carNode.RegistrationNumber;
			wayBillDocument.DriverFIO = employee.FullName;
			wayBillDocument.DriverLastName = employee.LastName;

			wayBillDocument.MechanicFIO = MechanicFIO;
			wayBillDocument.MechanicLastName = MechanicLastName;

			wayBillDocument.DriverLicense = employee.DrivingLicense;

			wayBillDocument.CarPassportSerialNumber = carNode.DocPTSSeries;
			wayBillDocument.CarPassportNumber = carNode.DocPTSNumber;

			wayBillDocument.GarageLeavingDateTime = generationDate.Add(wayBillDocument.WayBillDocumentItems.First().HoursFrom);
			wayBillDocument.GarageReturningDateTime = generationDate.Add(wayBillDocument.WayBillDocumentItems.Last().HoursTo);
			wayBillDocument.CarFuelType = carNode.FuelType;
			wayBillDocument.CarFuelConsumption = (decimal)carNode.FuelConsumption;

			wayBillDocument.OrganizationName = "vodovoz-spb.ru";
			wayBillDocument.RecalculatePlanedDistance(_distanceCalculator);

			wayBillDocument.Organization = orders.First().Contract.Organization;
			wayBillDocument.PrepareTemplate(_uow, _docTemplateRepository);

			if(wayBillDocument.DocumentTemplate == null)
			{
				throw new Exception($"Не обнаружен шаблон Путевого листа для организации: {wayBillDocument.Organization.Name}");
			}

			((WayBillDocumentParser)wayBillDocument.DocumentTemplate.DocParser).RootObject = wayBillDocument;

			WayBillSelectableDocuments.Add(new SelectablePrintDocument(wayBillDocument));
		}

		private GeoGroupVersion GetActualGeoGroupVersion(Employee employee, DateTime generationDate)
		{
			var geoGroupVersion = employee.Subdivision.GeographicGroup.GetVersionOrNull(generationDate);
			if(geoGroupVersion == null)
			{
				throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать километраж. Так как обслуживаемая часть города подразделения текущего пользователя ({employee.Subdivision.GeographicGroup.Name})" +
					$"не имеет актуальной версии части города на {generationDate}. ");
			}
			return geoGroupVersion;
		}

		private TimeSpan[] GenerateRandomRouteTime()
		{
			var rnd = new Random();
			var rndInt = rnd.Next(0, 2);
			return new[] { _timeSpans[rndInt, 0], _timeSpans[rndInt, 1] };
		}

		#endregion

		#region Export

		public void ExportODTDocuments(string path)
		{
			var odtToPrinter = WayBillSelectableDocuments
				.Where(x => x.Selected)
				.Select(x => x.Document)
				.OfType<IPrintableOdtDocument>()
				.ToList();

			LongOperationDlg.StartOperation(
				delegate(IWorker worker)
				{
					using(QSDocTemplates.FileWorker fileWorker = new QSDocTemplates.FileWorker())
					{
						int step = 0;
						foreach(IPrintableOdtDocument document in odtToPrinter)
						{
							worker.ReportProgress(step, document.Name);
							var filePath = "";
							var template = document.GetTemplate();
							if(template != null)
							{
								filePath = fileWorker.PrepareToExportODT(template, FileEditMode.Document);
							}
							var targetPath = path + "\\" + (template?.Name ?? "TemplateName") + " " + step + ".odt";

							if(File.Exists(targetPath))
							{
								File.SetAttributes(targetPath, FileAttributes.Normal);
								File.Delete(targetPath);
							}

							File.Copy(filePath, targetPath, true);

							step++;
						}
					}
				},
				"Выгрузка файлов...",
				odtToPrinter.Count
			);
		}

		#endregion

		private class CarNode
		{
			public string RegistrationNumber { get; set; }
			public string CarModelName { get; set; }
			public string DocPTSSeries { get; set; }
			public string DocPTSNumber { get; set; }
			public FuelType FuelType { get; set; }
			public double FuelConsumption { get; set; }
		}
	}
}
