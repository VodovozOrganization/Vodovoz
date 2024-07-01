using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Settings.Roboats;

namespace RoboatsCallsWorker
{
	public class StaleCallsController
	{
		private readonly ILogger<StaleCallsController> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IRoboatsSettings _roboatsSettings;

		public StaleCallsController(ILogger<StaleCallsController> logger, IUnitOfWorkFactory uowFactory, IRoboatsRepository roboatsRepository, IRoboatsSettings roboatsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public void CloseStaleCalls()
		{
			_logger.LogInformation($"Закрытие устаревших звонков.");

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var staleCalls = _roboatsRepository.GetStaleCalls(uow);
				foreach(var call in staleCalls)
				{
					var closeDetail = new RoboatsCallDetail
					{
						Call = call,
						Description = $"Закрыт по превышению таймаута ({_roboatsSettings.CallTimeout} мин)",
						FailType = RoboatsCallFailType.TimeOut,
						OperationTime = DateTime.Now,
						Operation = RoboatsCallOperation.ClosingStaleCalls
					};

					call.CallDetails.Add(closeDetail);
					call.Status = RoboatsCallStatus.Aborted;
					call.Result = RoboatsCallResult.Nothing;

					uow.Save(call);
				}
				uow.Commit();
			}
		}
	}
}
