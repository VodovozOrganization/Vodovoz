using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.RobotMia;

namespace Vodovoz.RobotMia.Api.Services
{
	/// <inheritdoc cref="IIncomingCallCallService"/>
	public class IncomingCallCallService : IIncomingCallCallService
	{
		private readonly ILogger<IncomingCallCallService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<RobotMiaCall> _robotMiaCallRespository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="uowFactory"></param>
		/// <param name="robotMiaCallRespository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public IncomingCallCallService(
			ILogger<IncomingCallCallService> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<RobotMiaCall> robotMiaCallRespository)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory
				?? throw new ArgumentNullException(nameof(uowFactory));
			_robotMiaCallRespository = robotMiaCallRespository
				?? throw new ArgumentNullException(nameof(robotMiaCallRespository));
		}

		/// <inheritdoc/>
		public async Task<RobotMiaCall> GetCallByIdAsync(Guid callId, IUnitOfWork uow = default)
		{
			try
			{
				uow ??= _uowFactory.CreateWithoutRoot();

				var call = _robotMiaCallRespository
					.Get(uow, rmc => rmc.CallGuid == callId)
					.SingleOrDefault();

				return await Task.FromResult(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка получения звонка");
				throw;
			}
		}

		/// <inheritdoc/>
		public async Task RegisterCallAsync(Guid callId, string phoneNumber, int? counterpartyId, IUnitOfWork uow = default)
		{
			try
			{
				uow ??= _uowFactory.CreateWithoutRoot();

				var call = await GetCallByIdAsync(callId, phoneNumber.NormalizePhone(), uow);

				call.CounterpartyId = counterpartyId;

				await uow.SaveAsync(call);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникло исключение при регистрации записи в мониторинг.");
			}
		}

		private async Task<RobotMiaCall> GetCallByIdAsync(Guid callId, string phoneNumber, IUnitOfWork uow = default)
		{
			try
			{
				uow ??= _uowFactory.CreateWithoutRoot();

				phoneNumber.NormalizePhone();

				var call = _robotMiaCallRespository
					.Get(uow, rmc => rmc.CallGuid == callId)
					.SingleOrDefault();

				if(call is null)
				{
					call = RobotMiaCall.Create(callId, phoneNumber, DateTime.Now);

					await uow.SaveAsync(call);
				}

				return call;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка получения звонка");
				throw;
			}
		}
	}
}
