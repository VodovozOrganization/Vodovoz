using System;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Движение средств по р/сч
	/// </summary>
	[EntityPermission]
	public class BankAccountMovement : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime _endDate;
		private string _accountNumber;
		private Account _account;
		private Bank _bank;
		private IObservableList<BankAccountMovementData>  _bankAccountMovements = new ObservableList<BankAccountMovementData>();

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата начала
		/// </summary>
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		/// <summary>
		/// Дата конца
		/// </summary>
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		/// <summary>
		/// Банк
		/// </summary>
		public virtual Bank Bank
		{
			get => _bank;
			set => SetField(ref _bank, value);
		}

		/// <summary>
		/// Расчетный счет
		/// </summary>
		public virtual Account Account
		{
			get => _account;
			set => SetField(ref _account, value);
		}
		
		/// <summary>
		/// Номер расчетного счета
		/// </summary>
		public virtual string AccountNumber
		{
			get
			{
				if(string.IsNullOrWhiteSpace(_accountNumber) && Account != null)
				{
					_accountNumber = Account.Number;
				}
				
				return _accountNumber;
			}
			set => _accountNumber = value;
		}

		public virtual IObservableList<BankAccountMovementData> BankAccountMovements
		{
			get => _bankAccountMovements;
			set => SetField(ref _bankAccountMovements, value);
		}

		/// <summary>
		/// Добавление данных выписки
		/// </summary>
		/// <param name="amount">Сумма</param>
		/// <param name="dataType">Тип данных <see cref="BankAccountMovementDataType"/></param>
		public virtual void AddData(decimal amount, BankAccountMovementDataType dataType)
		{
			BankAccountMovements.Add(BankAccountMovementData.Create(this, amount, dataType));
		}
	}
}
