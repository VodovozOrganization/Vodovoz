using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Payments
{
	public class BankAccountMovementData : PropertyChangedBase, IDomainObject
	{
		private decimal _amount;
		private BankAccountMovement _accountMovement;
		private BankAccountMovementDataType _accountMovementDataType;

		public BankAccountMovementData() { }

		protected BankAccountMovementData(
			BankAccountMovement accountMovement,
			decimal amount,
			BankAccountMovementDataType accountMovementDataType)
		{
			_accountMovement = accountMovement;
			_amount = amount;
			_accountMovementDataType = accountMovementDataType;
		}

		public virtual int Id { get; set; }

		public virtual BankAccountMovement AccountMovement
		{
			get => _accountMovement;
			set => SetField(ref _accountMovement, value);
		}

		/// <summary>
		/// Сумма
		/// </summary>
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Тип информации из выписки
		/// </summary>
		public virtual BankAccountMovementDataType AccountMovementDataType
		{
			get => _accountMovementDataType;
			set => SetField(ref _accountMovementDataType, value);
		}

		public static BankAccountMovementData Create(
			BankAccountMovement accountMovement,
			decimal amount,
			BankAccountMovementDataType accountMovementDataType) =>
			new BankAccountMovementData(accountMovement, amount, accountMovementDataType);
	}

	public enum BankAccountMovementDataType
	{
		[Display(Name = "Входящий остаток")]
		InitialBalance,
		[Display(Name = "Исходящий остаток")]
		FinalBalance,
		[Display(Name = "Кредит")]
		TotalReceived,
		[Display(Name = "Дебет")]
		TotalWrittenOff
	}
}
