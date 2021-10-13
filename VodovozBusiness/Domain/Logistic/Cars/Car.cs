using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Attachments.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "автомобили",
		Nominative = "автомобиль")]
	[EntityPermission]
	[HistoryTrace]
	public class Car : BusinessObjectBase<Car>, IDomainObject, IValidatableObject
	{
		#region Свойства

		private IList<Attachment> _attachments = new List<Attachment>();
		private GenericObservableList<Attachment> _observableAttachments;

		public virtual int Id { get; set; }

		private CarModel _carModel;
		[Display(Name = "Модель")]
		public virtual CarModel CarModel {
			get => _carModel;
			set => SetField(ref _carModel, value);
		}

		private IList<CarVersion> _carVersions = new List<CarVersion>();

		public virtual IList<CarVersion> CarVersions
		{
			get => _carVersions;
			set => SetField(ref _carVersions, value);
		}

		private GenericObservableList<CarVersion> _observableCarVersions;

		public virtual GenericObservableList<CarVersion> ObservableCarVersions => _observableCarVersions ??
		                                                                             (_observableCarVersions = new GenericObservableList<CarVersion>(CarVersions));


		string registrationNumber;
		[Display(Name = "Государственный номер")]
		public virtual string RegistrationNumber {
			get { return registrationNumber; }
			set { SetField(ref registrationNumber, value, () => RegistrationNumber); }
		}

		string vIN;
		[Display(Name = "VIN")]
		[StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать 17 знаков ")]
		public virtual string VIN {
			get { return vIN; }
			set { SetField(ref vIN, value, () => VIN); }
		}

		string manufactureYear;
		[Display(Name = "Год выпуска")]
		[StringLength(4, MinimumLength = 4, ErrorMessage = "Год выпуска должен содержать 4 знака")]
		public virtual string ManufactureYear {
			get { return manufactureYear; }
			set { SetField(ref manufactureYear, value, () => ManufactureYear); }
		}

		string motorNumber;
		[Display(Name = "Номер двигателя")]
		public virtual string MotorNumber {
			get { return motorNumber; }
			set { SetField(ref motorNumber, value, () => MotorNumber); }
		}

		string chassisNumber;
		[Display(Name = "Номер шасси")]
		public virtual string ChassisNumber {
			get { return chassisNumber; }
			set { SetField(ref chassisNumber, value, () => ChassisNumber); }
		}

		string carcase;
		[Display(Name = "Кузов")]
		public virtual string Carcase {
			get { return carcase; }
			set { SetField(ref carcase, value, () => Carcase); }
		}

		string color;
		[Display(Name = "Цвет")]
		public virtual string Color {
			get { return color; }
			set { SetField(ref color, value, () => Color); }
		}

		string docSeries;
		[Display(Name = "Серия свидетельства о регистрации ТС")]
		public virtual string DocSeries {
			get { return docSeries; }
			set { SetField(ref docSeries, value, () => DocSeries); }
		}

		string docNumber;
		[Display(Name = "Номер свидетельства о регистрации ТС")]
		public virtual string DocNumber {
			get { return docNumber; }
			set { SetField(ref docNumber, value, () => DocNumber); }
		}
		
		string docPTSSeries;
		[Display(Name = "Серия паспорта ТС")]
		public virtual string DocPTSSeries {
			get { return docPTSSeries; }
			set { SetField(ref docPTSSeries, value); }
		}

		string docPTSNumber;
		[Display(Name = "Номер паспорта ТС")]
		public virtual string DocPTSNumber {
			get { return docPTSNumber; }
			set { SetField(ref docPTSNumber, value); }
		}
		

		string docIssuedOrg;
		[Display(Name = "Кем выдано свидетельство о регистрации ТС")]
		public virtual string DocIssuedOrg {
			get { return docIssuedOrg; }
			set { SetField(ref docIssuedOrg, value, () => DocIssuedOrg); }
		}

		DateTime? docIssuedDate;

		[Display(Name = "Дата выдачи свидетельства о регистрации ТС")]
		public virtual DateTime? DocIssuedDate {
			get { return docIssuedDate; }
			set { SetField(ref docIssuedDate, value, () => DocIssuedDate); }
		}

		double fuelConsumption;

		[Display(Name = "Расход топлива")]
		public virtual double FuelConsumption {
			get { return fuelConsumption; }
			set { SetField(ref fuelConsumption, value, () => FuelConsumption); }
		}

		FuelType fuelType;

		[Display(Name = "Вид топлива")]
		public virtual FuelType FuelType {
			get { return fuelType; }
			set { SetField(ref fuelType, value, () => FuelType); }
		}

		Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		int minBottles;

		[Display(Name = "Минимум бутылей")]
		public virtual int MinBottles {
			get => minBottles;
			set => SetField(ref minBottles, value);
		}

		int maxBottles;

		[Display(Name = "Максимум бутылей")]
		public virtual int MaxBottles {
			get => maxBottles;
			set => SetField(ref maxBottles, value);
		}

		int minBottlesFromAddress;
		[Display(Name = "Минимальное количество бутылей для перевозки с адреса")]
		public virtual int MinBottlesFromAddress {
			get => minBottlesFromAddress;
			set => SetField(ref minBottlesFromAddress, value);
		}

		int maxBottlesFromAddress;
		[Display(Name = "Максимальное количество бутылей для перевозки с адреса")]
		public virtual int MaxBottlesFromAddress {
			get => maxBottlesFromAddress;
			set => SetField(ref maxBottlesFromAddress, value);
		}

		byte[] photo;

		[Display(Name = "Фотография")]
		public virtual byte[] Photo {
			get => photo;
			set => SetField(ref photo, value, () => Photo);
		}

		private string fuelCardNumber;
		[Display(Name = "Номер топливной карты")]
		public virtual string FuelCardNumber {
			get => fuelCardNumber;
			set => SetField(ref fuelCardNumber, value, () => FuelCardNumber);
		}
		
		private DriverCarKind driverCarKind;
		[Display(Name = "Вид наёмного автомобиля")]
		public virtual DriverCarKind DriverCarKind {
			get => driverCarKind;
			set => SetField(ref driverCarKind, value);
		}

		private int? orderNumber;
		[Display(Name = "Порядковый номер автомобиля")]
		public virtual int? OrderNumber {
			get => orderNumber;
			set {
				if (value == 0)
                {
					SetField(ref orderNumber, null);
				}
				else
                {
					SetField(ref orderNumber, value);
				}
			}
		}

		IList<GeographicGroup> geographicGroups = new List<GeographicGroup>();
		[Display(Name = "Группа района")]
		public virtual IList<GeographicGroup> GeographicGroups {
			get => geographicGroups;
			set => SetField(ref geographicGroups, value, () => GeographicGroups);
		}

		[Display(Name = "Прикрепленные файлы")]
		public virtual IList<Attachment> Attachments {
			get => _attachments;
			set => SetField(ref _attachments, value);
		}
		
		GenericObservableList<GeographicGroup> observableGeographicGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeographicGroup> ObservableGeographicGroups {
			get {
				if(observableGeographicGroups == null)
					observableGeographicGroups = new GenericObservableList<GeographicGroup>(GeographicGroups);
				return observableGeographicGroups;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Attachment> ObservableAttachments =>
			_observableAttachments ?? (_observableAttachments = new GenericObservableList<Attachment>(Attachments));

		#endregion

		public virtual string Title => $"{CarModel.Name} ({RegistrationNumber})";
		public virtual bool CanEditFuelCardNumber => ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_fuel_card_number");

		public virtual CarVersion GetActiveCarVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableCarVersions.SingleOrDefault(x =>
					x.StartDate <= activationTime && (x.EndDate == null || x.EndDate <= activationTime?.Date.AddDays(1)));
			return ObservableCarVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		
		public Car()
		{
			RegistrationNumber = String.Empty;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(RegistrationNumber))
				yield return new ValidationResult("Гос. номер автомобиля должен быть заполнен", new[] { "RegistrationNumber" });

			if(FuelType == null)
				yield return new ValidationResult("Тип топлива должен быть заполнен", new[] { "FuelType" });

			if(FuelConsumption <= 0)
				yield return new ValidationResult("Расход топлива должен быть больше 0", new[] { "FuelConsumption" });
			
			var cars = UoW.Session.QueryOver<Car>()
				.Where(c => c.RegistrationNumber == this.RegistrationNumber)
				.WhereNot(c => c.Id == this.Id)
				.List();

			if(cars.Any())
				yield return new ValidationResult("Автомобиль уже существует", new[] { "Duplication" });
				
			if(Driver != null) {
				var driversCar = UoW.Session.QueryOver<Car>().Where(x => x.Driver.Id == Driver.Id).List().FirstOrDefault(x => x.Id != this.Id);
				if(driversCar != null) {
					yield return new ValidationResult($"У водителя уже есть автомобиль\nГос. номер: {driversCar.RegistrationNumber}\n" +
						"Отправьте его в архив, а затем повторите закрепление еще раз.", new[] { nameof(Car) });
				}
			}
		}
		#endregion
	}

	public class CarTypeOfUseStringType : NHibernate.Type.EnumStringType
	{
		public CarTypeOfUseStringType() : base(typeof(CarTypeOfUse)) { }
	}
	
	public class GenderStringType : NHibernate.Type.EnumStringType
	{
		public GenderStringType() : base(typeof(Gender)) { }
	}
}

