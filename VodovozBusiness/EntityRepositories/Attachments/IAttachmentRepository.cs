using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Attachments;

namespace Vodovoz.EntityRepositories.Attachments
{
	public interface IAttachmentRepository
	{
		IList<Attachment> GetAllAttachmentsForEntity(IUnitOfWork uow, EntityType entityType, int entityId);
	}
}
