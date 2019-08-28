using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.CallTasks
{
	public interface IAutoCallTaskFactory
	{
		Order Order { get; set; }
		ITaskCreationInteractive TaskCreationInteractive { get; set; }
	}
}