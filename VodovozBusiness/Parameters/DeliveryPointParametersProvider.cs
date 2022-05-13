using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DeliveryPointParametersProvider: IDeliveryPointParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public DeliveryPointParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int EducationalInstitutionDeliveryPointCategoryId =>
			_parametersProvider.GetIntValue("educational_institution_delivery_point_category_id");
	}
}
