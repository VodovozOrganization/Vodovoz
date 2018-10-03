using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using QSSupportLib;

namespace Vodovoz.Repositories
{
	public static class SubdivisionsRepository
	{
		public static Subdivision GetQCDepartment(IUnitOfWork uow)
		{
			var qcDep = "номер_отдела_ОКК";
			if(!MainSupport.BaseParameters.All.ContainsKey(qcDep))
				throw new InvalidProgramException("В параметрах базы не указан номер отдела контроля качества [номер_отдела_ОКК]");
			return uow.GetById<Subdivision>(int.Parse(MainSupport.BaseParameters.All[qcDep]));
		}

		/// <summary>
		/// Возврат отсортированного списка подразделений компании
		/// </summary>
		/// <returns>Список подразделений</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public static IList<Subdivision> GetAllDepartments(IUnitOfWork UoW, bool orderByDescending = false)
		{
			var query = UoW.Session.QueryOver<Subdivision>()
			   .OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}
	}
}
