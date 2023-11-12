using MassTransit;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operator.Client
{
	public class OperatorClient : IOperatorClient, IObserver<OperatorState>, IDisposable
	{
		private readonly IDisposable _stateSubscription;
		private readonly int _operatorId;
		private readonly ILogger<OperatorClient> _logger;
		private readonly IScopedClientFactory _requestClientFactory;
		private readonly OperatorStateConsumer _operatorStateConsumer;

		public int OperatorId => throw new NotImplementedException();

		public OperatorClient(
			int operatorId,
			ILogger<OperatorClient> logger,
			IScopedClientFactory requestClientFactory, 
			OperatorStateConsumer operatorStateConsumer)
		{
			_operatorId = operatorId;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestClientFactory = requestClientFactory ?? throw new ArgumentNullException(nameof(requestClientFactory));
			_operatorStateConsumer = operatorStateConsumer ?? throw new ArgumentNullException(nameof(operatorStateConsumer));

			_stateSubscription = _operatorStateConsumer.Subscribe(this);
		}

		public event EventHandler<OperatorState> StateChanged;

		public async Task<OperatorState> GetState()
		{
			throw new NotImplementedException();
		}

		public async Task<OperatorState> StartWorkShift(string phoneNumber)
		{
			var client = _requestClientFactory.CreateRequestClient<StartWorkShift>();

			var command = new StartWorkShift
			{
				OperatorId = _operatorId,
				PhoneNumber = phoneNumber
			};

			var response = await client.GetResponse<OperatorState>(command);
			return response.Message;
		}

		public async Task<OperatorState> EndWorkShift()
		{
			var client = _requestClientFactory.CreateRequestClient<EndWorkShift>();

			var command = new EndWorkShift
			{
				OperatorId = _operatorId
			};

			var response = await client.GetResponse<OperatorState>(command);
			return response.Message;
		}

		public async Task<OperatorState> ChangeNumber(string phoneNumber)
		{
			var client = _requestClientFactory.CreateRequestClient<ChangePhone>();

			var command = new ChangePhone
			{
				OperatorId = _operatorId,
				PhoneNumber = phoneNumber
			};

			var response = await client.GetResponse<OperatorState>(command);
			return response.Message;
		}

		public async Task<OperatorState> StartBreak(CancellationToken cancellationToken = default)
		{
			var client = _requestClientFactory.CreateRequestClient<StartBreak>();

			var command = new StartBreak
			{
				OperatorId = _operatorId
			};

			var response = await client.GetResponse<OperatorState>(command, cancellationToken);
			return response.Message;
		}

		public async Task<OperatorState> EndBreak(CancellationToken cancellationToken = default)
		{
			var client = _requestClientFactory.CreateRequestClient<EndBreak>();

			var command = new EndBreak
			{
				OperatorId = _operatorId
			};

			var response = await client.GetResponse<OperatorState>(command, cancellationToken);
			return response.Message;
		}

		public void OnNext(OperatorState value)
		{
			StateChanged?.Invoke(this, value);
		}

		public void OnError(Exception error)
		{
			_logger.LogError(error, "Ошибка при передачи на стороне клиента полученного уведомления о состоянии оператора");
		}

		public void OnCompleted()
		{
			_stateSubscription?.Dispose();
		}

		public void Dispose()
		{
			OnCompleted();
		}
	}
}
