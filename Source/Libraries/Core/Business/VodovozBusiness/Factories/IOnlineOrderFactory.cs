using Vodovoz.Domain.Orders;
using VodovozBusiness.Nodes;

namespace VodovozBusiness.Factories
{
	public interface IOnlineOrderFactory
	{
		OnlineOrder Create(OnlineOrderTemplateData templateData);
	}
}
