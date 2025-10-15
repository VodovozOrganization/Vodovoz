using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.Entity;

namespace Edo.Transfer.Extensions.ExtensionInterfaces
{
	public interface IBulkOperationSupport<TDomainObject> where TDomainObject : IDomainObject
	{
		Task BulkInsertItems(IEnumerable<TDomainObject> domainObjects, CancellationToken cancellationToken);
		Task BulkInsertItemsWithParent(int parentId, IEnumerable<TDomainObject> domainObjects, CancellationToken cancellationToken);
		string BuildInsertSql(int rowCount);
	}
}
