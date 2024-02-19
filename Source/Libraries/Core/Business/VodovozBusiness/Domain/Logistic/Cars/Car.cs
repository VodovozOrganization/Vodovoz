using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Type;
using QS.Attachments.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
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
		private IList<Attachment> _attachments = new List<Attachment>();
		private CarModel _carModel;
		private bool _isArchive;
		private IList<CarVersion> _carVersions = new List<CarVersion>();
		private GenericObservableList<Attachment> _observableAttachments;
		private GenericObservableList<CarVersion> _observableCarVersions;
		private IList<OdometerReading> _odometerReadings = new List<OdometerReading>();
		private GenericObservableList<OdometerReading> _observableOdometerReadings;
		private string _carcase;
		private string _chassisNumber;
		private string _color;
		private DateTime? _docIssuedDate;
		private string _docIssuedOrg;
		private string _docNumber;
		private string _docPtsNumber;
		private string _docPtsSeries;
		private string _docSeries;
		private Employee _driver;
		private string _fuelCardNumber;
		private double _fuelConsumption;
		private FuelType _fuelType;
		private IList<GeoGroup> _geographicGroups = new List<GeoGroup>();
		private string _manufactureYear;
		private int _maxBottles;
		private int _maxBottlesFromAddress;
		private int _minBottles;
		private int _minBottlesFromAddress;
		private string _motorNumber;
		private GenericObservableList<GeoGroup> _observableGeographicGroups;
		private int? _orderNumber;
		private byte[] _photo;
		private string _registrationNumber = String.Empty;
		private string _vIn;
		private DateTime? _archivingDate;

		public virtual int Id { get; set; }

		[Display(Name = "Модель")]
		public virtual CarModel CarModel
		{
			get => _carModel;
			set => SetField(ref _carModel, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name ="Дата архивации")]
		public virtual DateTime? ArchivingDate
		{
			get => _archivingDate;
			set => SetField(ref _archivingDate, value);
		}

		public virtual IList<CarVersion> CarVersions
		{
			get => _carVersions;
			set => SetField(ref _carVersions, value);
		}

		public virtual GenericObservableList<CarVersion> ObservableCarVersions => _observableCarVersions
			?? (_observableCarVersions = new GenericObservableList<CarVersion>(CarVersions));

		public virtual IList<OdometerReading> OdometerReadings
		{
			get => _odometerReadings;
			set => SetField(ref _odometerReadings, value);
		}
		
		public virtual GenericObservableList<OdometerReading> ObservableOdometerReadings => _observableOdometerReadings 
		    ?? (_observableOdometerReadings = new GenericObservableList<OdometerReading>(OdometerReadings));

		[Display(Name = "Государственный номер")]
		public virtual string RegistrationNumber
		{
			get => _registrationNumber;
			set => SetField(ref _registrationNumber, value);
		}

		[Display(Name = "VIN")]
		[StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать 17 знаков ")]
		public virtual string VIN
		{
			get => _vIn;
			set => SetField(ref _vIn, value);
		}

		[Display(Name = "Год выпуска")]
		[StringLength(4, MinimumLength = 4, ErrorMessage = "Год выпуска должен содержать 4 знака")]
		public virtual string ManufactureYear
		{
			get => _manufactureYear;
			set => SetField(ref _manufactureYear, value);
		}

		[Display(Name = "Номер двигателя")]
		public virtual string MotorNumber
		{
			get => _motorNumber;
			set => SetField(ref _motorNumber, value);
		}

		[Display(Name = "Номер шасси")]
		public virtual string ChassisNumber
		{
			get => _chassisNumber;
			set => SetField(ref _chassisNumber, value);
		}

		[Display(Name = "Кузов")]
		public virtual string Carcase
		{
			get => _carcase;
			set => SetField(ref _carcase, value);
		}

		[Display(Name = "Цвет")]
		public virtual string Color
		{
			get => _color;
			set => SetField(ref _color, value);
		}

		[Display(Name = "Серия свидетельства о регистрации ТС")]
		public virtual string DocSeries
		{
			get => _docSeries;
			set => SetField(ref _docSeries, value);
		}

		[Display(Name = "Номер свидетельства о регистрации ТС")]
		public virtual string DocNumber
		{
			get => _docNumber;
			set => SetField(ref _docNumber, value);
		}

		[Display(Name = "Серия паспорта ТС")]
		public virtual string DocPTSSeries
		{
			get => _docPtsSeries;
			set => SetField(ref _docPtsSeries, value);
		}

		[Display(Name = "Номер паспорта ТС")]
		public virtual string DocPTSNumber
		{
			get => _docPtsNumber;
			set => SetField(ref _docPtsNumber, value);
		}

		[Display(Name = "Кем выдано свидетельство о регистрации ТС")]
		public virtual string DocIssuedOrg
		{
			get => _docIssuedOrg;
			set => SetField(ref _docIssuedOrg, value);
		}

		[Display(Name = "Дата выдачи свидетельства о регистрации ТС")]
		public virtual DateTime? DocIssuedDate
		{
			get => _docIssuedDate;
			set => SetField(ref _docIssuedDate, value);
		}

		[Display(Name = "Расход топлива")]
		public virtual double FuelConsumption
		{
			get => GetFuelConsumption();
		}

		[Display(Name = "Вид топлива")]
		public virtual FuelType FuelType
		{
			get => _fuelType;
			set => SetField(ref _fuelType, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Минимум бутылей")]
		public virtual int MinBottles
		{
			get => _minBottles;
			set => SetField(ref _minBottles, value);
		}

		[Display(Name = "Максимум бутылей")]
		public virtual int MaxBottles
		{
			get => _maxBottles;
			set => SetField(ref _maxBottles, value);
		}

		[Display(Name = "Минимальное количество бутылей для перевозки с адреса")]
		public virtual int MinBottlesFromAddress
		{
			get => _minBottlesFromAddress;
			set => SetField(ref _minBottlesFromAddress, value);
		}

		[Display(Name = "Максимальное количество бутылей для перевозки с адреса")]
		public virtual int MaxBottlesFromAddress
		{
			get => _maxBottlesFromAddress;
			set => SetField(ref _maxBottlesFromAddress, value);
		}

		[Display(Name = "Фотография")]
		public virtual byte[] Photo
		{
			get => _photo;
			set => SetField(ref _photo, value);
		}

		[Display(Name = "Номер топливной карты")]
		public virtual string FuelCardNumber
		{
			get => _fuelCardNumber;
			set => SetField(ref _fuelCardNumber, value);
		}

		[Display(Name = "Порядковый номер автомобиля")]
		public virtual int? OrderNumber
		{
			get => _orderNumber;
			set
			{
				if(value == 0)
				{
					SetField(ref _orderNumber, null);
				}
				else
				{
					SetField(ref _orderNumber, value);
				}
			}
		}

		[Display(Name = "Группа района")]
		public virtual IList<GeoGroup> GeographicGroups
		{
			get => _geographicGroups;
			set => SetField(ref _geographicGroups, value);
		}

		[Display(Name = "Прикрепленные файлы")]
		public virtual IList<Attachment> Attachments
		{
			get => _attachments;
			set => SetField(ref _attachments, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeoGroup> ObservableGeographicGroups =>
			_observableGeographicGroups ?? (_observableGeographicGroups = new GenericObservableList<GeoGroup>(GeographicGroups));

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Attachment> ObservableAttachments =>
			_observableAttachments ?? (_observableAttachments = new GenericObservableList<Attachment>(Attachments));

		public virtual string Title => $"{CarModel?.Name} ({RegistrationNumber})";

		/// <param name="dateTime">Если равно null, возвращает активную версию на текущее время</param>
		public virtual CarVersion GetActiveCarVersionOnDate(DateTime? dateTime = null)
		{
			if(dateTime.HasValue)
			{
				return ObservableCarVersions.FirstOrDefault(x =>
					x.StartDate <= dateTime && (x.EndDate == null || x.EndDate >= dateTime));
			}
			var currentDateTime = DateTime.Now;
			return ObservableCarVersions.FirstOrDefault(x =>
				x.StartDate <= currentDateTime && (x.EndDate == null || x.EndDate >= currentDateTime));
		}

		public static CarTypeOfUse[] GetCarTypesOfUseForRatesLevelWageCalculation() => new[] { CarTypeOfUse.Largus, CarTypeOfUse.GAZelle };

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(RegistrationNumber))
			{
				yield return new ValidationResult("Гос. номер автомобиля должен быть заполнен", new[] { nameof(RegistrationNumber) });
			}

			if(FuelType == null)
			{
				yield return new ValidationResult("Тип топлива должен быть заполнен", new[] { nameof(FuelType) });
			}
			
			if(CarModel == null)
			{
				yield return new ValidationResult("Модель должна быть заполнена", new[] { nameof(CarModel) });
			}

			if(FuelConsumption <= 0)
			{
				yield return new ValidationResult("Расход топлива должен быть больше 0", new[] { nameof(FuelConsumption) });
			}

			var cars = UoW.Session.QueryOver<Car>()
				.Where(c => c.RegistrationNumber == RegistrationNumber)
				.WhereNot(c => c.Id == Id)
				.List();

			if(cars.Any())
			{
				yield return new ValidationResult("Автомобиль уже существует", new[] { "Duplication" });
			}

			if(!CarVersions.Any())
			{
				yield return new ValidationResult("Должна быть создана хотя бы одна версия", new[] { nameof(CarVersions) });
			}

			if(Driver != null)
			{
				var driversCar = UoW.Session.QueryOver<Car>().Where(x => x.Driver.Id == Driver.Id).List().FirstOrDefault(x => x.Id != Id);
				if(driversCar != null)
				{
					yield return new ValidationResult($"У водителя уже есть автомобиль\nГос. номер: {driversCar.RegistrationNumber}\n" +
						"Отправьте его в архив, а затем повторите закрепление еще раз.", new[] { nameof(Car) });
				}
			}
		}

		private double GetFuelConsumption()
		{
			if(CarModel is null)
			{
				return 0;
			}
			
			var result = CarModel.CarFuelVersions.OrderByDescending(x => x.StartDate).FirstOrDefault()?.FuelConsumption;

			return result ?? 0;
		}
	}

	public class CarTypeOfUseStringType : EnumStringType
	{
		public CarTypeOfUseStringType() : base(typeof(CarTypeOfUse))
		{ }
	}

	public class GenderStringType : EnumStringType
	{
		public GenderStringType() : base(typeof(Gender))
		{ }
	}
}
