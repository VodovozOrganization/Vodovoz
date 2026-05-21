using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderFromTemplateCreator : IOnlineOrderFromTemplateCreator
	{
		private readonly ILogger<OnlineOrderFromTemplateCreator> _logger;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;

		public OnlineOrderFromTemplateCreator(
			ILogger<OnlineOrderFromTemplateCreator> logger,
			IOnlineOrderFactory onlineOrderFactory,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
		}
		
		public async Task Create(IUnitOfWork uow, CancellationToken cancellation)
		{
			_logger.LogInformation("Начинаем работу по созданию онлайн заказов из шаблонов");
			var activeTemplates =
				_onlineOrderTemplateRepository.GetActiveOnlineOrdersTemplatesForCreateOrders(uow, DateTime.Now);

			var templatesWithWeekDaysAndProducts = await _onlineOrderTemplateRepository.GetOnlineOrdersTemplatesDataAsync(
				uow, activeTemplates.Select(x => x.Id).ToArray());

			var weekdays = templatesWithWeekDaysAndProducts.WeekdaysLookup;
			var products = templatesWithWeekDaysAndProducts.ProductsLookup;
			
			_logger.LogInformation("Всего шаблонов на обработку: {TemplatesCount}", templatesWithWeekDaysAndProducts.Templates.Count);

			foreach(var template in templatesWithWeekDaysAndProducts.Templates)
			{
				//Подумать над логикой присвоения даты доставки. Может лучше убрать все в одно место?
				template.DeliveryDate = DateTime.Today.AddDays(1);
				
				template.Weekdays = weekdays.Contains(template.Id)
					? new ObservableList<OnlineOrderTemplateWeekday>(weekdays[template.Id])
					: new ObservableList<OnlineOrderTemplateWeekday>();

				ProcessProducts(template, products);

				if(template.State == OrderTemplateDataState.NeedArchive)
				{
					var savedTemplate = uow.GetById<OnlineOrderTemplate>(template.Id);
					savedTemplate.UpdateState(false, true);
					
					_logger.LogInformation("Архивируем шаблон {TemplateId}", template.Id);
					
					//TODO создавать уведомления по необходимости
					//var notification = ;
					//await uow.SaveAsync(notification, cancellationToken: cancellation);
					await uow.SaveAsync(savedTemplate, cancellationToken: cancellation);
				}
				else
				{
					var onlineOrder = _onlineOrderFactory.Create(template);
					await uow.SaveAsync(onlineOrder, cancellationToken: cancellation);
				}
				
				await uow.CommitAsync(cancellation);
			}
		}

		private void ProcessProducts(
			OnlineOrderTemplateData template,
			ILookup<int, OnlineOrderTemplateProduct> items)
		{
			var products = items.Contains(template.Id)
				? new ObservableList<OnlineOrderTemplateProduct>(items[template.Id])
				: new ObservableList<OnlineOrderTemplateProduct>();

			CheckProducts(template, products);
			template.TemplateProducts = products;
		}

		private void CheckProducts(
			OnlineOrderTemplateData template,
			IEnumerable<OnlineOrderTemplateProduct> products)
		{
			foreach(var product in products)
			{
				if(product.Nomenclature.IsArchive
					|| (product.PromoSet != null && product.PromoSet.IsArchive))
				{
					_logger.LogInformation(
						"Товар с номенклатурой {Nomenclature} или промонабор {PromoSet} заархивирован. Шаблон {TemplateId} будет заархивирован",
						product.Nomenclature.ToString(),
						product.PromoSet?.Title,
						template.Id);
					
					template.State = OrderTemplateDataState.NeedArchive;
				}
			}

			if(template.State is null)
			{
				template.State = OrderTemplateDataState.Valid;
			}
		}
	}
}
