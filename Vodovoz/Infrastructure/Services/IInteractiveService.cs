using QS.Dialog;
namespace Vodovoz.Infrastructure.Services
{
	public interface IInteractiveService
	{
		IInteractiveMessage InteractiveMessage { get; }
		IInteractiveQuestion InteractiveQuestion { get; }
	}
}