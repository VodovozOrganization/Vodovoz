using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Delivery;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Repositories
{
	public class OnlineTemplateRepository : IOnlineTemplateRepository
	{
		/// <inheritdoc/>
		public ILookup<WeekDayName, DeliveryScheduleDto> GetWeeklyDeliverySchedules(IUnitOfWork uow, int onlineOrderId)
		{
			var query = from deliveryScheduleRestriction in uow.Session.Query<DeliveryScheduleRestriction>()
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on deliveryScheduleRestriction.DeliverySchedule.Id equals deliverySchedule.Id
				join district in uow.Session.Query<District>()
					on deliveryScheduleRestriction.District.Id equals district.Id
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on district.Id equals deliveryPoint.District.Id
				join onlineOrder in uow.Session.Query<OnlineOrder>()
					on deliveryPoint.Id equals onlineOrder.DeliveryPoint.Id
				where deliveryScheduleRestriction.WeekDay != WeekDayName.Today
					&& onlineOrder.Id == onlineOrderId
				
				select new WeekDayDeliverySchedule
				{
					WeekDay = deliveryScheduleRestriction.WeekDay,
					Schedule = DeliveryScheduleDto.Create(
						deliverySchedule.Id,
						$"{deliverySchedule.From:hh\\:mm} - {deliverySchedule.To:hh\\:mm}")
				};

			return query
				.ToLookup(x => x.WeekDay, y => y.Schedule);
		}
		
		/// <inheritdoc/>
		public ILookup<WeekDayName, DeliveryScheduleDto> GetMonthlyDeliverySchedules(IUnitOfWork uow, int onlineOrderId)
		{
			
			
			
		}
	}
}
