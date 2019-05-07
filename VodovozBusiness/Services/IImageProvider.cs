using System;
using Gdk;
using QS.DomainModel.UoW;

namespace Vodovoz.Services
{
	public interface IImageProvider
	{
		Pixbuf GetCrmIndicator(IUnitOfWork uow);
	}
}
