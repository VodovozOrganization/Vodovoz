using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
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

        public string OKUD { get; set; }
        public string OKPO { get; set; }

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
        
        string _carModelName;
        [Display (Name = "Модель автомобиля")]
        public virtual string CarModelName {
	        get => _carModelName;
	        set => SetField(ref _carModelName, value);
        }
        
        string carRegistrationNumber;
        [Display (Name = "Регистрационный номер автомобиля")]
        public virtual string CarRegistrationNumber {
            get => carRegistrationNumber;
            set => SetField(ref carRegistrationNumber, value);
        }
        
        string driverFIO;
        [Display (Name = "ФИО водителя")]
        public virtual string DriverFIO {
            get => driverFIO;
            set => SetField(ref driverFIO, value);
        }
        
        string driverLastName;
        [Display (Name = "Фамилия водителя")]
        public virtual string DriverLastName {
            get => driverLastName;
            set => SetField(ref driverLastName, value);
        }

        string mechanicFIO;
        [Display(Name = "ФИО механика")]
        public string MechanicFIO
        {
            get => mechanicFIO;
            set => SetField(ref mechanicFIO, value);
        }

        string mechanicLastName;
        [Display(Name = "Фамилия механика")]
        public string MechanicLastName
        {
            get => mechanicLastName;
            set => SetField(ref mechanicLastName, value);
        }

        string driverLicense;
        [Display (Name = "Водительское удостоверение")]
        public virtual string DriverLicense {
            get => driverLicense;
            set => SetField(ref driverLicense, value);
        }
        
        string carPassportSerialNumber;
        [Display (Name = "ПТС Серии")]
        public virtual string CarPassportSerialNumber {
            get => carPassportSerialNumber;
            set => SetField(ref carPassportSerialNumber, value);
        }
        string carPassportNumber;
        [Display (Name = "ПТС Номер")]
        public virtual string CarPassportNumber {
            get => carPassportNumber;
            set => SetField(ref carPassportNumber, value);
        }
        
        [Display(Name = "Адрес первой подачи")]
        public virtual string FirstAddress => WayBillDocumentItems.First().AddressFrom;
        
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
        
        decimal carFuelConsumption;
        [Display(Name = "Расход топлива")]
        public virtual decimal CarFuelConsumption {
            get { return carFuelConsumption; }
            set { SetField(ref carFuelConsumption, value); }
        }

        [Display(Name = "Выдано по заправочному листу")]
        public decimal FuelByFuelList => CarFuelConsumption * 1.5m;

        [Display (Name = "Всего пробега")]
        public virtual decimal TotalMileage => WayBillDocumentItems.Sum(x => x.Mileage);

        #endregion

        #region Printing

        public PrinterType PrintType => PrinterType.ODT;
        public DocumentOrientation Orientation { get; }
        public int CopiesToPrint { get; set; }
        public string Name => "Путевой лист";

        
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

        public void PrepareTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
        {
            if (DocumentTemplate == null)
            {
                var tempTemplate = docTemplateRepository.GetFirstAvailableTemplate(uow, TemplateType.WayBill, Organization);

                DocTemplate newTemplate = null;

                if (tempTemplate != null)
                {
                    newTemplate = new DocTemplate() // Клонирование шаблона, необходимо, если будете печатать несколько одинаковых ODT
                    {
                        Id = tempTemplate.Id,
                        Name = tempTemplate.Name,
                        Organization = tempTemplate.Organization,
                        ContractType = tempTemplate.ContractType,
                        TempalteFile = tempTemplate.TempalteFile,
                        TemplateType = tempTemplate.TemplateType
                    };
                }
                
                DocumentTemplate = newTemplate;
            }
        }

        #endregion
        
    }
}
