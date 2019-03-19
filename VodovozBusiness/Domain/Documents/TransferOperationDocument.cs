using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "переносы",
		Nominative = "перенос")]
	[EntityPermission]
	[HistoryTrace]
	public class TransferOperationDocument : Document, IValidatableObject, IDomainObject
	{
		[Display(Name = "Дата")]
		public override DateTime TimeStamp {
			get => base.TimeStamp;
			set {
				base.TimeStamp = value;

				if(this.OutBottlesOperation != null) {
					this.OutBottlesOperation.OperationTime = TimeStamp;
				}
				if(this.IncBottlesOperation != null) {
					this.IncBottlesOperation.OperationTime = TimeStamp;
				}

				if(this.OutBottlesDepositOperation != null) {
					this.OutBottlesDepositOperation.OperationTime = TimeStamp;
				}
				if(this.IncBottlesDepositOperation != null) {
					this.IncBottlesDepositOperation.OperationTime = TimeStamp;
				}
				if(this.OutEquipmentDepositOperation != null) {
					this.OutEquipmentDepositOperation.OperationTime = TimeStamp;
				}
				if(this.IncEquipmentDepositOperation != null) {
					this.IncEquipmentDepositOperation.OperationTime = TimeStamp;
				}
			}
		}

		BottlesMovementOperation outBottlesOperation;

		public virtual BottlesMovementOperation OutBottlesOperation {
			get { return outBottlesOperation; }
			set { SetField(ref outBottlesOperation, value, () => OutBottlesOperation);}
		}

		BottlesMovementOperation incBottlesOperation;

		public virtual BottlesMovementOperation IncBottlesOperation {
			get { return incBottlesOperation; }
			set { SetField(ref incBottlesOperation, value, () => IncBottlesOperation); }
		}

		DepositOperation outBottlesDepositOperation;

		public virtual DepositOperation OutBottlesDepositOperation {
			get { return outBottlesDepositOperation; }
			set { SetField(ref outBottlesDepositOperation, value, () => OutBottlesDepositOperation); }
		}

		DepositOperation incBottlesDepositOperation;

		public virtual DepositOperation IncBottlesDepositOperation {
			get { return incBottlesDepositOperation; }
			set { SetField(ref incBottlesDepositOperation, value, () => IncBottlesDepositOperation); }
		}

		DepositOperation outEquipmentDepositOperation;

		public virtual DepositOperation OutEquipmentDepositOperation {
			get { return outEquipmentDepositOperation; }
			set { SetField(ref outEquipmentDepositOperation, value, () => OutEquipmentDepositOperation); }
		}

		DepositOperation incEquipmentDepositOperation;

		public virtual DepositOperation IncEquipmentDepositOperation {
			get { return incEquipmentDepositOperation; }
			set { SetField(ref incEquipmentDepositOperation, value, () => IncEquipmentDepositOperation); }
		}

		Counterparty fromClient;

		[Display(Name = "Контрагент-отправитель")]
		[Required(ErrorMessage = "Контрагент-отправитель должен быть указан.")]
		public virtual Counterparty FromClient {
			get { return fromClient; }
			set { SetField(ref fromClient, value, () => FromClient); }
		}

		Counterparty toClient;

		[Display(Name = "Контрагент-получатель")]
		[Required(ErrorMessage = "Контрагент-получатель должен быть указан.")]
		public virtual Counterparty ToClient {
			get { return toClient; }
			set { SetField(ref toClient, value, () => ToClient); }
		}

		DeliveryPoint fromDeliveryPoint;

		[Display(Name = "Точка доставки отправителя")]
		[Required(ErrorMessage = "Точка доставки отправителя должна быть указана.")]
		public virtual DeliveryPoint FromDeliveryPoint {
			get { return fromDeliveryPoint; }
			set { SetField(ref fromDeliveryPoint, value, () => FromDeliveryPoint); }
		}

		DeliveryPoint toDeliveryPoint;

		[Display(Name = "Точка доставки получателя")]
		[Required(ErrorMessage = "Точка доставки получателя должна быть указана.")]
		public virtual DeliveryPoint ToDeliveryPoint {
			get { return toDeliveryPoint; }
			set { SetField(ref toDeliveryPoint, value, () => ToDeliveryPoint); }
		}

		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		Employee responsibleEmployee;

		public virtual Employee ResponsiblePerson {
			get { return responsibleEmployee; }
			set { SetField(ref responsibleEmployee, value, () => ResponsiblePerson); }
		}

		public virtual string[] SaveOperations(IUnitOfWork UoW, int bottles, decimal bottlesDeposits, decimal equipmentDeposits)
		{
			var messages = new List<string>();

			BottlesMovementOperation outBottles = this.OutBottlesOperation ?? new BottlesMovementOperation(),
										incBottles = this.IncBottlesOperation ?? new BottlesMovementOperation();
			DepositOperation outBottlesDeposits = this.OutBottlesDepositOperation ?? new DepositOperation(),
								incBottlesDeposits = this.IncBottlesDepositOperation ?? new DepositOperation(),
								outEquipmentDeposits = this.OutEquipmentDepositOperation ?? new DepositOperation(),
								incEquipmentDeposits = this.IncEquipmentDepositOperation ?? new DepositOperation();

			SaveBottlesOperations(UoW, ref outBottles, ref incBottles, bottles);
			SaveDepositOperations(UoW, ref outBottlesDeposits, ref incBottlesDeposits, bottlesDeposits, DepositType.Bottles);
			SaveDepositOperations(UoW, ref outEquipmentDeposits, ref incEquipmentDeposits, equipmentDeposits, DepositType.Equipment);

			if(outBottles == null && incBottles == null)
			{
				if(this.OutBottlesOperation != null)
				{
					UoW.Delete(this.OutBottlesOperation);
					this.OutBottlesOperation = null;
				}

				if(this.IncBottlesOperation != null) {
					UoW.Delete(this.IncBottlesOperation);
					this.IncBottlesOperation = null;
				}
			} else
			{
				this.OutBottlesOperation = outBottles;
				this.IncBottlesOperation = incBottles;
			}

			if(outBottlesDeposits == null && incBottlesDeposits == null) {
				if(this.OutBottlesDepositOperation != null) {
					UoW.Delete(this.OutBottlesDepositOperation);
					this.OutBottlesDepositOperation = null;
				}

				if(this.IncBottlesDepositOperation != null) {
					UoW.Delete(this.IncBottlesDepositOperation);
					this.IncBottlesDepositOperation = null;
				}
			} else {
				this.OutBottlesDepositOperation = outBottlesDeposits;
				this.IncBottlesDepositOperation = incBottlesDeposits;
			}

			if(outEquipmentDeposits == null && incEquipmentDeposits == null) {
				if(this.OutEquipmentDepositOperation != null) {
					UoW.Delete(this.OutEquipmentDepositOperation);
					this.OutEquipmentDepositOperation = null;
				}

				if(this.IncEquipmentDepositOperation != null) {
					UoW.Delete(this.IncEquipmentDepositOperation);
					this.IncEquipmentDepositOperation = null;
				}
			} else {
				this.OutEquipmentDepositOperation = outEquipmentDeposits;
				this.IncEquipmentDepositOperation = incEquipmentDeposits;
			}

			UoW.Save();
			return messages.ToArray();
		}

		protected void SaveBottlesOperations(IUnitOfWork UoW, ref BottlesMovementOperation outBottles, ref BottlesMovementOperation incBottles, int bottles)
		{
			if(bottles == 0) {
				outBottles = null;
				incBottles = null;
				return;
			}

			outBottles.Counterparty = this.FromClient;
			outBottles.DeliveryPoint = this.FromDeliveryPoint;
			outBottles.OperationTime = this.TimeStamp;

			incBottles.Counterparty = this.ToClient;
			incBottles.DeliveryPoint = this.ToDeliveryPoint;
			incBottles.OperationTime = this.TimeStamp;

			if(bottles > 0) {
				outBottles.Returned = incBottles.Delivered = bottles;
				outBottles.Delivered = incBottles.Returned = 0;
			} else if(bottles < 0) {
				outBottles.Delivered = incBottles.Returned = bottles * -1;
				outBottles.Returned = incBottles.Delivered = 0;
			}
		}

		protected void SaveDepositOperations(IUnitOfWork UoW, ref DepositOperation outDeposits, ref DepositOperation incDeposits, decimal deposits, DepositType depositType)
		{
			if(deposits == 0) {
				outDeposits = null;
				incDeposits = null;
				return;
			}

			outDeposits.Counterparty = this.FromClient;
			outDeposits.DeliveryPoint = this.FromDeliveryPoint;
			outDeposits.DepositType = depositType;
			outDeposits.OperationTime = this.TimeStamp;


			incDeposits.Counterparty = this.ToClient;
			incDeposits.DeliveryPoint = this.ToDeliveryPoint;
			incDeposits.DepositType = depositType;
			incDeposits.OperationTime = this.TimeStamp;

			if(deposits > 0) {
				outDeposits.RefundDeposit = incDeposits.ReceivedDeposit = deposits;
				outDeposits.ReceivedDeposit = incDeposits.RefundDeposit = 0;
			} else if(deposits < 0) {
				outDeposits.ReceivedDeposit = incDeposits.RefundDeposit = deposits * -1;
				outDeposits.RefundDeposit = incDeposits.ReceivedDeposit = 0;
			}
		}

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FromClient == null)
				yield return new ValidationResult(String.Format("Клиент-отправитель не указан."),
				                                  new[] { this.GetPropertyName(o => o.FromClient) });

			if(FromDeliveryPoint == null)
				yield return new ValidationResult(String.Format("Точка доставки отправителя не указана."),
												  new[] { this.GetPropertyName(o => o.FromDeliveryPoint) });

			if(ToClient == null)
				yield return new ValidationResult(String.Format("Клиент-получатель не указан."),
												  new[] { this.GetPropertyName(o => o.ToClient) });

			if(ToDeliveryPoint == null)
				yield return new ValidationResult(String.Format("Точка доставки получателя не указана."),
												  new[] { this.GetPropertyName(o => o.ToDeliveryPoint) });
			
			if(FromDeliveryPoint == ToDeliveryPoint)
				yield return new ValidationResult(String.Format("Точки доставки отправителя и получателя одинаковые."),
												  new[] { this.GetPropertyName(o => o.FromDeliveryPoint), this.GetPropertyName(o => o.ToDeliveryPoint) });

		/*	if(OutBottlesOperation == null
			   && OutBottlesDepositOperation == null
			   && OutEquipmentDepositOperation == null)
				yield return new ValidationResult(String.Format("Вы ничего не указали для перемещения."),
												  new[] {this.GetPropertyName(o => o.OutBottlesOperation),
														this.GetPropertyName(o => o.OutBottlesDepositOperation),
														this.GetPropertyName(o => o.OutEquipmentDepositOperation)}); */
		}
	}
}
