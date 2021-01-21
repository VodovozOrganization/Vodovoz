using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Conventions;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Org.BouncyCastle.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;
using VodovozInfrastructure.Utils;
using VodovozInfrastructure.Utils.NHibernate;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.DocTemplates;

namespace Vodovoz.Additions.Accounting
{
    public class WayBillDocumentGenerator : PropertyChangedBase, IDomainObject
    {
        private readonly IWayBillDocumentRepository repository;
        private readonly RouteGeometryCalculator DistanceCalculator;

        public WayBillDocumentGenerator(IUnitOfWork unitOfWork, IWayBillDocumentRepository repository, RouteGeometryCalculator calculator)
        {
            this.uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.DistanceCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            DocPrinterInit();
        }

        public int Id { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        IUnitOfWork uow;
        RouteList currentRouteList;

        #region Events

        public event EventHandler DocumentsPrinted;
        public event EventHandler PrintingCanceled;

        #endregion

        #region Properties
        public GenericObservableList<SelectablePrintDocument> WayBillSelectableDocuments { get; set; } =
            new GenericObservableList<SelectablePrintDocument>();
        
        // public GenericObservableList<WayBillDocument> wayBillDocuments = new GenericObservableList<WayBillDocument>();
        
//09:00 - 18:00, 11:00 - 21:00, 14:00 - 23:00)
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
            if(!cancelPrinting) {
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
                
                }
            }
        }
      
        #endregion

        #region Generating

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
                return;// TODO gavr тут нужно выводить сообщение или типа того
            
            foreach (var employeer in manOfficialWithCarEmployeers)
                employeeToCars[employeer] = carsStack.Pop();
            
            // Распределяем заказы
            
            // Собираем заказы
            var orders = repository.GetOrdersForWayBillDocuments(uow, startDate, endDate);

            Stack<Domain.Orders.Order> ordersStack = new Stack<Domain.Orders.Order>(orders);
            
            //цикл по дням из выбранной даты
            var daysFromInterval = startDate.Range(endDate);
            foreach (var everydayFromInterval in daysFromInterval)
            {
                // Нужно создать 12-15 адресов в список
                var rand = new Random();
                var rand12_15 = rand.Next(12, 15);
                //Сгенерировать лист точек 
                foreach (var pair in employeeToCars)
                {
                    var wayBillDocument = new WayBillDocument();
                    DeliveryPoint deliveryPointFrom = null;

                    for (int i = 0; i < rand12_15; i++)
                    {
                        if (ordersStack.IsEmpty()) break;
                        var nextOrder = ordersStack.Pop();
                        //для сохранения предыдущих значений для выставления from следующим

                        #region DefaultParameters
                        // параметры не зависящие от того с базы человек едет или между адресами

                        var randomTimeInterval = GenerateRandomRouteTime();
                        var wayBillDocumentItem = new WayBillDocumentItem()
                        {
                            CounterpartyName = nextOrder.Client.Name,
                            SequenceNumber = i + 1,
                            DriverLastName = pair.Key.LastName,
                            HoursFrom = randomTimeInterval[0],
                            HoursTo = randomTimeInterval[1]
                        };
                        #endregion

                        #region FromBaseBasedParameters
                        //Если с базы то считать Дистанцию нужно fromBase методами калькулятора
                        
                        //С базы на адрес
                        if (i == 0)
                        {
                            wayBillDocumentItem.AddressFrom = pair.Key.Subdivision.Name;
                            wayBillDocumentItem.AddressTo = nextOrder.DeliveryPoint.ShortAddress;
                            // wayBillDocumentItem.Mileage =
                            //     DistanceCalculator.DistanceFromBaseMeter(pair.Key.Subdivision.GeographicGroup, nextOrder.DeliveryPoint);
                            wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(pair.Key.Subdivision.GeographicGroup));
                            deliveryPointFrom = nextOrder.DeliveryPoint;
                        }
                        //С адреса на базу
                        else if (i == rand12_15 - 1)
                        {
                            Debug.Assert(deliveryPointFrom != null, nameof(deliveryPointFrom) + " != null");
                            wayBillDocumentItem.AddressFrom = deliveryPointFrom.ShortAddress;
                            wayBillDocumentItem.AddressTo = pair.Key.Subdivision.Name;
                            // wayBillDocumentItem.Mileage = DistanceCalculator.DistanceToBaseMeter(nextOrder.DeliveryPoint,
                            // pair.Key.Subdivision.GeographicGroup);
                            if (nextOrder.DeliveryPoint.CoordinatesExist)
                            {
                                wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(nextOrder.DeliveryPoint));
                            }
                            wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(pair.Key.Subdivision.GeographicGroup));
                        // 0 1 2 0
                        // 0 1 // add 0
                        // 1 2 // add 1
                        // 2 0
                        
                        }
                        //С адреса на адрес
                        else
                        {
                            wayBillDocumentItem.AddressFrom = deliveryPointFrom.ShortAddress;
                            wayBillDocumentItem.AddressTo = nextOrder.DeliveryPoint.Address1c;
                            // wayBillDocumentItem.Mileage = DistanceCalculator.DistanceMeter(deliveryPointFrom,
                                // nextOrder.DeliveryPoint);
                            if (nextOrder.DeliveryPoint.CoordinatesExist)
                            {
                                wayBillDocument.HashPointsOfRoute.Add(CachedDistance.GetHash(nextOrder.DeliveryPoint));
                            }
                            deliveryPointFrom = nextOrder.DeliveryPoint;
                        }
                        
