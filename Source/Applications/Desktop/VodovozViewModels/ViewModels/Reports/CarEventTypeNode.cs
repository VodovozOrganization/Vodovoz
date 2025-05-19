using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class CarEventTypeNode
	{
		public virtual bool Selected { get; set; }
		public CarEventType CarEventType { get; }
		public string Title => CarEventType.Name;

		public CarEventTypeNode(CarEventType carEventType)
		{
			CarEventType = carEventType;
		}
	}
}
