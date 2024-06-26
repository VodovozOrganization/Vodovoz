using System.Collections.Generic;
using QS.DocTemplates;
using QS.DomainModel.UoW;
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

		IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org);
		DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org);
	}
}
