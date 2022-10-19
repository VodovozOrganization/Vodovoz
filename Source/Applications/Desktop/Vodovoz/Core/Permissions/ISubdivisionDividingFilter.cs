using System;
using NHibernate;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Core.Permissions
{
	public interface ISubdivisionAccessFilter
	{
		IQueryOver<TEntity, TEntity> FilterBySubdivisionsAccess<TEntity>(IQueryOver<TEntity, TEntity> baseQuery)
			where TEntity : class, IDomainObject, ISubdivisionEntity;
	}
}
