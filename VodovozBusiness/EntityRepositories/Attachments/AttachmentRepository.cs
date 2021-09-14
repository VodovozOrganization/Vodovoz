using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Attachments;

namespace Vodovoz.EntityRepositories.Attachments
{
	public class AttachmentRepository : IAttachmentRepository
	{
		public IList<Attachment> GetAllAttachmentsForEntity(IUnitOfWork uow, EntityType entityType, int entityId)
		{
			return uow.Session.QueryOver<Attachment>()
				.Where(a => a.EntityType == entityType)
				.And(a => a.EntityId == entityId)
				.List();
		}
	}
}
