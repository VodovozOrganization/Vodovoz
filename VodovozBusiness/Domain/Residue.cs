using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using money = Vodovoz.Repository.Operations.MoneyRepository;
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

		[Display (Name = "Клиент")]
		public virtual Counterparty Customer {
			get { return customer; }
			set { SetField (ref customer, value, () => Customer); }
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки")]
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

		[Display (Name = "операция по залогу за тару")]
		public virtual DepositOperation DepositBottlesOperation{
			get { return depositBottlesOperation; }
			set { SetField(ref depositBottlesOperation, value, () => DepositBottlesOperation); }
		}

		DepositOperation depositEquipmentOperation;

		[Display (Name = "операция по залогу за оборудование")]
		public virtual DepositOperation DepositEquipmentOperation{
			get { return depositEquipmentOperation; }
			set {SetField(ref depositEquipmentOperation, value, () => DepositEquipmentOperation); }
		}

		decimal? moneyResidue;

		[Display (Name = "остаток денег")]
		public virtual decimal? MoneyResidue{
			get { return moneyResidue; }
			set { SetField(ref moneyResidue, value, () => MoneyResidue); }
		}

		MoneyMovementOperation moneyMovementOperation;

		[Display (Name = "передвижение денег")]
		public virtual MoneyMovementOperation MoneyMovementOperation{
			get { return moneyMovementOperation; }
			set { SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation); }
		}
		#endregion

		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			if (MoneyResidue == null)
				MoneyMovementOperation = null;///
			else
			{
				if(MoneyMovementOperation == null)
					MoneyMovementOperation = new MoneyMovementOperation();

				var date = moneyMovementOperation != null ? moneyMovementOperation.OperationTime : Date;
				moneyMovementOperation.Counterparty = customer;
				moneyMovementOperation.OperationTime = date;
				money.CounterpartyDebtQueryResult counterpartyMoney =  money.GetCounterpartyMoney(uow, customer, date);
				var Charged = counterpartyMoney.Charged;
				var Payed = counterpartyMoney.Payed;
				var Deposit = counterpartyMoney.Deposit;
				// Deb = Charged - (Payed - Deposit)
				// Считаем, что дерозит не оплачен
				// И платится из денег в Payed

				var dep = depositBottlesOperation == null ? 0 : depositBottlesOperation;
				dep = depositEquipmentOperation == null ? dep : dep + depositEquipmentOperation;

				var XDeposit = dep - Deposit;
				var XDebt = 
			}
			//be

				
		}

		void CreateOperation(DateTime dt, IUnitOfWork uow)
		{
			



		}

		#endregion

	}
}

