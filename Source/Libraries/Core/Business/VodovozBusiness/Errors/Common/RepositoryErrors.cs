using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Common
{
	public static class RepositoryErrors
	{
		/// <summary>
		/// Ошибка получения данных
		/// </summary>
		[Display(Name = "Ошибка получения данных")]		
		public static Error DataRetrievalError => new Error(
			typeof(RepositoryErrors),
			nameof(DataRetrievalError),
			"Ошибка получения данных");
	}
}
