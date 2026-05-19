using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IOnlineOrderTemplateRepository _onlineOrderTemplateRepository;

		public OnlineOrderFromTemplateCreator(
			IOnlineOrderFactory onlineOrderFactory,
			IOnlineOrderTemplateRepository onlineOrderTemplateRepository
			)
		{
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_onlineOrderTemplateRepository = onlineOrderTemplateRepository ?? throw new ArgumentNullException(nameof(onlineOrderTemplateRepository));
		}
		
		public async Task Create(IUnitOfWork uow, CancellationToken cancellation)
		{
			var activeTemplates =
				_onlineOrderTemplateRepository.GetActiveOnlineOrdersTemplatesForCreateOrders(uow, DateTime.Now);

			var templatesWithWeekDaysAndProducts = await _onlineOrderTemplateRepository.GetOnlineOrdersTemplatesDataAsync(
				uow, activeTemplates.Select(x => x.Id).ToArray());

			var weekdays = templatesWithWeekDaysAndProducts.WeekdaysLookup;
			var products = templatesWithWeekDaysAndProducts.ProductsLookup;

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
