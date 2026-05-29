using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IOnlinePaymentRepository
	{
		/// <summary>
		/// Получить онлайн-платёж
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="externalId">Номер онлайн-платежа из ИПЗ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Онлайн-платёж</returns>
		Task<OnlinePayment> GetByExternalIdAsync(
			IUnitOfWork uow,
			int externalId,
			CancellationToken cancellationToken);
	}
}
