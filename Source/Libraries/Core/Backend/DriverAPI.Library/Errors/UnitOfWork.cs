using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Errors
{
	internal class UnitOfWork
	{
		public static Error CommitError => new Error(
			typeof(UnitOfWork),
			nameof(CommitError),
			"Ошибка сохранения данных");
	}
}
