using System.Collections.Generic;
using System.Linq;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using TemplateType = Vodovoz.Core.Domain.Clients.TemplateType;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class DocTemplateRepository : IDocTemplateRepository
	{
		public DocTemplate GetTemplate(IUnitOfWork uow, TemplateType type, Organization org, ContractType contractType)
		{
			var templates = uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.Where(x => x.ContractType == contractType)
				.List<DocTemplate>();

			return templates.FirstOrDefault();
		}

		public IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate>()
				.Cast<IDocTemplate>()
				.ToList();
		}

		public DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate>().FirstOrDefault();
		}

		public Result<IDocTemplate> GetMatchingTemplate(
			IUnitOfWork unitOfWork,
			TemplateType templateType,
			Organization organization,
			ContractType? contractType = null)
		{
			var query = unitOfWork.Session.Query<DocTemplate>();

			if(contractType != null)
			{
				query = query.Where(x => x.ContractType == contractType);
			}

			if(organization != null)
			{
				query = query.Where(x => x.Organization != organization || x.Organization == null);
			}

			query = query.Where(x => x.TemplateType == templateType);

			var result = query.OrderByDescending(x => x.Organization)
				.Take(1)
				.SingleOrDefault();

			if(result is null)
			{
				return Result.Failure<IDocTemplate>(Errors.Documents.DocumentTemplateErrors.NotFound);
			}

			return result;
		}
	}
}
