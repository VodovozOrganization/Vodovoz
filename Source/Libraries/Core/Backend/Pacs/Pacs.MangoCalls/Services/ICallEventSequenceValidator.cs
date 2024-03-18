using Mango.Core.Dto;

namespace Pacs.MangoCalls.Services
{
	public interface ICallEventSequenceValidator
	{
		bool ValidateCallSequence(MangoCallEvent callEvent);
	}
}
