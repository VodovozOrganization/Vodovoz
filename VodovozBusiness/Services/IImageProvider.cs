using System.Drawing;
using QS.DomainModel.UoW;

namespace Vodovoz.Services
{
	public interface IImageProvider
	{
		Image GetCrmIndicator(IUnitOfWork uow);
	}
}
