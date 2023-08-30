using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы ввода остатков",
		Nominative = "документа ввода остатков")]
	[EntityPermission]
	public class Residue : PropertyChangedBase, IDomainObject, IValidatableObject
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
		public virtual Counterparty Customer {
			get { return customer; }
			set {
				if(SetField(ref customer, value, () => Customer) && customer == null) {
					DeliveryPoint = null;
				}
			}
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
		public virtual decimal? BottlesDeposit{
			get{ return depositResidueBottels; }
			set { SetField(ref depositResidueBottels, value, () => BottlesDeposit); }
		}

		DepositOperation depositBottlesOperation;

		[Display (Name = "Операция по залогу за тару")]
		public virtual DepositOperation BottlesDepositOperation{
			get { return depositBottlesOperation; }
			set { SetField(ref depositBottlesOperation, value, () => BottlesDepositOperation); }
		}

		DepositOperation depositEquipmentOperation;

		[Display (Name = "Операция по залогу за оборудование")]
		public virtual DepositOperation EquipmentDepositOperation{
			get { return depositEquipmentOperation; }
			set {SetField(ref depositEquipmentOperation, value, () => EquipmentDepositOperation); }
		}

		decimal? debtResidue;

		[Display (Name = "Долг по деньгам")]
		public virtual decimal? DebtResidue{
			get { return debtResidue; }
			set { SetField(ref debtResidue, value, () => DebtResidue); }
		}

		MoneyMovementOperation moneyMovementOperation;

		[Display (Name = "Операция по передвижению денег")]
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

		IList<ResidueEquipmentDepositItem> equipmentDepositItems = new List<ResidueEquipmentDepositItem>();

		[Display(Name = "Залоги за оборудование")]
		public virtual IList<ResidueEquipmentDepositItem> EquipmentDepositItems {
			get => equipmentDepositItems;
			set => SetField(ref equipmentDepositItems, value, () => EquipmentDepositItems);
		}

		GenericObservableList<ResidueEquipmentDepositItem> observableEquipmentDepositItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ResidueEquipmentDepositItem> ObservableEquipmentDepositItems {
			get {
				if(observableEquipmentDepositItems == null) {
					observableEquipmentDepositItems = new GenericObservableList<ResidueEquipmentDepositItem>(equipmentDepositItems);
				}
				return observableEquipmentDepositItems;
			}
		}

		#endregion

		#region Функции

		public virtual void AddEquipmentDepositItem(Nomenclature nomenclature)
		{
			var newItem = new ResidueEquipmentDepositItem();
			newItem.Residue = this;
			newItem.Nomenclature = nomenclature;
			newItem.EquipmentCount = 1;
			newItem.DepositCount = 1;
			newItem.PaymentType = PaymentType.Cash;
			ObservableEquipmentDepositItems.Add(newItem);
		}

		public virtual void RemoveEquipmentDepositItem(ResidueEquipmentDepositItem item)
		{
			if(ObservableEquipmentDepositItems.Contains(item)) {
				ObservableEquipmentDepositItems.Remove(item);
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow, IBottlesRepository bottlesRepository, IMoneyRepository moneyRepository, IDepositRepository depositRepository, IValidator validator)
		{
			if(uow == null) {
				throw new ArgumentNullException(nameof(uow));
			}

			if(bottlesRepository == null) {
				throw new ArgumentNullException(nameof(bottlesRepository));
			}

			if(moneyRepository == null) {
				throw new ArgumentNullException(nameof(moneyRepository));
			}

			if(depositRepository == null) {
				throw new ArgumentNullException(nameof(depositRepository));
			}

			if(validator == null) {
				throw new ArgumentNullException(nameof(validator));
			}

			//var validator = validationService.GetValidator();
			if(!validator.Validate(this)) {
				return;
			}

			UpdateBottlesOperation(uow, bottlesRepository);
			UpdateMoneyOperation(uow, moneyRepository);
			UpdateBottlesDepositOperation(uow, depositRepository);
			UpdateEquipmentDepositOperation(uow, depositRepository);
		}

		private void UpdateBottlesDepositOperation(IUnitOfWork uow, IDepositRepository depositRepository)
		{
			//Обновляем операции по залогам за бутыли.
			if(BottlesDeposit == null)
				BottlesDepositOperation = null;
			else {
				if(BottlesDepositOperation == null)
					BottlesDepositOperation = new DepositOperation();

				BottlesDepositOperation.Counterparty = customer;
				BottlesDepositOperation.OperationTime = Date;
				BottlesDepositOperation.DeliveryPoint = DeliveryPoint;
				BottlesDepositOperation.DepositType = DepositType.Bottles;
				decimal bottleDeposit;
				if(DeliveryPoint == null)
					bottleDeposit = depositRepository.GetDepositsAtCounterparty(uow, Customer, DepositType.Bottles, Date);
				else
					bottleDeposit = depositRepository.GetDepositsAtDeliveryPoint(uow, DeliveryPoint, DepositType.Bottles, Date);

				var needCorrect = BottlesDeposit.Value - bottleDeposit;

				if(needCorrect > 0) {
					BottlesDepositOperation.ReceivedDeposit = needCorrect;
					BottlesDepositOperation.RefundDeposit = 0;
				} else {
					BottlesDepositOperation.RefundDeposit = Math.Abs(needCorrect);
					BottlesDepositOperation.ReceivedDeposit = 0;
				}
			}
		}

		private void UpdateMoneyOperation(IUnitOfWork uow, IMoneyRepository moneyRepository)
		{
			//Обновляем операции по деньгам.
			if(DebtResidue == null)
				MoneyMovementOperation = null;
			else {
				if(MoneyMovementOperation == null)
					MoneyMovementOperation = new MoneyMovementOperation();

				MoneyMovementOperation.Counterparty = customer;
				MoneyMovementOperation.OperationTime = Date;
				MoneyMovementOperation.PaymentType = DebtPaymentType;

				decimal debt = moneyRepository.GetCounterpartyDebt(uow, Customer, Date);

				var needCorrect = DebtResidue.Value - debt;
				MoneyMovementOperation.Debt = needCorrect;
			}
		}

		private void UpdateBottlesOperation(IUnitOfWork uow, IBottlesRepository bottlesRepository)
		{
			//Обновляем операции по бутылям.
			if(BottlesResidue == null)
				BottlesMovementOperation = null;
			else {
				if(BottlesMovementOperation == null)
					BottlesMovementOperation = new BottlesMovementOperation();

				BottlesMovementOperation.Counterparty = customer;
				BottlesMovementOperation.OperationTime = Date;
				BottlesMovementOperation.DeliveryPoint = DeliveryPoint;
				int bottleDebt;
				
				bottleDebt = DeliveryPoint == null
					? bottlesRepository.GetBottlesDebtAtCounterparty(uow, Customer, Date)
					: bottlesRepository.GetBottlesDebtAtDeliveryPoint(uow, DeliveryPoint, Date);

				var needCorrect = BottlesResidue.Value - bottleDebt;

				if(needCorrect > 0) {
					BottlesMovementOperation.Delivered = needCorrect;
					BottlesMovementOperation.Returned = 0;
				} else {
					BottlesMovementOperation.Returned = Math.Abs(needCorrect);
					BottlesMovementOperation.Delivered = 0;
				}
			}
		}

		private void UpdateEquipmentDepositOperation(IUnitOfWork uow, IDepositRepository depositRepository)
		{
			decimal equipmentDeposit = EquipmentDepositItems.Sum(x => x.EquipmentDeposit * x.EquipmentCount);

			//Обновляем операции по залогам за оборудование.
			if(equipmentDeposit == 0)
				EquipmentDepositOperation = null;
			else {
				if(EquipmentDepositOperation == null)
					EquipmentDepositOperation = new DepositOperation();

				EquipmentDepositOperation.Counterparty = customer;
				EquipmentDepositOperation.OperationTime = Date;
				EquipmentDepositOperation.DeliveryPoint = DeliveryPoint;
				EquipmentDepositOperation.DepositType = DepositType.Equipment;
				decimal equipmentDepositExists;
				if(DeliveryPoint == null)
					equipmentDepositExists = depositRepository.GetDepositsAtCounterparty(uow, Customer, DepositType.Equipment, Date);
				else
					equipmentDepositExists = depositRepository.GetDepositsAtDeliveryPoint(uow, DeliveryPoint, DepositType.Equipment, Date);

				var needCorrect = equipmentDeposit - equipmentDepositExists;

				if(needCorrect > 0) {
					EquipmentDepositOperation.ReceivedDeposit = needCorrect;
					EquipmentDepositOperation.RefundDeposit = 0;
				} else {
					EquipmentDepositOperation.RefundDeposit = Math.Abs(needCorrect);
					EquipmentDepositOperation.ReceivedDeposit = 0;
				}
			}

			foreach(var item in EquipmentDepositItems) {
				item.UpdateOperation();
			}
		}

		#endregion

		#region IValidableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Customer == null) 
				yield return new ValidationResult(
					"Должен быть заполнен контрагент",
					new[] { this.GetPropertyName(o => o.Customer) }
				);
		}

		#endregion IValidableObject implementation
	}
}
