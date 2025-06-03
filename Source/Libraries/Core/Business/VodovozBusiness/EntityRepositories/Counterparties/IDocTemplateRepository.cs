using System.Collections.Generic;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IDocTemplateRepository
	{
		/// <summary>
		/// Получаем первый подходящий шаблон документа по указанным критериям.
		/// </summary>
		DocTemplate GetTemplate(IUnitOfWork uow, TemplateType type, Organization org, ContractType contractType);

		Result<IDocTemplate> GetMatchingTemplate(IUnitOfWork uow, TemplateType templateType, Organization organization, ContractType? contractType = null);

		IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org);

		DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org);
	}
}
