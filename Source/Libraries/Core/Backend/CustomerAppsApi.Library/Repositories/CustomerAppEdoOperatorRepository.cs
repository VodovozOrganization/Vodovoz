using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Edo;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Repositories
{
	public class CustomerAppEdoOperatorRepository : ICustomerAppEdoOperatorRepository
	{
		/// <inheritdoc/>
		public IEnumerable<EdoOperatorDto> GetAllEdoOperators(IUnitOfWork uow)
		{
			return uow.Session.Query<EdoOperator>()
				.Select(x => EdoOperatorDto.Create(x.Id, x.Code, x.Name, x.BrandName))
				.ToList();
		} 
	}
}
