using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Sale
{
	/// <summary>
	/// Создатель шаблона автозаказа
	/// </summary>
	public interface IOnlineOrderTemplateCreator
	{
		/// <summary>
		/// Создать шаблон автозаказа из онлайн заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ <see cref="OnlineOrder"/></param>
		/// <param name="schedule">Данные по доставке <see cref="TemplateDeliveryScheduleData"/></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Созданный шаблон <see cref="OnlineOrderTemplate"/></returns>
		Task<OnlineOrderTemplate> CreateAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			TemplateDeliveryScheduleData schedule,
			CancellationToken cancellationToken
		);
	}
}
