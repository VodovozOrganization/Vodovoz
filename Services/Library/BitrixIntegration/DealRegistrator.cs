using Bitrix;
using Bitrix.DTO;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;

namespace BitrixIntegration
{
	public sealed class DealRegistrator
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBitrixRepository _bitrixRepository;
		private readonly IBitrixClient _bitrixClient;

		public DealRegistrator(IUnitOfWorkFactory uowFactory, IBitrixRepository bitrixRepository, IBitrixClient bitrixClient)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_bitrixRepository = bitrixRepository ?? throw new ArgumentNullException(nameof(bitrixRepository));
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
		}

		public void RegisterDealAsInProgress(uint dealId)
		{
			if(dealId == 0)
			{
				throw new ArgumentException($"Номер сделки должен быть указан", nameof(dealId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var dealRegistration = _bitrixRepository.GetDealRegistration(uow, dealId);
				if(dealRegistration == null)
				{
					dealRegistration = CreateNew(dealId);
				}

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.InProgress, dealId);
				if(!statusSetted)
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.InProgress}) для сделки Id {dealId}");
				}

				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = false;
				dealRegistration.NeedSync = true;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		public void RegisterDealAsError(uint dealId, int orderId, string errorDescription)
		{
			if(dealId == 0)
			{
				throw new ArgumentException($"Номер сделки должен быть указан", nameof(dealId));
			}

			if(orderId < 1)
			{
				throw new ArgumentException($"Номер заказа должен быть указан", nameof(orderId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = uow.GetById<Order>(orderId) ?? throw new InvalidOperationException($"В бд не найден заказ с Id {orderId}");

				var dealRegistration = GetExistingRegistrationOrCreate(uow, dealId, orderId);

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Error, dealId);
				if(!statusSetted)
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Error}) для сделки Id {dealId}");
				}

				dealRegistration.Order = order;
				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = false;
				dealRegistration.NeedSync = true;
				dealRegistration.ErrorDescription = errorDescription;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		public void RegisterDealAsFail(uint dealId, int orderId)
		{
			if(dealId == 0)
			{
				throw new ArgumentException($"Номер сделки должен быть указан", nameof(dealId));
			}

			if(orderId < 1)
			{
				throw new ArgumentException($"Номер заказа должен быть указан", nameof(orderId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = uow.GetById<Order>(orderId) ?? throw new InvalidOperationException($"В бд не найден заказ с Id {orderId}");

				var dealRegistration = GetExistingRegistrationOrCreate(uow, dealId, orderId);

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Fail, dealId);
				if(statusSetted)
				{
					dealRegistration.NeedSync = false;
				}
				else
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Fail}) для сделки Id {dealId}");
					dealRegistration.NeedSync = true;
				}

				dealRegistration.Order = order;
				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = false;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		public void RegisterDealAsSuccess(uint dealId, int orderId)
		{
			if(dealId == 0)
			{
				throw new ArgumentException($"Номер сделки должен быть указан", nameof(dealId));
			}

			if(orderId < 1)
			{
				throw new ArgumentException($"Номер заказа должен быть указан", nameof(orderId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = uow.GetById<Order>(orderId) ?? throw new InvalidOperationException($"В бд не найден заказ с Id {orderId}");

				var dealRegistration = GetExistingRegistrationOrCreate(uow, dealId, orderId);

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Success, dealId);
				if(statusSetted)
				{
					dealRegistration.NeedSync = false;
				}
				else
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Success}) для сделки Id {dealId}");
					dealRegistration.NeedSync = true;
				}

				dealRegistration.Order = order;
				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = true;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		private BitrixDealRegistration GetExistingRegistrationOrCreate(IUnitOfWork uow, uint dealId, int orderId)
		{
			var dealRegistration = _bitrixRepository.GetDealRegistration(uow, dealId);

			if(dealRegistration == null)
			{
				dealRegistration = _bitrixRepository.GetDealRegistrationByOrder(uow, orderId);
			}

			if(dealRegistration == null)
			{
				return CreateNew(dealId);
			}

			if(dealRegistration.BitrixId != dealId)
			{
				throw new InvalidOperationException($"Найденная по номеру заказа, регистрация сделки " +
					$"(Id сделки: {dealRegistration.BitrixId}, Id заказа: {dealRegistration.Order.Id})" +
					$" предназначена для другой сделки. Попытка регистрации сделки Id {dealId} для заказа Id {orderId}");
			}

			if(dealRegistration.Order.Id != orderId)
			{
				throw new InvalidOperationException($"Найденная по номеру сделки, регистрация сделки " +
					$"(Id сделки: {dealRegistration.BitrixId}, Id заказа: {dealRegistration.Order.Id})" +
					$" предназначена для другого заказа. Попытка регистрации сделки Id {dealId} для заказа Id {orderId}");
			}

			return dealRegistration;
		}

		private BitrixDealRegistration CreateNew(uint bitrixId)
		{
			var newRegistration = new BitrixDealRegistration
			{
				BitrixId = bitrixId,
				CreateDate = DateTime.Now
			};
			return newRegistration;
		}
	}
}
