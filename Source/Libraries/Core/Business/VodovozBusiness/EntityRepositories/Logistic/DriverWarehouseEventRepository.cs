using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class DriverWarehouseEventRepository : IDriverWarehouseEventRepository
	{
		public IEnumerable<DriverWarehouseEvent> GetActiveDriverWarehouseEventsForDocument(IUnitOfWork uow, DocumentType documentType)
		{
			return uow.Session.QueryOver<DriverWarehouseEvent>()
				.Where(x => x.DocumentType == documentType)
				.And(x => !x.IsArchive)
				.List();
		}
	}
}
