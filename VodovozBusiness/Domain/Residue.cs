using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "остатки",
		Nominative = "остаток")]
	public class Residue : PropertyChangedBase, IDomainObject//, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		Counterparty customer;

		[Display (Name = "Контрагент")]
		[Required(ErrorMessage = "Контрагент должен быть указан.")]
		public virtual Counterparty Customer {
			get { return customer; }
			set { SetField (ref customer, value, () => Customer); }
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки")]
		[Required (ErrorMessage = "Точка доставки должна быть указана.")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint);}
		}
			
		private Employee author;

		[Display (Name = "Автор")]
		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		DateTime lastEditTime;

		[Display (Name = "Время последнего редактирования")]
		public virtual DateTime LastEditTime{
			get { return lastEditTime; }
			set { SetField(ref lastEditTime, value, () => LastEditTime); }
		}

		Employee lastEditAuthor;

		[Display (Name = "Автор последнего редактирования")]
		public virtual Employee LastEditAuthor {
			get { return lastEditAuthor; }
			set { SetField (ref lastEditAuthor, value, () => LastEditAuthor); }
		}

		int? bottlesResidue;

		[Display (Name = "Остаток по таре")]
		public virtual int? BottlesResidue {
			get { return bottlesResidue; }
			set { SetField (ref bottlesResidue, value, () => BottlesResidue); }
		}

		BottlesMovementOperation bottlesMovementOperation;

		[Display (Name = "Передвижение бутылей")]
		public virtual BottlesMovementOperation BottlesMovementOperation {
			get { return bottlesMovementOperation; }
			set { SetField (ref bottlesMovementOperation, value, () => BottlesMovementOperation); }
		}

		decimal? depositResidueBottels;

		[Display (Name = "Залог за тару")]
		public virtual decimal? DepositResidueBottels{
			get{ return depositResidueBottels; }
			set { SetField(ref depositResidueBottels, value, () => DepositResidueBottels); }
		}

		decimal? depositResidueEquipment;


		[Display (Name = "Залог за оборудование")]
		public virtual decimal? DepositResidueEquipment{
			get{ return depositResidueEquipment; }
			set { SetField(ref depositResidueEquipment, value, () => DepositResidueEquipment); }
		}

		DepositOperation depositBottlesOperation;

		[Display (Name = "Операция по залогу за тару")]
		public virtual DepositOperation DepositBottlesOperation{
			get { return depositBottlesOperation; }
			set { SetField(ref depositBottlesOperation, value, () => DepositBottlesOperation); }
		}

		DepositOperation depositEquipmentOperation;

		[Display (Name = "Операция по залогу за оборудование")]
		public virtual DepositOperation DepositEquipmentOperation{
			get { return depositEquipmentOperation; }
			set {SetField(ref depositEquipmentOperation, value, () => DepositEquipmentOperation); }
		}

		decimal? debtResidue;

		[Display (Name = "Долг по деньгам")]
		public virtual decimal? DebtResidue{
			get { return debtResidue; }
			set { SetField(ref debtResidue, value, () => DebtResidue); }
		}

		MoneyMovementOperation moneyMovementOperation;

		[Display (Name = "Операция по передвижение денег")]
		public virtual MoneyMovementOperation MoneyMovementOperation{
			get { return moneyMovementOperation; }
			set { SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		private PaymentType debtPaymentType;

		[Display(Name = "Тип долга")]
		public virtual PaymentType DebtPaymentType
		{
			get { return debtPaymentType; }
			set { SetField(ref debtPaymentType, value, () => DebtPaymentType); }
		}

		#endregion

		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			//Обновляем операции по бутылям.
			if (BottlesResidue == null)
				BottlesMovementOperation = DeleteOperation(uow, BottlesMovementOperation);
			else
			{
				if(BottlesMovementOperation == null)
					BottlesMovementOperation = new BottlesMovementOperation();

				BottlesMovementOperation.Counterparty = customer;
				BottlesMovementOperation.OperationTime = Date;
				BottlesMovementOperation.DeliveryPoint = DeliveryPoint;
				int bottleDebt;
				if(DeliveryPoint == null)
					bottleDebt = Repository.Operations.BottlesRepository.GetBottlesAtCounterparty(uow, Customer, Date);
				else
					bottleDebt = Repository.Operations.BottlesRepository.GetBottlesAtDeliveryPoint(uow, DeliveryPoint, Date);

				var needCorrect = BottlesResidue.Value - bottleDebt;

				if (needCorrect > 0)
				{
					BottlesMovementOperation.Delivered = needCorrect;
					BottlesMovementOperation.Returned = 0;
				}
				else
				{
					BottlesMovementOperation.Returned = Math.Abs(needCorrect);
					BottlesMovementOperation.Delivered = 0;
				}
			}

			//Обновляем операции по залогам за бутыли.
			if (DepositResidueBottels == null)
				DepositBottlesOperation = DeleteOperation(uow, DepositBottlesOperation);
			else
			{
				if(DepositBottlesOperation == null)
					DepositBottlesOperation = new DepositOperation();

				DepositBottlesOperation.Counterparty = customer;
				DepositBottlesOperation.OperationTime = Date;
				DepositBottlesOperation.DeliveryPoint = DeliveryPoint;
				DepositBottlesOperation.DepositType = DepositType.Bottles;
				decimal bottleDeposit;
				if(DeliveryPoint == null)
					bottleDeposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(uow, Customer, DepositType.Bottles, Date);
				else
					bottleDeposit = Repository.Operations.DepositRepository.GetDepositsAtDeliveryPoint(uow, DeliveryPoint, DepositType.Bottles, Date);

				var needCorrect = DepositResidueBottels.Value - bottleDeposit;

				if (needCorrect > 0)
				{
					DepositBottlesOperation.ReceivedDeposit = needCorrect;
					DepositBottlesOperation.RefundDeposit = 0;
				}
				else
				{
					DepositBottlesOperation.RefundDeposit = Math.Abs(needCorrect);
					DepositBottlesOperation.ReceivedDeposit = 0;
				}
			}

			//Обновляем операции по залогам за бутыли.
			if (DepositResidueEquipment == null)
				DepositEquipmentOperation = DeleteOperation(uow, DepositEquipmentOperation);
			else
			{
				if(DepositEquipmentOperation == null)
					DepositEquipmentOperation = new DepositOperation();

				DepositEquipmentOperation.Counterparty = customer;
				DepositEquipmentOperation.OperationTime = Date;
				DepositEquipmentOperation.DeliveryPoint = DeliveryPoint;
				DepositEquipmentOperation.DepositType = DepositType.Equipment;
				decimal equipmentDeposit;
				if(DeliveryPoint == null)
					equipmentDeposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(uow, Customer, DepositType.Equipment, Date);
				else
					equipmentDeposit = Repository.Operations.DepositRepository.GetDepositsAtDeliveryPoint(uow, DeliveryPoint, DepositType.Equipment, Date);

				var needCorrect = DepositResidueEquipment.Value - equipmentDeposit;

				if (needCorrect > 0)
				{
					DepositEquipmentOperation.ReceivedDeposit = needCorrect;
					DepositEquipmentOperation.RefundDeposit = 0;
				}
				else
				{
					DepositEquipmentOperation.RefundDeposit = Math.Abs(needCorrect);
					DepositEquipmentOperation.ReceivedDeposit = 0;
				}
			}

			//Обновляем операции по деньгам.
			if (DebtResidue == null)
				MoneyMovementOperation = DeleteOperation(uow, MoneyMovementOperation);
			else
			{
				if(MoneyMovementOperation == null)
					MoneyMovementOperation = new MoneyMovementOperation();

				MoneyMovementOperation.Counterparty  = customer;
				MoneyMovementOperation.OperationTime = Date;
				MoneyMovementOperation.PaymentType 	 = DebtPaymentType;

				decimal debt = Repository.Operations.MoneyRepository.GetCounterpartyDebt(uow, Customer, Date);

				var needCorrect = DebtResidue.Value - debt;
				MoneyMovementOperation.Debt = needCorrect;
			}
		}

		private TOperation DeleteOperation<TOperation>(IUnitOfWork uow, TOperation op) where TOperation : OperationBase
		{
			if (op != null && op.Id > 0)
				uow.Delete(op);
			return null;
		}

		#endregion

	}
}

