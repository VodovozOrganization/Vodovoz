using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Информация о связи юридического лица и физического,
	/// для возможности заказа пользователем физиком под указанным юриком в ИПЗ
	/// </summary>
	[HistoryTrace]
	public class ConnectedCustomer : PropertyChangedBase, IDomainObject
	{
		private int _legalCounterpartyId;
		private int _naturalCounterpartyPhoneId;
		private string _blockingReason;
		private ConnectedCustomerConnectState _connectedCustomerConnectState;

		protected ConnectedCustomer() { }
		
		private ConnectedCustomer(int legalCounterpartyId, int naturalCounterpartyPhoneId)
		{
			LegalCounterpartyId = legalCounterpartyId;
			NaturalCounterpartyPhoneId = naturalCounterpartyPhoneId;
			ConnectState = ConnectedCustomerConnectState.Active;
		}

		/// <summary>
		/// Id
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Id юридического лица, от которого сможет заказывать в ИПЗ физик
		/// </summary>
		[Display(Name = "Id юридического лица, к которому привязан физик")]
		public virtual int LegalCounterpartyId
		{
			get => _legalCounterpartyId;
			set => SetField(ref _legalCounterpartyId, value);
		}

		/// <summary>
		/// Id телефона клиента физика, который привязан к юр лицу, для возможности заказа
		/// </summary>
		[Display(Name = "Id телефона физика, привязанного к юрику")]
		public virtual int NaturalCounterpartyPhoneId
		{
			get => _naturalCounterpartyPhoneId;
			set => SetField(ref _naturalCounterpartyPhoneId, value);
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

		public static ConnectedCustomer Create(int legalCounterpartyId, int naturalCounterpartyPhoneId) =>
			new ConnectedCustomer(legalCounterpartyId, naturalCounterpartyPhoneId);
	}
}
