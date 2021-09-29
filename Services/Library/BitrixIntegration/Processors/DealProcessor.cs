using Bitrix;
using Bitrix.DTO;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Services;
using VodovozInfrastructure.Utils;
using BitrixPhone = Bitrix.DTO.Phone;
using Contact = Bitrix.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;

namespace BitrixIntegration.Processors
{
	//Класс перегружен, его необходимо разделить на зависимости отвечающие за свои действия
	public class DealProcessor
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBitrixClient _bitrixClient;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly CounterpartyContractFactory _counterpartyContractFactory;
		private readonly DealRegistrator _dealRegistrator;
		private readonly IBitrixServiceSettings _bitrixServiceSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;
		private readonly IDeliveryPointProcessor _deliveryPointProcessor;
		private readonly IProductProcessor _productProcessor;

		public DealProcessor(
			IUnitOfWorkFactory uowFactory,
			IBitrixClient bitrixClient,
			ICounterpartyContractRepository counterpartyContractRepository,
			CounterpartyContractFactory counterpartyContractFactory,
			DealRegistrator dealRegistrator,
			IBitrixServiceSettings bitrixServiceSettings,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository,
			IDeliveryScheduleRepository deliveryScheduleRepository,
			IDeliveryPointProcessor deliveryPointProcessor,
			IProductProcessor productProcessor
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
			_bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_deliveryPointProcessor = deliveryPointProcessor ?? throw new ArgumentNullException(nameof(deliveryPointProcessor));
			_productProcessor = productProcessor ?? throw new ArgumentNullException(nameof(productProcessor));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_dealRegistrator = dealRegistrator ?? throw new ArgumentNullException(nameof(dealRegistrator));
		}

		public void ProcessDeals(DateTime date)
		{
			var startDay = date.Date;
			var endDay = date.Date.AddDays(1).AddMilliseconds(-1);

			var deals = _bitrixClient.GetDeals(startDay, endDay).GetAwaiter().GetResult();
			foreach(var deal in deals)
			{
				_dealRegistrator.RegisterDealAsInProgress(deal.Id);
				ProcessDeal(deal);
			}
		}

