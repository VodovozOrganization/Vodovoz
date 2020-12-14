using System.Collections.Generic;
using System.Linq;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Repository.Client
{
	public static class DocTemplateRepository
	{

		/// <summary>
		/// Получаем первый подходящий шаболон документа по указанным критериям.
		/// </summary>
		public static DocTemplate GetTemplate (IUnitOfWork uow, TemplateType type, Organization org, ContractType contractType)
		{
			var templates = uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.Where(x => x.ContractType == contractType)
				.List<DocTemplate>(); ;
			return templates.FirstOrDefault();
		}

		public static IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				      .List<DocTemplate>().OfType<IDocTemplate>().ToList();
		}

		public static DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate>().FirstOrDefault();
		}

	}
}

