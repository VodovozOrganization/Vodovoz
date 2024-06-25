using System.Collections.Generic;
using System.Linq;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	public class DocTemplateRepository : IDocTemplateRepository
	{
		/// <summary>
		/// Получаем первый подходящий шаболон документа по указанным критериям.
		/// </summary>
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
					  .List<DocTemplate>().OfType<IDocTemplate>().ToList();
		}

		public DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate>().FirstOrDefault();
		}
	}
}

