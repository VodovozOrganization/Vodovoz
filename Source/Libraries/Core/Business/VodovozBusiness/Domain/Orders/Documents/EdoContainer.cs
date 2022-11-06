using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class EdoContainer : PropertyChangedBase, IDomainObject
	{
		private bool _received;
		private bool _isIncoming;
		private string _mainDocumentId;
		private string _errorDescription;
		private Guid? _internalId;
		private Guid? _docFlowId;
		private Order _order;
		private Counterparty _counterparty;
		private byte[] _container;
		private EdoContainerStatus _edoContainerStatus;
		
		public virtual int Id { get; set; }

		public virtual bool Received
		{
			get => _received;
			set => SetField(ref _received, value);
		}
		
		public virtual bool IsIncoming
		{
			get => _isIncoming;
			set => SetField(ref _isIncoming, value);
		}
		
		public virtual string MainDocumentId
		{
			get => _mainDocumentId;
			set => SetField(ref _mainDocumentId, value);
		}
		
		public virtual Guid? InternalId
		{
			get => _internalId;
			set => SetField(ref _internalId, value);
		}
		
		public virtual Guid? DocFlowId
		{
			get => _docFlowId;
			set => SetField(ref _docFlowId, value);
		}
		
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
		
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		public virtual byte[] Container
		{
			get => _container;
			set => SetField(ref _container, value);
		}
		
		public virtual EdoContainerStatus EdoContainerStatus
		{
			get => _edoContainerStatus;
			set => SetField(ref _edoContainerStatus, value);
		}
		
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}
	}

	public enum EdoContainerStatus
	{
		Unknown,
		InProgress,
		Succeed,
		Warning,
		Error,
		NotStarted,
		CompletedWithDivergences,
		NotAccepted,
	}
}
