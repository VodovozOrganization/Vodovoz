using Mango.Api.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mango.Api.Validators
{
	public class RequestValidator : IRequestValidator
	{
		private readonly IEnumerable<IValidator> _validators;

		public RequestValidator(IEnumerable<IValidator> validators)
		{
			_validators = validators ?? throw new ArgumentNullException(nameof(validators));
		}

		public bool Validate(EventRequestBase eventRequest)
		{
			return _validators.All(validator => validator.Validate(eventRequest));
		}
	}
}
