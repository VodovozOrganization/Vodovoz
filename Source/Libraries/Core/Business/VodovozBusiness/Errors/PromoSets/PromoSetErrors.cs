using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.PromoSets
{
	public static class PromoSetErrors
	{
		public static Error HasPreviousShipmentToAnotherIndividualClient =>
			new Error(
				typeof(PromoSetErrors),
				nameof(HasPreviousShipmentToAnotherIndividualClient),
				"По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо");
	}
}
