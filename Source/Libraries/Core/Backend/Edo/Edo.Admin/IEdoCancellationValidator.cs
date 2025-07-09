using Vodovoz.Core.Domain.Edo;

namespace Edo.Admin
{
	public interface IEdoCancellationValidator
	{
		bool CanCancelEdoTask(EdoTask edoTask);
	}
}