using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Tools
{
	public class DocumentsHelper
	{
		private readonly IDriverWarehouseEventRepository _driverWarehouseEventRepository;

		public DocumentsHelper(
			IDriverWarehouseEventRepository driverWarehouseEventRepository)
		{
			_driverWarehouseEventRepository =
				driverWarehouseEventRepository ?? throw new ArgumentNullException(nameof(driverWarehouseEventRepository));
		}
		
		public void AddQrEventForDocument(IUnitOfWork uow, DocumentType documentType)
		{
			var events = _driverWarehouseEventRepository.GetActiveDriverWarehouseEventsForDocument(uow, documentType);

			foreach(var @event in events)
			{
				
			}
		}
	}

	public enum DocumentType
	{
		RouteList,
		CarLoadDocument,
		CarUnloadDocument
	}
}
