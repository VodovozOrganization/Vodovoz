using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain
{
	/// <summary>
	/// Информация о связи юридического лица и физического, через почту первого,
	/// для возможности заказа пользователем физиком под указанным юриком в ИПЗ
	/// </summary>
	[HistoryTrace]
	public class LinkedLegalCounterpartyEmailToExternalUser : PropertyChangedBase, IDomainObject
	{
		private int _legalCounterpartyId;
		private int _legalCounterpartyEmailId;
		private int _externalCounterpartyId;
		private string _blockingReason;
		private string _accountPassword;
		private ConnectedCustomerConnectState _connectedCustomerConnectState;

		protected LinkedLegalCounterpartyEmailToExternalUser() { }
		
		private LinkedLegalCounterpartyEmailToExternalUser(
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			int externalCounterpartyId,
			string accountPassword)
		{
			LegalCounterpartyId = legalCounterpartyId;
			LegalCounterpartyEmailId = legalCounterpartyEmailId;
			ExternalCounterpartyId = externalCounterpartyId;
			AccountPassword =  accountPassword;
			ConnectState = ConnectedCustomerConnectState.Active;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор юридического лица, от которого сможет заказывать в ИПЗ физик
		/// </summary>
		[Display(Name = "Идентификатор юридического лица, к которому привязан физик")]
		public virtual int LegalCounterpartyId
		{
			get => _legalCounterpartyId;
			set => SetField(ref _legalCounterpartyId, value);
		}
		
		/// <summary>
		/// Идентификатор внешнего пользователя
		/// </summary>
		[Display(Name = "Идентификатор внешнего пользователя")]
		public virtual int ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}

		/// <summary>
		/// Идентификатор почты юр лица, через которую настроена связь с физиком
		/// </summary>
		[Display(Name = "Идентификатор почты юр лица, через которую настроена связь с физиком")]
		public virtual int LegalCounterpartyEmailId
		{
			get => _legalCounterpartyEmailId;
			set => SetField(ref _legalCounterpartyEmailId, value);
		}
		
		/// <summary>
		/// Идентификатор почты юр лица, через которую настроена связь с физиком
		/// </summary>
		[Display(Name = "Пароль аакаунта")]
		[IgnoreHistoryTrace]
		public virtual string AccountPassword
		{
			get => _accountPassword;
			set => SetField(ref _accountPassword, value);
		}
		
		/// <summary>
		/// Состояние связи
		/// </summary>
		[Display(Name = "Состояние связи")]
		public virtual ConnectedCustomerConnectState ConnectState
		{
			get => _connectedCustomerConnectState;
			set => SetField(ref _connectedCustomerConnectState, value);
		}
		
		/// <summary>
		/// Причина блокировки
		/// </summary>
		[Display(Name = "Причина блокировки")]
		public virtual string BlockingReason
		{
			get => _blockingReason;
			set => SetField(ref _blockingReason, value);
		}

		public virtual void ActivateConnect()
		{
			ConnectState = ConnectedCustomerConnectState.Active;
			BlockingReason = null;
		}

		public virtual void BlockConnect()
		{
			ConnectState = ConnectedCustomerConnectState.Blocked;
		}

		public static LinkedLegalCounterpartyEmailToExternalUser Create(
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			int externalCounterpartyId,
			string accountPassword) =>
			new LinkedLegalCounterpartyEmailToExternalUser(
				legalCounterpartyId,
				legalCounterpartyEmailId,
				externalCounterpartyId,
				accountPassword);
	}
}
