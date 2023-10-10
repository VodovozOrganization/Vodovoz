using Mango.Api.Dto;

namespace Mango.Api.Validators
{
	public interface IRequestValidator
	{
		bool Validate(EventRequestBase eventRequest);
	}
}
