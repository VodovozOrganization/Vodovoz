using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Vodovoz.Tools
{
	public static class ValidationHelper
	{
		public static string RaiseValidationAndGetResult(this IValidatableObject validatableObject)
		{
			return RaiseValidationAndGetResult(validatableObject, new Dictionary<object, object>());
		}

		public static string RaiseValidationAndGetResult(this IValidatableObject validatableObject, IDictionary<object, object> context)
		{
			string result = null;
			var validationContext = new ValidationContext(validatableObject, context);
			result = RaiseValidationAndGetResult(validatableObject, validationContext);
			return result;
		}

		public static string RaiseValidationAndGetResult(this IValidatableObject validatableObject, ValidationContext context)
		{
			string result = null;
			var validationResult = validatableObject.Validate(context);
			if(validationResult.Any()) {
				result = string.Join(Environment.NewLine, validationResult.Select(x => x.ErrorMessage));
			}
			return result;
		}
	}
}