                        #endregion
                        wayBillDocument.WayBillDocumentItems.Add(wayBillDocumentItem);
                    }

                    if (wayBillDocument.WayBillDocumentItems.IsEmpty())
                    {
                        break;
                    }
                    
                    wayBillDocument.Date = DateTime.Now;
                    wayBillDocument.CarModel = pair.Value.Model;
                    wayBillDocument.CarRegistrationNumber = pair.Value.RegistrationNumber;
                    wayBillDocument.DriverFIO = pair.Key.FullName;
                    wayBillDocument.DriverLastName = pair.Key.LastName;
                    wayBillDocument.DriverLicense = pair.Key.DrivingLicense;
                    wayBillDocument.CarPassportSerialNumber = pair.Value.DocPTSSeries;
                    wayBillDocument.CarPassportNumber = pair.Value.DocPTSNumber;
                    
                    wayBillDocument.GarageLeavingDateTime = startDate.Add(wayBillDocument.WayBillDocumentItems.First().HoursFrom) ;
                    wayBillDocument.GarageReturningDateTime = endDate.Add(wayBillDocument.WayBillDocumentItems.Last().HoursFrom);
                    wayBillDocument.CarFuelType = pair.Value.FuelType;
                    wayBillDocument.CarFuelConsumption = pair.Value.FuelConsumption;
                    wayBillDocument.CarFuelConsumption = pair.Value.FuelConsumption;

                    wayBillDocument.OrganizationName = "vodovoz-spb.ru";
                    wayBillDocument.RecalculatePlanedDistance(DistanceCalculator);
                    
                    wayBillDocument.Organization = orders.First().Contract.Organization;
                    wayBillDocument.PrepareTemplate(uow);

                    // Update root Object
                    (wayBillDocument.DocumentTemplate.DocParser as WayBillDocumentParser).RootObject = wayBillDocument;
                    
                    WayBillSelectableDocuments.Add(new SelectablePrintDocument(wayBillDocument));
                } //foreach employee in cars
                
            } //foreach days
            
        }

        #region FillFromToBaseBasedParameters
        //
        // void FillTimeOfWayBillDocumentItemFromBase(
        //     ref WayBillDocumentItem wayBillDocumentItem,
        //     Subdivision subdivision,
        //     DeliveryPoint point )
        // {
        //     var timeFromBaseSec = DistanceCalculator.TimeFromBase(subdivision.GeographicGroup, point);
        //     TimeSpan timeFromBase = TimeSpan.FromSeconds(timeFromBaseSec);
        //     wayBillDocumentItem.HoursFrom = timeFromBase.Hours;
        //     wayBillDocumentItem.MinutesFrom = timeFromBase.Minutes;
        // }
        //
        // void FillTimeOfWayBillDocumentItemToBase(
        //     ref WayBillDocumentItem wayBillDocumentItem,
        //     DeliveryPoint point,
        //     Subdivision subdivision
        // )
        // {
        //     var timeFromBaseSec = DistanceCalculator.TimeToBase(point, subdivision.GeographicGroup);
        //     TimeSpan timeFromBase = TimeSpan.FromSeconds(timeFromBaseSec);
        //     wayBillDocumentItem.HoursFrom = timeFromBase.Hours;
        //     wayBillDocumentItem.MinutesFrom = timeFromBase.Minutes;
        // }
        //
        // void FillTimeOfWayBillDocumentItemFromAdress(
        //     ref WayBillDocumentItem wayBillDocumentItem,
        //     DeliveryPoint fromPoint,
        //     DeliveryPoint toPoint)
        // {
        //     var timeFromBaseSec = DistanceCalculator.TimeSec(fromPoint, toPoint);
        //     TimeSpan timeFromBase = TimeSpan.FromSeconds(timeFromBaseSec);
        //     wayBillDocumentItem.HoursFrom = timeFromBase.Hours;
        //     wayBillDocumentItem.MinutesFrom = timeFromBase.Minutes;
        // }

        #endregion

        TimeSpan[] GenerateRandomRouteTime()
        {
            var rnd = new Random();
            var rndInt = rnd.Next(0, 2);
            return new[] { timeSpans[rndInt, 0], timeSpans[rndInt, 1] };
        }
        
        WayBillDocumentItem FillWayBillDocumentFromOrder(Order order)
        {
            var wayBillDocumentItem = new WayBillDocumentItem();
            wayBillDocumentItem.CounterpartyName = order.Client.Name;
            wayBillDocumentItem.DriverLastName = order.Client.Name;
            wayBillDocumentItem.CounterpartyName = order.Client.Name;
            wayBillDocumentItem.CounterpartyName = order.Client.Name;
            

            return new WayBillDocumentItem();
        }

        #endregion
        
    }
}