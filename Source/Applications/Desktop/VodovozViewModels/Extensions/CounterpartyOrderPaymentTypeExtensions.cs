using Vodovoz.Core.Domain.Common;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;

namespace Vodovoz.ViewModels.Extensions
{
	public static class CounterpartyOrderPaymentTypeExtensions
	{
		public static EdoLightsMatrixPaymentType ToEdoLightsMatrixPaymentType(this CounterpartyOrderPaymentType source)
		{
			switch(source)
			{
				case CounterpartyOrderPaymentType.Cashless:
					return EdoLightsMatrixPaymentType.Cashless;
				default:
					return EdoLightsMatrixPaymentType.Receipt;
			}
		}
	}
}
