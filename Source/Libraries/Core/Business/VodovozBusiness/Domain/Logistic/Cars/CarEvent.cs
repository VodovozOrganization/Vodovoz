using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Logistics;
using VodovozBusiness.Domain.Client.Specifications;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "события ТС",
		Nominative = "событие ТС")]
	[EntityPermission]
	[HistoryTrace]

	public class CarEvent : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _createDate;
		private Employee _author;
		private CarEventType _carEventType;
		private Car _car;
		private Employee _driver;
		private DateTime _startDate;
		private DateTime _endDate;
		private string _comment;
		private string _foundation;
		private bool _doNotShowInOperation;
		private bool _compensationFromInsuranceByCourt;
		private decimal _repairCost;
		private CarEvent _originalCarEvent;
		private int _odometer;
		private DateTime? _carTechnicalCheckupEndingDate;
		private WriteOffDocument _writeOffDocument;
		private bool _isWriteOffDocumentNotRequired;
		private decimal? _actualFuelBalance;
		private decimal? _currentFuelBalance;
		private decimal? _substractionFuelBalance;
		private decimal? _fuelCost;
		private FuelOperation _calibrationFuelOperation;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Вид события ТС")]
		public virtual CarEventType CarEventType
		{
			get => _carEventType;
			set => SetField(ref _carEventType, value);
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Дата начала события ТС")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания события ТС")]
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Основание")]
		public virtual string Foundation
		{
			get => _foundation;
			set => SetField(ref _foundation, value);
		}

		[Display(Name = "Не отражать в эксплуатации ТС")]
		public virtual bool DoNotShowInOperation
		{
			get => _doNotShowInOperation;
			set => SetField(ref _doNotShowInOperation, value);
		}

		[Display(Name = "Компенсация от страховой, по суду")]
		public virtual bool CompensationFromInsuranceByCourt
		{
			get => _compensationFromInsuranceByCourt;
			set => SetField(ref _compensationFromInsuranceByCourt, value);
		}

		[Display(Name = "Стоимость работ")]
		[PropertyChangedAlso(nameof(RepairAndPartsSummaryCost))]
		public virtual decimal RepairCost
		{
			get => _repairCost;
			set => SetField(ref _repairCost, value);
		}

		[Display(Name = "Исходное ремонтное событие")]
		public virtual CarEvent OriginalCarEvent
		{
			get => _originalCarEvent;
			set => SetField(ref _originalCarEvent, value);
		}

		IList<Fine> fines = new List<Fine>();
		[Display(Name = "Штрафы")]
		public virtual IList<Fine> Fines
		{
			get => fines;
			set => SetField(ref fines, value, () => Fines);
		}

		[Display(Name = "Показание одометра")]
		public virtual int Odometer
		{
			get => _odometer;
			set => SetField(ref _odometer, value);
		}

		[Display(Name = "Дата окончания действия техосмотра")]
		public virtual DateTime? CarTechnicalCheckupEndingDate
		{
			get => _carTechnicalCheckupEndingDate;
			set => SetField(ref _carTechnicalCheckupEndingDate, value);
		}

		[Display(Name = "Акт списания ТМЦ")]
		[PropertyChangedAlso(
			nameof(RepairPartsCost),
			nameof(RepairAndPartsSummaryCost))]
		public virtual WriteOffDocument WriteOffDocument
		{
			get => _writeOffDocument;
			set => SetField(ref _writeOffDocument, value);
		}

		[Display(Name = "Акт списания не нужен")]
		public virtual bool IsWriteOffDocumentNotRequired
		{
			get => _isWriteOffDocumentNotRequired;
			set => SetField(ref _isWriteOffDocumentNotRequired, value);
		}

		#region Калибровка баланса топлива

		[Display(Name = "Актуальный баланс топлива")]
		public virtual decimal? ActualFuelBalance
		{
			get => _actualFuelBalance;
			set => SetField(ref _actualFuelBalance, value);
		}

		[Display(Name = "Текущий баланс топлива")]
		public virtual decimal? CurrentFuelBalance
		{
			get => _currentFuelBalance;
			set => SetField(ref _currentFuelBalance, value);
		}

		[Display(Name = "Разность литража")]
		public virtual decimal? SubstractionFuelBalance
		{
			get => _substractionFuelBalance;
			set => SetField(ref _substractionFuelBalance, value);
		}

		[Display(Name = "Стоимость топлива")]
		public virtual decimal? FuelCost
		{
			get => _fuelCost;
			set => SetField(ref _fuelCost, value);
		}

		public virtual FuelOperation CalibrationFuelOperation
		{
			get => _calibrationFuelOperation;
			set => SetField(ref _calibrationFuelOperation, value);
		}

		#endregion Калибровка баланса топлива

		GenericObservableList<Fine> observableFines;

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Fine> ObservableFines
		{
			get
			{
				if(observableFines == null)
					observableFines = new GenericObservableList<Fine>(Fines);
				return observableFines;
			}
		}

		public virtual void AddFine(Fine fine)
		{
			if(ObservableFines.Contains(fine))
			{
				return;
			}
			ObservableFines.Add(fine);
		}

		public virtual string GetFineReason()
		{
			return $"Событие №{Id} от {CreateDate.ToShortDateString()}";
		}

		public virtual decimal RepairPartsCost =>
			WriteOffDocument?.TotalSumOfDamage ?? 0;

		public virtual decimal RepairAndPartsSummaryCost =>
			RepairCost + RepairPartsCost;

		#endregion

		public override string ToString()
		{
			return $"Событие ТС №{Id} {CarEventType.Name}";
		}

		public virtual void UpdateCalibrationFuelOperation()
		{
			if(CalibrationFuelOperation is null)
			{
				CalibrationFuelOperation = new FuelOperation
				{
					Car = Car,
					Fuel = Car?.FuelType,
					Driver = Driver,
					OperationTime = DateTime.Now
				};
			}
			else
			{
				CalibrationFuelOperation.Car = Car;
				CalibrationFuelOperation.Fuel = Car?.FuelType;
				CalibrationFuelOperation.Driver = Driver;
				CalibrationFuelOperation.OperationTime = DateTime.Now;
			}

			if(SubstractionFuelBalance > 0)
			{
				CalibrationFuelOperation.LitersOutlayed = 0;
				CalibrationFuelOperation.LitersGived = SubstractionFuelBalance ?? 0;
			}
			else
			{
				CalibrationFuelOperation.LitersOutlayed = -(SubstractionFuelBalance ?? 0);
				CalibrationFuelOperation.LitersGived = 0;
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var carEventSettings = validationContext.GetRequiredService<ICarEventSettings>();

			if(CarEventType == null)
			{
				yield return new ValidationResult("Вид события ТС должен быть указан.",
					new[] { nameof(CarEventType) });
			}

			if(Car == null)
			{
				yield return new ValidationResult("Автомобиль должен быть указан.",
					new[] { nameof(Car) });
			}

			if(StartDate == default(DateTime))
			{
				yield return new ValidationResult("Дата начала события должна быть указана.",
					new[] { nameof(StartDate) });
			}

			if(EndDate == default(DateTime))
			{
				yield return new ValidationResult("Дата окончания события должна быть указана.",
					new[] { nameof(EndDate) });
			}

			if(StartDate > EndDate)
			{
				yield return new ValidationResult("Дата окончания должна быть больше даты начала.",
					new[] { nameof(StartDate), nameof(EndDate) });
			}

			if(string.IsNullOrEmpty(Foundation))
			{
				yield return new ValidationResult($"Основание должено быть заполнено.",
					new[] { nameof(Comment) });
			}

			if(Foundation?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина основания ({Comment.Length}/255).",
					new[] { nameof(Comment) });
			}

			if(CarEventType != null && CarEventType.NeedComment && string.IsNullOrEmpty(Comment))
			{
				yield return new ValidationResult("Комментарий должен быть заполнен.",
						new[] { nameof(Comment) });
			}

			if(Comment?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина комментария ({Comment.Length}/255).",
					new[] { nameof(Comment) });
			}

			if(CarEventType?.Id == carEventSettings.TechInspectCarEventTypeId && Odometer == 0)
			{
				yield return new ValidationResult($"Заполните показания одометра.",
					new[] { nameof(Odometer) });
			}

			if(CarEventType?.Id == carEventSettings.CarTechnicalCheckupEventTypeId)
			{
				if(!CarTechnicalCheckupEndingDate.HasValue)
				{
					yield return new ValidationResult($"Заполните дату окончания действия техосмотра.",
						new[] { nameof(CarTechnicalCheckupEndingDate) });
				}

				if(CarTechnicalCheckupEndingDate.HasValue && CarTechnicalCheckupEndingDate.Value < StartDate)
				{
					yield return new ValidationResult($"Дата окончания действия техосмотра не должна быть меньше даты начала события.",
						new[] { nameof(CarTechnicalCheckupEndingDate) });
				}
			}

			if(CarEventType?.IsAttachWriteOffDocument == true
				&& WriteOffDocument is null
				&& !IsWriteOffDocumentNotRequired)
			{
				yield return new ValidationResult("Не указана информация о складских запчастях. Пожалуйста, прикрепите акт списания или подтвердите, что запчасти не были списаны по ходу работ.",
					new[] { nameof(WriteOffDocument) });
			}

			if(Car != null && CarEventType?.Id == carEventSettings.FuelBalanceCalibrationCarEventTypeId)
			{
				var uow = validationContext.GetRequiredService<IUnitOfWork>();
				var routeListRepository = validationContext.GetRequiredService<IRouteListRepository>();
				var fuelDocumentRepository = validationContext.GetRequiredService<IGenericRepository<FuelDocument>>();

				var completedOrdersInTodayRouteLists = routeListRepository.GetCompletedOrdersInTodayRouteListsByCarId(uow, Car.Id);
				var todayGivedFuelRouteListIds = fuelDocumentRepository.Get(uow, FuelDocumentSpecifications.CreateForTodayGivedFuelByCarId(Car.Id))
					.Select(x => x.RouteList.Id)
					.Distinct()
					.ToList();

				if(completedOrdersInTodayRouteLists.Any())
				{
					yield return new ValidationResult(
						$"Невозможно откалибровать топливо, так как в маршрутных листах на сегодня есть завершенные заказы: {string.Join(", ", completedOrdersInTodayRouteLists)}",
						new[] { nameof(CarEventType) });
				}

				if(todayGivedFuelRouteListIds.Any())
				{
					yield return new ValidationResult(
						$"Невозможно откалибровать топливо, так как сегодня на автомобиль выдавалось топливо по МЛ: {string.Join(", ", todayGivedFuelRouteListIds)}",
						new[] { nameof(CarEventType) });
				}
			}

			if(CalibrationFuelOperation?.LitersGived > 9999)
			{
				yield return new ValidationResult("Слишком большое кол-во в операции выданного топлива", new[] { nameof(CalibrationFuelOperation.LitersGived) });
			}

			if(CalibrationFuelOperation?.LitersOutlayed > 9999)
			{
				yield return new ValidationResult("Слишком большое кол-во в операции полученного топлива", new[] { nameof(CalibrationFuelOperation.LitersOutlayed) });
			}
		}

		#endregion
	}
}
