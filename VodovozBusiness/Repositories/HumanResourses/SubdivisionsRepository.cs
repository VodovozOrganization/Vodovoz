using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repositories.HumanResources
{
	[Obsolete("Используйте одноимённый класс из EntityRepositories.Subdivisions")]
	public static class SubdivisionsRepository
	{
		[Obsolete]
		public static Subdivision GetQCDepartment(IUnitOfWork uow)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetQCDepartment(uow);
		}

		/// <summary>
		/// Возврат отсортированного списка подразделений компании
		/// </summary>
		/// <returns>Список подразделений</returns>
		/// <param name="uow">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		[Obsolete]
		public static IList<Subdivision> GetAllDepartments(IUnitOfWork uow, bool orderByDescending = false)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetAllDepartments(uow, orderByDescending);
		}

		/// <summary>
		/// Список дочерних подразделений
		/// </summary>
		/// <returns>Список дочерних подразделений</returns>
		/// <param name="uow">Unit of Work</param>
		/// <param name="parentSubdivision">Подразделение, список дочерних подразделений которого требуется вернуть</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		[Obsolete]
		public static IList<Subdivision> GetChildDepartments(IUnitOfWork uow, Subdivision parentSubdivision, bool orderByDescending = false)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetChildDepartments(uow, parentSubdivision, orderByDescending);
		}

		/// <summary>
		/// Список подразделений в которых произодится работа с указанными документами
		/// </summary>
		[Obsolete]
		public static IEnumerable<Subdivision> GetSubdivisionsForDocumentTypes(IUnitOfWork uow, Type[] documentTypes)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetSubdivisionsForDocumentTypes(uow, documentTypes);
		}

		/// <summary>
		/// Список складов, которые привязаны к подразделению
		/// </summary>
		/// <returns>Склады подразделения</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="subdivision">подразделение</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		[Obsolete]
		public static IList<Warehouse> GetWarehouses(IUnitOfWork uow, Subdivision subdivision, bool orderByDescending = false)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetWarehouses(uow, subdivision, orderByDescending);
		}

		[Obsolete]
		public static IEnumerable<Subdivision> GetAvailableSubdivionsForUser(IUnitOfWork uow, Type[] documentsTypes)
		{
			return new EntityRepositories.Subdivisions.SubdivisionRepository().GetAvailableSubdivionsForUser(uow, documentsTypes);
		}
	}
}
