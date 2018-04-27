using System.Collections.Generic;
using System.Linq;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository.Client
{
	public static class DocTemplateRepository
	{
		public static IList<DocTemplate> GetTemplatesOnlyForOrganization (IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate> ()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate> ();
		}

		public static IList<DocTemplate> GetTemplatesForAnyOrganization (IUnitOfWork uow, TemplateType type)
		{
			return uow.Session.QueryOver<DocTemplate> ()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == null)
				.List<DocTemplate> ();
		}

		/// <summary>
		/// Получаем первый подходящий шаболон документа по указанным критериям.
		/// </summary>
		public static DocTemplate GetTemplate (IUnitOfWork uow, TemplateType type, Organization org)
		{
			var forOrg = GetTemplatesOnlyForOrganization(uow, type, org);
			if (forOrg.Count > 0)
				return forOrg.First();

			var any = GetTemplatesForAnyOrganization(uow, type);
			return any.FirstOrDefault();
		}

		public static IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				      .List<DocTemplate>().OfType<IDocTemplate>().ToList();
		}

	}
}

