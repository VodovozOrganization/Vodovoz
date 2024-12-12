using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Profitability
{
	public class AverageMileageCarsByTypeOfUseNode
	{
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public decimal Distance { get; set; }
		public int CountCars { get; set; }
	}
}
