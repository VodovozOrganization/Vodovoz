using Edo.Problem.Routine.Options;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис обработки проблем с отправкой фискальных документов в ЭДО
	/// </summary>
	public class FiscalDocumentSendErrorProblemService
	{
		private readonly ILogger<FiscalDocumentSendErrorProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> _options;
		private readonly IBus _messageBus;

		public FiscalDocumentSendErrorProblemService(
			ILogger<FiscalDocumentSendErrorProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> options,
			IBus messageBus)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <summary>
		/// Обработчик фискальных документов с проблемой отправки в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		public async Task ProcessProblemFiscalDocuments(CancellationToken cancellationToken)
		{
		}
	}
}
