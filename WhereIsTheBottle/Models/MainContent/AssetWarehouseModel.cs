using QS.DomainModel.UoW;
using QS.Models;

namespace WhereIsTheBottle.Models.MainContent
{
	public class AssetWarehouseModel : UoWFactoryModelBase
	{
		public AssetWarehouseModel(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{

		}
	}
}