		private void ProcessDeal(Deal deal)
		{
			_logger.Info($"Обработка сделки: {deal.Id}");

			//ВНИМАНИЕ! Тут реализована обработка сделки на основании создания нового заказа
			//необходимо сделать обработку заказа универсально как для нового так и для существующего
			//чтобы без опаски можно было прогнать сделку на существующем заказе и актуализировать информацию в нем

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = FindExistingOrder(uow, deal.Id);
				if(order != null)
				{
					_logger.Info($"Обработка сделки пропущена. Для сделки №{deal.Id} уже есть существующий заказ №{order.Id}.");
					return;
				}

				_logger.Info("Обработка контрагента");
				Counterparty counterparty = GetCounterpartyFromDeal(uow, deal);

				_logger.Info("Сборка заказа");
				order = CreateOrder(uow, deal, counterparty);

				_logger.Info("Обработка номенклатур");
				_productProcessor.ProcessProducts(uow, deal, order);

				foreach(var orderItem in order.OrderItems)
				{
					uow.Save(orderItem.Nomenclature);
				}

				uow.Save(order);
				uow.Commit();
			}
		}

		private Order FindExistingOrder(IUnitOfWork uow, uint dealId)
		{
			var order = _orderRepository.GetOrderByBitrixId(uow, dealId);
			if(order == null)
			{
				//Тут необходимо реализовать поиск заказа по контрагенту и точке доставке на день доставки
			}

			return order;
		}

		private Order CreateOrder(IUnitOfWork uow, Deal deal, Counterparty counterparty)
		{
			DeliveryPoint deliveryPoint = null;
			DeliverySchedule deliverySchedule = null;

			if(!deal.IsSelfDelivery)
			{
				_logger.Info("Обработка точек доставки");
				deliveryPoint = _deliveryPointProcessor.ProcessDeliveryPoint(uow, deal, counterparty);
				deliverySchedule = _deliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
				if(deliverySchedule == null)
				{
					throw new InvalidOperationException($"Не найдено время доставки DeliverySchedule ({deal.DeliverySchedule}) по bitrixId");
				}
			}

			var bitrixAccount = uow.GetById<Employee>(_bitrixServiceSettings.EmployeeForOrderCreate);
			var order = new Order()
			{
				UoW = uow,
				BitrixDealId = deal.Id,
				PaymentType = deal.GetPaymentMethod(),
				CreateDate = deal.CreateDate,
				DeliveryDate = deal.DeliveryDate,
				DeliverySchedule = deliverySchedule,
				Client = counterparty,
				DeliveryPoint = deliveryPoint,
				OrderStatus = OrderStatus.Accepted,
				Author = bitrixAccount,
				LastEditor = bitrixAccount,
				LastEditedTime = DateTime.Now,
				PaymentBySms = deal.IsSmsPayment,
				OrderPaymentStatus = deal.GetOrderPaymentStatus(),
				SelfDelivery = deal.IsSelfDelivery,
				Comment = deal.Comment,
				Trifle = deal.Trifle ?? 0,
				BottlesReturn = deal.BottlesToReturn,
				EShopOrder = (int)deal.Id,
				OnlineOrder = deal.OrderNumber ?? null
			};

			if(order.PaymentType == PaymentType.ByCard)
			{
				order.PaymentByCardFrom = uow.GetById<PaymentFrom>(7);
			}

			order.UpdateOrCreateContract(uow, _counterpartyContractRepository, _counterpartyContractFactory);

			return order;
		}

		private Counterparty GetCounterpartyFromDeal(IUnitOfWork uow, Deal deal)
		{
			if(deal.ContactId != 0)
			{
				return GetContact(uow, deal);
			}

			if(deal.CompanyId != 0)
			{
				return GetCompany(uow, deal);
			}

			throw new InvalidOperationException("Сделка не имеет ни контакта ни компании, такие сделки невозможно обработать");
		}

		private Counterparty GetContact(IUnitOfWork uow, Deal deal)
		{
			_logger.Info("Обработка контрагента как контакта");

			var contact = _bitrixClient.GetContact(deal.ContactId).GetAwaiter().GetResult()
				?? throw new InvalidOperationException($"Не удалось загрузить контакт №{deal.ContactId}");

			Counterparty counterparty = GetCounterpartyOrNull(uow, contact);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для контакта: {contact.Id} {contact.SecondName} {contact.Name} {contact.LastName} " +
						", создаем нового контрагента");
				counterparty = new Counterparty()
				{
					FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					Name = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					BitrixId = contact.Id,
					PersonType = PersonType.natural,
					CreateDate = contact.CreatedDate,
					PaymentMethod = deal.GetPaymentMethod(),
					IsArchive = false
				};
				AddPhonesToCounterparty(counterparty, contact.Phones);
			}

			return counterparty;
		}

		private Counterparty GetCompany(IUnitOfWork uow, Deal deal)
		{
			_logger.Info("Обработка контрагента как компании");

			var company = _bitrixClient.GetCompany(deal.CompanyId).GetAwaiter().GetResult()
				?? throw new InvalidOperationException($"Не удалось загрузить компанию №{deal.CompanyId}");

			Counterparty counterparty = GetCounterpartyOrNull(uow, company);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для компании: {company.Title}, создаем нового контрагента");
				counterparty = new Counterparty()
				{
					FullName = company.Title,
					Name = company.Title,
					BitrixId = company.Id,
					PersonType = PersonType.legal,
					TypeOfOwnership = NamesUtils.TryGetOrganizationType(company.Title) ?? "",
					CreateDate = company.DateCreate,
					PaymentMethod = deal.GetPaymentMethod(),
					IsArchive = false
				};
				AddPhonesToCounterparty(counterparty, company.Phones);
			}

			return counterparty;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Contact contact)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, contact.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = contact.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			string contactName;
			if(!string.IsNullOrWhiteSpace(contact.LastName))
			{
				contactName = contact.LastName;
			}
			else if(!string.IsNullOrWhiteSpace(contact.Name))
			{
				contactName = contact.Name;
			}
			else if(!string.IsNullOrWhiteSpace(contact.SecondName))
			{
				contactName = contact.SecondName;
			}
			else
			{
				throw new InvalidOperationException("Контакт не содержит имени, фамилии и отчества, необходимых для поиска контрагента");
			}

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, contactName, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для контакта с BitrixId {contact.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и части имени");
				return counterparty;
			}
			
			if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для контакта с BitrixId {contact.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и части имени. Невозможно выбрать кого-то одного");
				return null;
			}
			
			_logger.Info($"Для контакта с BitrixId {contact.Id} не найдено контрагентов по телефону и части имени");
			return null;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Company company)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, company.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = company.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			if(string.IsNullOrWhiteSpace(company.Title))
			{
				throw new InvalidOperationException("Компания не содержит названия, необходимого для поиска контрагента");
			}

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, company.Title, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для компании с BitrixId {company.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и названию");
				return counterparty;
			}
			
			if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для компании с BitrixId {company.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и названию. Невозможно выбрать кого-то одного");
				return null;
			}

			_logger.Info($"Для компании с BitrixId {company.Id} не найдено контрагентов по телефону и названию");
			return null;
		}

		private void AddPhonesToCounterparty(Counterparty counterparty, IEnumerable<BitrixPhone> phones)
		{
			foreach(var contactPhone in phones)
			{
				var phone = new Phone
				{
					Number = contactPhone.Value
				};
				_logger.Info($"Добавляем телефон: {contactPhone} контрагенту {counterparty.FullName}");
				counterparty.Phones.Add(phone);
			}
		}
	}
}
