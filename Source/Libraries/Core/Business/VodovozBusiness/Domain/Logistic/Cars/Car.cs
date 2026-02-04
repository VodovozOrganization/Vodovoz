using QS.Attachments.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class Car : CarEntity, IValidatableObject, IHasPhoto
	{
		private CarModel _carModel;
		private bool _isArchive;
		private IList<CarVersion> _carVersions = new List<CarVersion>();
		private GenericObservableList<Attachment> _observableAttachments;
		private GenericObservableList<CarVersion> _observableCarVersions;
		private IList<OdometerReading> _odometerReadings = new List<OdometerReading>();
		private GenericObservableList<OdometerReading> _observableOdometerReadings;
		private IList<FuelCardVersion> _fuelCardVersions = new List<FuelCardVersion>();
		private GenericObservableList<FuelCardVersion> _observableFuelCardVersions;
		private IList<CarInsurance> _carInsurances = new List<CarInsurance>();
		private GenericObservableList<CarInsurance> _observableCarInsurances;
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
		private string _vIn;
		private DateTime? _archivingDate;
		private ArchivingReason? _archivingReason;
		private int _leftUntilTechInspect;
		private IncomeChannel _incomeChannel;
		private bool _isKaskoInsuranceNotRelevant = true;
		private int? _techInspectForKm;
		private string _photoFileName;

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
			set
			{
				if(SetField(ref _isArchive, value) && !value)
				{
					ArchivingReason = null;
				}
			}
		}

		[Display(Name = "Дата архивации")]
		public virtual DateTime? ArchivingDate
		{
			get => _archivingDate;
			set => SetField(ref _archivingDate, value);
		}

		[Display(Name = "Причина архивации")]
		public virtual ArchivingReason? ArchivingReason
		{
			get => _archivingReason;
			set => SetField(ref _archivingReason, value);
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

		public virtual IList<FuelCardVersion> FuelCardVersions
		{
			get => _fuelCardVersions;
			set => SetField(ref _fuelCardVersions, value);
		}

		public virtual GenericObservableList<FuelCardVersion> ObservableFuelCardVersions => _observableFuelCardVersions
			?? (_observableFuelCardVersions = new GenericObservableList<FuelCardVersion>(FuelCardVersions));

		public virtual IList<CarInsurance> CarInsurances
		{
			get => _carInsurances;
			set => SetField(ref _carInsurances, value);
		}

		public virtual GenericObservableList<CarInsurance> ObservableCarInsurances => _observableCarInsurances
			?? (_observableCarInsurances = new GenericObservableList<CarInsurance>(CarInsurances));

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

		[Display(Name = "Имя файла фотографии")]
		public virtual string PhotoFileName
		{
			get => _photoFileName;
			set => SetField(ref _photoFileName, value);
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

		[Display(Name = "Осталось до ТО, км")]
		public virtual int LeftUntilTechInspect
		{
			get => _leftUntilTechInspect;
			set => SetField(ref _leftUntilTechInspect, value);
		}

		[Display(Name = "ТО на км")]
		public virtual int? TechInspectForKm
		{
			get => _techInspectForKm;
			set => SetField(ref _techInspectForKm, value);
		}

		[Display(Name = "Канал поступления")]
		public virtual IncomeChannel IncomeChannel
		{
			get => _incomeChannel;
			set => SetField(ref _incomeChannel, value);
		}

		[Display(Name = "Страховка Каско не актуальна для данного ТС")]
		public virtual bool IsKaskoInsuranceNotRelevant
		{
			get => _isKaskoInsuranceNotRelevant;
			set => SetField(ref _isKaskoInsuranceNotRelevant, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeoGroup> ObservableGeographicGroups =>
			_observableGeographicGroups ?? (_observableGeographicGroups = new GenericObservableList<GeoGroup>(GeographicGroups));

		public virtual string Title => $"{CarModel?.Name} ({RegistrationNumber})";
		public virtual string FullTitle => $"{CarModel?.Title} ({RegistrationNumber})";

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

		public virtual FuelCardVersion GetCurrentActiveFuelCardVersion()
		{
			return GetActiveFuelCardVersionOnDate(DateTime.Now);
		}

		public virtual FuelCardVersion GetActiveFuelCardVersionOnDate(DateTime date)
		{
			return ObservableFuelCardVersions.FirstOrDefault(x =>
				x.StartDate <= date && (x.EndDate == null || x.EndDate >= date));
		}

		public static CarTypeOfUse[] GetCarTypesOfUseForRatesLevelWageCalculation() => new[] { CarTypeOfUse.Largus, CarTypeOfUse.Minivan, CarTypeOfUse.GAZelle };

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var currentPermissionService = validationContext.GetRequiredService<ICurrentPermissionService>();
			var canChangeCompositionCompanyTransportPark =
				currentPermissionService.ValidatePresetPermission(CarPermissions.CanChangeCompositionCompanyTransportPark);
			
			if(string.IsNullOrWhiteSpace(RegistrationNumber))
			{
				yield return new ValidationResult("Гос. номер автомобиля должен быть заполнен", new[] { nameof(RegistrationNumber) });
			}

			if(FuelType is null)
			{
				yield return new ValidationResult("Тип топлива должен быть заполнен", new[] { nameof(FuelType) });
			}

			if(CarModel is null)
			{
				yield return new ValidationResult("Модель должна быть заполнена", new[] { nameof(CarModel) });
			}

			if(FuelConsumption <= 0)
			{
				yield return new ValidationResult("Расход топлива должен быть больше 0", new[] { nameof(FuelConsumption) });
			}

			if(IncomeChannel == IncomeChannel.None)
			{
				yield return new ValidationResult("Должен быть указан канал поступления", new[] { nameof(IncomeChannel) });
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

			if(!canChangeCompositionCompanyTransportPark)
			{
				var activeVersion = GetActiveCarVersionOnDate();

				if(Id == 0)
				{
					if(activeVersion != null && (activeVersion.IsCompanyCar || activeVersion.IsRaskat))
					{
						yield return new ValidationResult(
							"Невозможно сохранить авто в выбранной принадлежности. У Вас нет права менять состав автопарка компании",
							new[] { nameof(CarVersions) });
					}
				}
				else
				{
					using(var uow = validationContext.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Получение данных авто из БД"))
					{
						var carDataFromBase = uow.GetById<Car>(Id);
						var activeVersionFromBase = carDataFromBase.GetActiveCarVersionOnDate();

						if(carDataFromBase.IsArchive != IsArchive && (activeVersion.IsCompanyCar || activeVersion.IsRaskat))
						{
							yield return new ValidationResult(
								"Невозможно поменять архивность авто. У Вас нет права менять состав автопарка компании",
								new[] { nameof(CarVersions) });
						}
						
						bool error = false;

						if(activeVersionFromBase != null)
						{
							switch(activeVersionFromBase.CarOwnType)
							{
								case CarOwnType.Company:
									if(activeVersion != null && activeVersion.CarOwnType != CarOwnType.Company)
									{
										error =  true;
									}
									break;
								case CarOwnType.Raskat:
									if(activeVersion != null && activeVersion.CarOwnType != CarOwnType.Raskat)
									{
										error =  true;
									}
									break;
								case CarOwnType.Driver:
									if(activeVersion != null && (activeVersion.IsCompanyCar || activeVersion.IsRaskat))
									{
										error =  true;
									}
									break;
							}
						}
						
						if(error)
						{
							yield return new ValidationResult(
								"Невозможно сохранить авто в выбранной принадлежности. У Вас нет права менять состав автопарка компании",
								new[] { nameof(CarVersions) });
						}
					}
				}
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

			if(IsArchive && ArchivingReason == null)
			{
				yield return new ValidationResult("Выберите причину архивирования", new[] { nameof(ArchivingReason) });
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
}
