using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class BooleanExtensions
	{
		public static Result ToResult(this bool value, Error error)
		{
			return value ? Result.Success() : Result.Failure(error);
		}
	}
}
