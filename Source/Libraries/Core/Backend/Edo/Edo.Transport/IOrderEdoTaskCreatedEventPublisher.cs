using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transport
{
	/// <summary>
	/// Публикует событие запуска задачи ЭДО заказа
	/// </summary>
	public interface IOrderEdoTaskCreatedEventPublisher
	{
		/// <summary>
		/// Публикует событие, соответствующее типу задачи ЭДО
		/// </summary>
		Task Publish(OrderEdoTask edoTask, CancellationToken cancellationToken = default);
	}
}
