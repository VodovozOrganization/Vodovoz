using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Domain.Documents
{
    public class WayBillDocument : PropertyChangedBase, IPrintableOdtDocument, ITemplateOdtDocument
    {

        #region DistanceCalc

        public virtual void RecalculatePlanedDistance(RouteGeometryCalculator distanceCalculator)
        {
            if(WayBillDocumentItems.Count == 0)
                PlanedDistance = 0;
            else
                PlanedDistance = distanceCalculator.GetRouteDistance(HashPointsOfRoute.ToArray()) / 1000m;
        }

        public List<long> HashPointsOfRoute = new List<long>();

        #endregion
        
        #region Properties
        public virtual int Id { get; set; }

        public List<WayBillDocumentItem> WayBillDocumentItems { get; set; } = new List<WayBillDocumentItem>();
        
        private decimal? planedDistance;

        /// <summary>
        /// Расстояние в километрах
        /// </summary>
        [Display(Name = "Планируемое расстояние")]
        public virtual decimal? PlanedDistance {
            get => planedDistance;
            protected set => SetField(ref planedDistance, value);
        }
        
        DateTime date;
        [Display (Name = "Дата")]
        public virtual DateTime Date {
            get => date;
            set => SetField(ref date, value);
        }
        
        string organizationName;
        [Display (Name = "Название организации")]
        public virtual string OrganizationName {
            get => organizationName;
            set => SetField(ref organizationName, value);
        }
        
        string carModel;
        [Display (Name = "Модель автомобиля")]
        public virtual string CarModel {
            get => carModel;
            set => SetField(ref carModel, value);
        }
        
        string carRegistrationNumber;
        [Display (Name = "Модель автомобиля")]
        public virtual string CarRegistrationNumber {
            get => carRegistrationNumber;
            set => SetField(ref carRegistrationNumber, value);
        }
        
        string driverFIO;
        [Display (Name = "Модель автомобиля")]
        public virtual string DriverFIO {
            get => driverFIO;
            set => SetField(ref driverFIO, value);
        }
        
        string driverLastName;
        [Display (Name = "Модель автомобиля")]
        public virtual string DriverLastName {
            get => driverLastName;
            set => SetField(ref driverLastName, value);
        }
        string driverLicense;
        [Display (Name = "Модель автомобиля")]
        public virtual string DriverLicense {
            get => driverLicense;
            set => SetField(ref driverLicense, value);
        }
        
        string carPassportSerialNumber;
        [Display (Name = "Модель автомобиля")]
        public virtual string CarPassportSerialNumber {
            get => carPassportSerialNumber;
            set => SetField(ref carPassportSerialNumber, value);
        }
        string carPassportNumber;
        [Display (Name = "Модель автомобиля")]
        public virtual string CarPassportNumber {
            get => carPassportNumber;
            set => SetField(ref carPassportNumber, value);
        }
        
        string firstAddress;

        [Display(Name = "Модель автомобиля")]
        public virtual string FirstAddress => WayBillDocumentItems.First().AddressTo;
        
        DateTime garageLeavingDateTime;
        [Display (Name = "Время выезда из гаража")]
        public virtual DateTime GarageLeavingDateTime {
            get => garageLeavingDateTime;
            set => SetField(ref garageLeavingDateTime, value);
        }
        DateTime garageReturningDateTime;
        [Display (Name = "Время возвращения в гараж")]
        public virtual DateTime GarageReturningDateTime {
            get => garageReturningDateTime;
            set => SetField(ref garageReturningDateTime, value);
        }
        
        FuelType carFuelType;
        [Display (Name = "Тип бензина")]
        public virtual FuelType CarFuelType {
            get => carFuelType;
            set => SetField(ref carFuelType, value);
        }
        
        double carFuelConsumption;
        [Display(Name = "Расход топлива")]
        public virtual double CarFuelConsumption {
            get { return carFuelConsumption; }
            set { SetField(ref carFuelConsumption, value); }
        }
        //
        // [Display (Name = "Всего пробега")]
        // public virtual decimal TotalMileage => WayBillDocumentItems.Sum(x => x.Mileage);
        // [Display (Name = "Всего работал")]
        // public virtual decimal TotalWorkTime => WayBillDocumentItems.Sum(x => 0);
        //
        #endregion



        #region Printing

        public PrinterType PrintType => PrinterType.ODT;
        public DocumentOrientation Orientation { get; }
        public int CopiesToPrint { get; set; }
        public string Name { get; }

        
        DocTemplate wayBillTemplate;
        [Display (Name = "Шаблон документа")]
        public virtual DocTemplate DocumentTemplate {
            get => wayBillTemplate;
            protected set => SetField(ref wayBillTemplate, value);
        }
        public virtual IDocTemplate GetTemplate() => DocumentTemplate;

        Organization organization;
        [Display(Name = "Организация")]
        public virtual Organization Organization
        {
            get { return organization; }
            set { SetField(ref organization, value, () => Organization); }
        }
        public void PrepareTemplate(IUnitOfWork uow)
        {
            if (DocumentTemplate == null)
            {
                var newTemplate = Repository.Client.DocTemplateRepository.GetFirstAvailableTemplate(uow, TemplateType.WayBill, Organization);
                DocumentTemplate = newTemplate;
            }

        }

        #endregion
        
    }
}