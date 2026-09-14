using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace VodovozBusiness.Services.Logistics
{
	/// <summary>
	/// Сервис для получения контактного номера водителя, доставляющего заказ
	/// </summary>
	public interface IDriverContactNumberService
	{
		/// <summary>
		/// Возвращает номер для связи с водителем, доставляющим заказ
		/// Номер состоит из номера линии Манго и активного добавочного номера водителя,
		/// разделенных двумя запятыми, например +78122000000,,54678.
		/// Если активный добавочный номер водителя не найден или отключен сервис регистрации карточек водителей в Манго, 
		/// то возвращается только номер линии Манго
		/// </summary>
		/// <param name="unitOfWork">Unit Of Work</param>
		/// <param name="orderId">Код заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Контактный номер водителя</returns>
		Task<string> GetDriverContactNumberForCustomersApiAsync(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Возвращает номер для связи с водителем, доставляющим заказ для СМС уведомления
		/// Номер состоит из номера линии Манго и активного добавочного номера водителя,
		/// разделенных двумя запятыми с указанием добавочного номера в скобках, например +78122000000,,54678 (доб. 54678)
		/// Если активный добавочный номер водителя не найден, возвращается только номер линии Манго.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="orderId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<string> GetDriverContactNumberForSmsNotificationAsync(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken = default);
	}
}
