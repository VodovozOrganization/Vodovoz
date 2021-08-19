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
using QS.Dialog.GtkUI;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Additions.Accounting
{
    public class WayBillDocumentGenerator : PropertyChangedBase, IDomainObject
    {
        private readonly IWayBillDocumentRepository _wayBillDocumentRepository;
        private readonly RouteGeometryCalculator _distanceCalculator;
        private readonly IDocTemplateRepository _docTemplateRepository;

        public WayBillDocumentGenerator(
	        IUnitOfWork unitOfWork,
	        IWayBillDocumentRepository repository,
	        RouteGeometryCalculator calculator,
	        IDocTemplateRepository docTemplateRepository)
        {
            uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _wayBillDocumentRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _distanceCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _docTemplateRepository = docTemplateRepository ?? throw new ArgumentNullException(nameof(docTemplateRepository));
            DocPrinterInit();
        }

        public int Id { get; }

        string mechanicFIO;
        public string MechanicFIO
        {
            get => mechanicFIO;
            set => SetField(ref mechanicFIO, value);
        }

        string mechanicLastName;
        public string MechanicLastName
        {
            get => mechanicLastName;
            set => SetField(ref mechanicLastName, value);
        }

        IUnitOfWork uow;

        #region Events

        public event EventHandler DocumentsPrinted;
        public event EventHandler PrintingCanceled;

        #endregion

        #region Properties
        public GenericObservableList<SelectablePrintDocument> WayBillSelectableDocuments { get; set; } =
            new GenericObservableList<SelectablePrintDocument>();
        
        private TimeSpan[,] timeSpans = {
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
            },
        };


        private DateTime startDate;
        public DateTime StartDate {
            get => startDate;
            set => SetField(ref startDate, value);
        }

        private DateTime endDate;
        public DateTime EndDate {
            get => endDate;
            set => SetField(ref endDate, value);
        }
        #endregion

        #region Printing

        public static PrintSettings PrinterSettings { get; set; }
        bool cancelPrinting = false;
        public string ODTTemplateNotFoundMessages { get; set; }
        
        public MultipleDocumentPrinter MultiDocPrinter { get; set; }
        
        void DocPrinterInit()
        {
            MultiDocPrinter = new MultipleDocumentPrinter {
                PrintableDocuments = new GenericObservableList<SelectablePrintDocument>(WayBillSelectableDocuments)
            };
            MultiDocPrinter.PrintingCanceled += (o, args) => PrintingCanceled?.Invoke(o, args);
        }
        
        public void PrintSelected(SelectablePrintDocument document = null)
        {
            if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                var settingsOperaation = new PrintOperation();
                settingsOperaation.Run(PrintOperationAction.PrintDialog, null);
                PrinterSettings = settingsOperaation.PrintSettings;
            }

            if (!cancelPrinting) {
                MultiDocPrinter.PrinterSettings = PrinterSettings;
                if(document == null)
                    MultiDocPrinter.PrintSelectedDocuments();
                else
                    MultiDocPrinter.PrintDocument(document);
                PrinterSettings = MultiDocPrinter.PrinterSettings;
            } else 
                PrintingCanceled?.Invoke(this, new EventArgs());
        }

        public void PrintDocuments()
        {
            if (WayBillSelectableDocuments.Count == WayBillSelectableDocuments.Count(x => x.Document is WayBillDocument))
            {
                QSMain.WaitRedraw();

                PrintingCanceled += (sender, args) =>
                {
                    cancelPrinting = true;
                };
                
                PrintSelected();
                if (!string.IsNullOrEmpty(ODTTemplateNotFoundMessages))
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

            var manOfficialWithCarEmployeers = uow.Session.QueryOver<Employee>()
                .Where(x => x.Gender == Gender.male)
                .WhereStringIsNotNullOrEmpty(x => x.DrivingLicense)
                .And(x => x.Registration == RegistrationType.LaborCode)
                .List();
            
            var cars = uow.Session.QueryOver<Car>()
                .And(Restrictions.On<Car>(x => x.TypeOfUse)
                    .IsIn(new[] {CarTypeOfUse.CompanyLargus, CarTypeOfUse.CompanyGAZelle}))
                .List<Car>();
            
            //Распределяем автомобили на сотрудников
            var employeeToCars = new Dictionary<Employee, Car>();
            Stack<Car> carsStack = new Stack<Car>(cars);
            
            if (cars.Count < manOfficialWithCarEmployeers.Count)
            {
                MessageDialogHelper.RunWarningDialog("Количество водителей больше количества автомобилей");
                return;
            }
            
            foreach (var employee in manOfficialWithCarEmployeers)
                employeeToCars[employee] = carsStack.Pop();
            
            var randomizer = new Random();

            foreach (var day in startDate.Range(endDate)) {
                var currentDayOrders = _wayBillDocumentRepository.GetOrdersForWayBillDocuments(uow, day, day.AddHours(23).AddMinutes(59).AddSeconds(59));

                foreach (var employeeToCarPair in employeeToCars)
                {
                    var routesCount = Math.Min(randomizer.Next(12, 15), currentDayOrders.Count);
                    var randomTimeInterval = GenerateRandomRouteTime();
                    GenerateWayBill(currentDayOrders.Take(routesCount).ToList(), routesCount, day, randomTimeInterval, employeeToCarPair.Key, employeeToCarPair.Value );
                    
                    currentDayOrders = currentDayOrders.Skip(routesCount).ToList();
                }
            }
        }

        private void GenerateWayBill(IList<Order> orders, int waypointsCount, DateTime generationDate, TimeSpan[] timeInterval, Employee employee, Car car)
        {
            var wayBillDocument = new WayBillDocument();

            if (orders.IsEmpty())
            {
                return;
            }

            var orderEnumerator = orders.GetEnumerator();

            for (var i = 0; orderEnumerator.MoveNext() == true && i < waypointsCount; i++)
            {
                var wayBillDocumentItem = new WayBillDocumentItem()
                {
                    CounterpartyName = orderEnumerator.Current.Client.Name,
                    DriverLastName = employee.LastName,
                    HoursFrom = timeInterval[0],
                    HoursTo = timeInterval[1],
                    AddressTo = orderEnumerator.Current.DeliveryPoint.ShortAddress
                };

                wayBillDocument.WayBillDocumentItems.Add(wayBillDocumentItem);
            }

            wayBillDocument.WayBillDocumentItems.First().AddressFrom = employee.Subdivision.Name;

            wayBillDocument.WayBillDocumentItems.Last().AddressTo = employee.Subdivision.Name;

            var waybillItemsEnumerator = wayBillDocument.WayBillDocumentItems.GetEnumerator();

            string lastAddressTo = "";

            orderEnumerator.Reset();
            DeliveryPoint deliveryPointFrom = null;

            var lastId = wayBillDocument.WayBillDocumentItems.Count - 2;
            for (var i = 0; waybillItemsEnumerator.MoveNext() == true && orderEnumerator.MoveNext() == true; i++)
            {
                if (i != 0 && i != waypointsCount)
                {
                    waybillItemsEnumerator.Current.AddressFrom = lastAddressTo;
                }
                lastAddressTo = waybillItemsEnumerator.Current.AddressTo;
                waybillItemsEnumerator.Current.SequenceNumber = i + 1;

                if (i == 0)
                {
                    waybillItemsEnumerator.Current.Mileage =
                        _distanceCalculator.DistanceFromBaseMeter(employee.Subdivision.GeographicGroup, orderEnumerator.Current.DeliveryPoint) * 2 / 1000;

                    wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(employee.Subdivision.GeographicGroup));
                    deliveryPointFrom = orderEnumerator.Current.DeliveryPoint;
                }
                else if (i == lastId)
                {
                    waybillItemsEnumerator.Current.Mileage = _distanceCalculator.DistanceToBaseMeter(orderEnumerator.Current.DeliveryPoint,
                        employee.Subdivision.GeographicGroup) * 2 / 1000;

                    if (orderEnumerator.Current.DeliveryPoint.CoordinatesExist)
                    {
                        wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(orderEnumerator.Current.DeliveryPoint));
                    }
                    wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(employee.Subdivision.GeographicGroup));
                }
                else
                {
                    waybillItemsEnumerator.Current.Mileage = _distanceCalculator.DistanceMeter(deliveryPointFrom,
                        orderEnumerator.Current.DeliveryPoint) * 2 / 1000;

                    if (orderEnumerator.Current.DeliveryPoint.CoordinatesExist)
                    {
                        wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(orderEnumerator.Current.DeliveryPoint));
                    }
                    deliveryPointFrom = orderEnumerator.Current.DeliveryPoint;
                }
            }

            if (wayBillDocument.WayBillDocumentItems.IsEmpty())
            {
                return;
            }

            wayBillDocument.Date = generationDate.Date;
            wayBillDocument.CarModel = car.Model;
            wayBillDocument.CarRegistrationNumber = car.RegistrationNumber;
            wayBillDocument.DriverFIO = employee.FullName;
            wayBillDocument.DriverLastName = employee.LastName;

            wayBillDocument.MechanicFIO = MechanicFIO;
            wayBillDocument.MechanicLastName = MechanicLastName;

            wayBillDocument.DriverLicense = employee.DrivingLicense;

            wayBillDocument.CarPassportSerialNumber = car.DocPTSSeries;
            wayBillDocument.CarPassportNumber = car.DocPTSNumber;

            wayBillDocument.GarageLeavingDateTime = generationDate.Add(wayBillDocument.WayBillDocumentItems.First().HoursFrom);
            wayBillDocument.GarageReturningDateTime = generationDate.Add(wayBillDocument.WayBillDocumentItems.Last().HoursTo);
            wayBillDocument.CarFuelType = car.FuelType;
            wayBillDocument.CarFuelConsumption = (decimal)car.FuelConsumption;

            wayBillDocument.OrganizationName = "vodovoz-spb.ru";
            wayBillDocument.RecalculatePlanedDistance(_distanceCalculator);

            wayBillDocument.Organization = orders.First().Contract.Organization;
            wayBillDocument.PrepareTemplate(uow, _docTemplateRepository);

            if (wayBillDocument.DocumentTemplate == null)
            {
                throw new Exception($"Не обнаружен шаблон Путевого листа для организации: {wayBillDocument.Organization.Name}");
            }

            (wayBillDocument.DocumentTemplate.DocParser as WayBillDocumentParser).RootObject = wayBillDocument;

            WayBillSelectableDocuments.Add(new SelectablePrintDocument(wayBillDocument));
        }

        TimeSpan[] GenerateRandomRouteTime()
        {
            var rnd = new Random();
            var rndInt = rnd.Next(0, 2);
            return new[] { timeSpans[rndInt, 0], timeSpans[rndInt, 1] };
        }

        #endregion

        #region Export

        public void ExportODTDocuments(string path)
        {

            List<IPrintableDocument> odtToPrinter = new List<IPrintableDocument>();
            foreach (var document in WayBillSelectableDocuments.Where(x => x.Selected).Select(x => x.Document))
            {
                odtToPrinter.Add(document);
            }

            var result = LongOperationDlg.StartOperation(
                delegate (IWorker worker) {
                    using (FileWorker fileWorker = new FileWorker())
                    {
                        int step = 0;
                        foreach (IPrintableOdtDocument document in odtToPrinter)
                        {
                            worker.ReportProgress(step, document.Name);
                            var filePath = "";
                            var template = document.GetTemplate();
                            if (template != null)
                            {
                                filePath = fileWorker.PrepareToExportODT(template, FileEditMode.Document);
                            }
                            var targetPath = path + "\\" + template.Name + " " + step + ".odt";

                            if (File.Exists(targetPath))
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
                odtToPrinter.Count()
            );
            if (result == LongOperationResult.Canceled)
                return;
        }

        #endregion
    }
}