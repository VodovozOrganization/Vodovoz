using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerNotifications.Application.Providers
{
	/// <summary>
	/// Провайдер номера для связи клиента с водителем
	/// </summary>
	public interface IDriverContactNumberProvider
	{
		/// <summary>
		/// Возвращает номер для связи с водителем, доставляющим заказ.
		/// Номер состоит из номера линии Манго и активного добавочного номера водителя,
		/// разделенных двумя запятыми, например +78122000000,,54678.
		/// Если активный добавочный номер водителя не найден, возвращается только номер линии Манго.
		/// </summary>
		/// <param name="unitOfWork">Unit Of Work</param>
		/// <param name="orderId">Код заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task<string> GetDriverContactNumberAsync(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken = default);
	}
}
