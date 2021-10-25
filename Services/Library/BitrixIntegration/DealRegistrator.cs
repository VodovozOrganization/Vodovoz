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

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.InProgress, dealId).GetAwaiter().GetResult();
				if(!statusSetted)
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.InProgress}) для сделки Id {dealId}");
				}

				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = false;
				dealRegistration.NeedSync = false;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		public void RegisterDealAsError(uint dealId, string errorDescription)
		{
			if(dealId == 0)
			{
				throw new ArgumentException($"Номер сделки должен быть указан", nameof(dealId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var dealRegistration = _bitrixRepository.GetDealRegistration(uow, dealId) ??
				                       throw new InvalidOperationException($"В бд не найдена сделка с Id {dealId}");

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Error, dealId).GetAwaiter().GetResult();
				if(!statusSetted)
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Error}) для сделки Id {dealId}");
				}

				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = false;
				dealRegistration.NeedSync = false;
				var toInsert = $"{DateTime.Now.ToShortDateString()} {errorDescription}\n";
				dealRegistration.ErrorDescription = dealRegistration.ErrorDescription.Insert(0, toInsert);
				var currentLength = dealRegistration.ErrorDescription.Length;
				if(currentLength > 1000)
				{
					_logger.Warn($"Превышена длина лога ошибок регистрации сделки {dealRegistration.Id}: {currentLength} > 1000");
					dealRegistration.ErrorDescription = dealRegistration.ErrorDescription.Remove(1000, currentLength - 1000);
				}

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

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Fail, dealId).GetAwaiter().GetResult();
				if(statusSetted)
				{
					dealRegistration.NeedSync = false;
				}
				else
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Fail}) для сделки Id {dealId}");
					dealRegistration.NeedSync = true;
				}

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

				bool statusSetted = _bitrixClient.SetStatusToDeal(DealStatus.Success, dealId).GetAwaiter().GetResult();
				if(statusSetted)
				{
					dealRegistration.NeedSync = false;
				}
				else
				{
					_logger.Warn($"Не удалось установить в битриксе статус ({DealStatus.Success}) для сделки Id {dealId}");
					dealRegistration.NeedSync = true;
				}

				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.Success = true;

				uow.Save(dealRegistration);
				uow.Commit();
			}
		}

		public void BindOrderToRegistration(uint dealId, Order order)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var dealRegistration = _bitrixRepository.GetDealRegistration(uow, dealId);
				if(dealRegistration.Order != null)
				{
					if(dealRegistration.Order.Id != order.Id)
					{
						throw new InvalidOperationException($"Для сделки {dealId} уже привязан заказ с id {dealRegistration.Order.Id}." +
						                                    $" Попытка изменить привязанный заказ на заказ с id {order.Id}");
					}
					else
					{
						return;
					}
				}

				dealRegistration.Order = order;
				dealRegistration.ProcessedDate = DateTime.Now;
				dealRegistration.NeedSync = true;

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
