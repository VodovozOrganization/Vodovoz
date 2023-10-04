using Mango.Api.Dto;

namespace Mango.Api.Validators
{
	public interface IValidator
	{
		bool Validate(EventRequestBase eventRequest);
	}
}
