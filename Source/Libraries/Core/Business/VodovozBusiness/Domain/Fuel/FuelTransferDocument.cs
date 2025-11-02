using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ перемещения топлива",
		NominativePlural = "документы перемещения топлива")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelTransferDocument : BusinessObjectBase<FuelTransferDocument>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime creationTime;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationTime {
			get => creationTime;
			set => SetField(ref creationTime, value, () => CreationTime);
		}

		private Employee authtor;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => authtor;
			set => SetField(ref authtor, value, () => Author);
		}

		private Car car;
		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get => car;
			set => SetField(ref car, value, () => Car);
		}

		private Employee driver;
		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get => driver;
			set => SetField(ref driver, value, () => Driver);
		}

		private FuelTransferDocumentStatuses status;
		[Display(Name = "Статус")]
		public virtual FuelTransferDocumentStatuses Status {
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		private FuelTransferOperation fuelTransferOperation;
		[Display(Name = "Операция транспортировки топлива")]
		public virtual FuelTransferOperation FuelTransferOperation {
			get => fuelTransferOperation;
			set => SetField(ref fuelTransferOperation, value, () => FuelTransferOperation);
		}

		private FuelType fuelType;
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType {
			get => fuelType;
			set => SetField(ref fuelType, value, () => FuelType);
		}

		private decimal transferedLiters;
		[Display(Name = "Перемещаемое топливо")]
		public virtual decimal TransferedLiters {
			get => transferedLiters;
			set => SetField(ref transferedLiters, value, () => TransferedLiters);
		}

		#region sender information

		private Subdivision cashSubdivisionFrom;
		[Display(Name = "Касса отправитель")]
		public virtual Subdivision CashSubdivisionFrom {
			get => cashSubdivisionFrom;
			set => SetField(ref cashSubdivisionFrom, value, () => CashSubdivisionFrom);
		}

		private FuelExpenseOperation fuelExpenseOperation;
		[Display(Name = "Операция списания топлива")]
		public virtual FuelExpenseOperation FuelExpenseOperation {
			get => fuelExpenseOperation;
			set => SetField(ref fuelExpenseOperation, value, () => FuelExpenseOperation);
		}

		private DateTime? sendTime;
		[Display(Name = "Время отправки")]
		public virtual DateTime? SendTime {
			get => sendTime;
			set => SetField(ref sendTime, value, () => SendTime);
		}

		private Employee cashierSender;
		[Display(Name = "Отправивший кассир")]
		public virtual Employee CashierSender {
			get => cashierSender;
			set => SetField(ref cashierSender, value, () => CashierSender);
		}

		#endregion sender information

		#region receiver information

		private Subdivision cashSubdivisionTo;
		[Display(Name = "Касса получатель")]
		public virtual Subdivision CashSubdivisionTo {
			get => cashSubdivisionTo;
			set => SetField(ref cashSubdivisionTo, value, () => CashSubdivisionTo);
		}

		private FuelIncomeOperation fuelIncomeOperation;
		[Display(Name = "Операция поступления топлива")]
		public virtual FuelIncomeOperation FuelIncomeOperation {
			get => fuelIncomeOperation;
			set => SetField(ref fuelIncomeOperation, value, () => FuelIncomeOperation);
		}

		private DateTime? receiveTime;
		[Display(Name = "Время получения")]
		public virtual DateTime? ReceiveTime {
			get => receiveTime;
			set => SetField(ref receiveTime, value, () => ReceiveTime);
		}

		private Employee cashierReceiver;
		[Display(Name = "Принявший кассир")]
		public virtual Employee CashierReceiver {
			get => cashierReceiver;
			set => SetField(ref cashierReceiver, value, () => CashierReceiver);
		}

		#endregion receiver information

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Driver == null) {
				yield return new ValidationResult("Должен быть заполнен водитель", new[] { nameof(Driver) });
			}
			if(Car == null) {
				yield return new ValidationResult("Должен быть заполнен автомобиль", new[] { nameof(Car) });
			}
			if(CashSubdivisionFrom == null) {
				yield return new ValidationResult("Должна быть заполнена касса из которой переносится топливо", new[] { nameof(CashSubdivisionFrom) });
			} else if(CashSubdivisionFrom.Id == 0) {
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionFrom) });
			}
			if(CashSubdivisionTo == null) {
				yield return new ValidationResult("Должна быть заполнена касса в которую переносится топливо", new[] { nameof(CashSubdivisionTo) });
			} else if(CashSubdivisionTo.Id == 0) {
				yield return new ValidationResult("Должна быть выбрана существующая касса", new[] { nameof(CashSubdivisionTo) });
			}
			if(CashSubdivisionFrom != null && CashSubdivisionTo != null && CashSubdivisionFrom.Id == CashSubdivisionTo.Id) {
				yield return new ValidationResult("Невозможно перенести топливо в ту же самую кассу");
			}
			if(TransferedLiters <= 0) {
				yield return new ValidationResult("Объем перемещаемого топлива должен быть больше нуля");
			}

			if(validationContext.Items.ContainsKey("ForStatus") && (FuelTransferDocumentStatuses)validationContext.Items["ForStatus"] == FuelTransferDocumentStatuses.Sent) {
				if(!(validationContext.GetService(typeof(IFuelRepository)) is IFuelRepository fuelRepository)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IFuelRepository)}");
				}

				if(cashSubdivisionFrom != null && FuelType != null && Status == FuelTransferDocumentStatuses.Sent) {
					decimal balance = fuelRepository.GetFuelBalanceForSubdivision(UoW, CashSubdivisionFrom, FuelType);
					if(transferedLiters > balance) {
						yield return new ValidationResult($"На балансе недостаточно топлива ({FuelType.Name}) для перемещения");
					}
				}
			}
		}

		public virtual void Send(Employee cashier, IFuelRepository fuelRepository)
		{
			if(cashier == null) {
				throw new ArgumentNullException(nameof(cashier));
			}

			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			if(Status != FuelTransferDocumentStatuses.New) {
				throw new InvalidOperationException($"Невозможно отправить документ транспортировки топлива не из статуса {FuelTransferDocumentStatuses.New.GetEnumTitle()}");
			}

			if(FuelTransferOperation != null || FuelExpenseOperation != null) {
				throw new InvalidOperationException($"Топливо уже было отправлено ранее в этом же документе, изменить данные о факте отправки топлива невозможно");
			}

			ValidationContext context = new ValidationContext(this, new Dictionary<object, object>() {
				{"ForStatus", FuelTransferDocumentStatuses.Sent}
			});

			context.InitializeServiceProvider(type =>
			{
				if(type == typeof(IFuelRepository))
				{
					return fuelRepository;
				}

				return null;
			});

			string exceptionMessage = this.RaiseValidationAndGetResult(context);
			if(!string.IsNullOrWhiteSpace(exceptionMessage)) {
				throw new ValidationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try {
				FuelTransferOperation newFuelTransferOperation = new FuelTransferOperation {
					SubdivisionFrom = CashSubdivisionFrom,
					SubdivisionTo = CashSubdivisionTo,
					FuelType = FuelType,
					TransferedLiters = TransferedLiters,
					SendTime = now
				};

				FuelExpenseOperation newFuelExpenseOperation = new FuelExpenseOperation {
					FuelTransferDocument = this,
					FuelType = FuelType,
					FuelLiters = TransferedLiters,
					СreationTime = now,
					RelatedToSubdivision = CashSubdivisionFrom
				};

				Status = FuelTransferDocumentStatuses.Sent;
				CashierSender = cashier;
				SendTime = now;
				FuelTransferOperation = newFuelTransferOperation;
				FuelExpenseOperation = newFuelExpenseOperation;
			} catch(Exception) {
				//восстанавливаем состояние
				Status = FuelTransferDocumentStatuses.New;
				FuelTransferOperation = null;
				FuelExpenseOperation = null;
				throw;
			}
		}

		public virtual void Receive(Employee cashier)
		{
			if(cashier == null) {
				throw new ArgumentNullException(nameof(cashier), $"Не указано кто является кассиром");
			}

			if(FuelTransferOperation == null) {
				throw new InvalidOperationException($"Не было создано операции перемещения топлива, без нее невозможно создать операцию принятия");
			}

			if(Status != FuelTransferDocumentStatuses.Sent) {
				throw new InvalidOperationException($"Невозможно принять документ транспортировки топлива не из статуса \"{FuelTransferDocumentStatuses.Sent.GetEnumTitle()}\"");
			}

			if(FuelIncomeOperation != null || FuelTransferOperation.ReceiveTime.HasValue) {
				throw new InvalidOperationException($"Топливо уже было принято ранее, изменить данные о факте принятия топлива невозможно");
			}

			string exceptionMessage = this.RaiseValidationAndGetResult();
			if(!string.IsNullOrWhiteSpace(exceptionMessage)) {
				throw new ValidationException(exceptionMessage);
			}

			DateTime now = DateTime.Now;

			try {
				FuelIncomeOperation newIncomeOperation = new FuelIncomeOperation {
					FuelTransferDocument = this,
					FuelType = FuelType,
					FuelLiters = TransferedLiters,
					СreationTime = now,
					RelatedToSubdivision = CashSubdivisionTo
				};

				Status = FuelTransferDocumentStatuses.Received;
				CashierReceiver = cashier;
				ReceiveTime = now;
				FuelTransferOperation.ReceiveTime = now;
				FuelIncomeOperation = newIncomeOperation;
			} catch(Exception) {
				//восстанавливаем состояние
				Status = FuelTransferDocumentStatuses.Sent;
				CashierReceiver = null;
				FuelTransferOperation.ReceiveTime = null;
				FuelIncomeOperation = null;
				throw;
			}
		}
	}

	public enum FuelTransferDocumentStatuses
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Отправлен")]
		Sent,
		[Display(Name = "Получен")]
		Received
	}
}
