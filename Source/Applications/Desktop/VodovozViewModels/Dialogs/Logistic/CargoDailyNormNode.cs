using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class CargoDailyNormNode
	{
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public decimal Amount { get; set; }
		public string Postfix => "X грузоподъёмность";
	}
}