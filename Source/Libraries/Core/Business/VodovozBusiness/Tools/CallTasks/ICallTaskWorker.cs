using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.CallTasks
{
	public interface ICallTaskWorker
	{
		ITaskCreationInteractive TaskCreationInteractive { get; set; }
		void CreateTasks(Order order);
	}
}