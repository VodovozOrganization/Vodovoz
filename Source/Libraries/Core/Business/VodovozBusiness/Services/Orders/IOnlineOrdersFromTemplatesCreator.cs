using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrdersFromTemplatesCreator
	{
		/// <summary>
		/// Создание автозаказов из активных шаблонов, подходящих по условиям
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task CreateAsync(IUnitOfWork uow, CancellationToken cancellationToken);
	}
}
