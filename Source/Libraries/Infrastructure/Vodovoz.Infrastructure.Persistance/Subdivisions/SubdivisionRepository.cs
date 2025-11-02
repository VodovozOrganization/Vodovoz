using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Permissions;
using Vodovoz.Settings;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Infrastructure.Persistance.Subdivisions
{
	internal sealed class SubdivisionRepository : ISubdivisionRepository
	{
		private readonly ISettingsController _settingsController;

		public SubdivisionRepository(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		/// <summary>
		/// Список подразделений в которых произодится работа с указанными документами
		/// </summary>
		public IEnumerable<Subdivision> GetSubdivisionsForDocumentTypes(IUnitOfWork uow, Type[] documentTypes)
		{
			Subdivision subdivisionAlias = null;
			TypeOfEntity typeOfEntityAlias = null;
			return uow.Session.QueryOver(() => subdivisionAlias)
				.Left.JoinAlias(() => subdivisionAlias.DocumentTypes, () => typeOfEntityAlias)
				.WhereRestrictionOn(() => typeOfEntityAlias.Type).IsIn(documentTypes.Select(x => x.Name).ToArray())
				.List()
				.Distinct()
				;
		}

		public IEnumerable<Subdivision> GetCashSubdivisions(IUnitOfWork uow)
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			return GetSubdivisionsForDocumentTypes(uow, cashDocumentTypes);
		}

		public IEnumerable<Subdivision> GetCashSubdivisionsAvailableForUser(IUnitOfWork uow, UserBase user)
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			var validationResult = EntitySubdivisionForUserPermissionValidator.Validate(uow, user.Id, cashDocumentTypes);
			var subdivisionsList = new List<Subdivision>();
			foreach(var item in cashDocumentTypes)
			{
				subdivisionsList.AddRange(validationResult
					.Where(x => x.GetPermission(item).Read)
					.Select(x => x.Subdivision)
				);
			}
			return subdivisionsList.Distinct();
		}

		public Subdivision GetQCDepartment(IUnitOfWork uow)
		{
			var qcDep = "номер_отдела_ОКК";

			if(!_settingsController.ContainsSetting(qcDep))
			{
				throw new InvalidProgramException("В параметрах базы не указан номер отдела контроля качества [номер_отдела_ОКК]");
			}

			return uow.GetById<Subdivision>(int.Parse(_settingsController.GetStringValue(qcDep)));
		}

		/// <summary>
		/// Возврат отсортированного списка подразделений компании
		/// </summary>
		/// <returns>Список подразделений</returns>
		/// <param name="uow">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public IList<Subdivision> GetAllDepartmentsOrderedByName(IUnitOfWork uow, bool orderByDescending = false)
		{
			var query = uow.Session.QueryOver<Subdivision>()
			   .OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}

		/// <summary>
		/// Список дочерних подразделений
		/// </summary>
		/// <returns>Список дочерних подразделений</returns>
		/// <param name="uow">Unit of Work</param>
		/// <param name="parentSubdivision">Подразделение, список дочерних подразделений которого требуется вернуть</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public IList<Subdivision> GetChildDepartments(IUnitOfWork uow, Subdivision parentSubdivision, bool orderByDescending = false)
		{
			return GetAllDepartmentsOrderedByName(uow, orderByDescending).Where(s => s.ParentSubdivision == parentSubdivision).ToList();
		}

		/// <summary>
		/// Список складов, которые привязаны к подразделению
		/// </summary>
		/// <returns>Склады подразделения</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="subdivision">подразделение</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public IList<Warehouse> GetWarehouses(IUnitOfWork uow, Subdivision subdivision, bool orderByDescending = false)
		{
			var subdivisionId = subdivision?.Id;
			var query = uow.Session.QueryOver<Warehouse>()
							.Where(w => w.OwningSubdivisionId == subdivisionId)
							.OrderBy(w => w.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}

		public IEnumerable<Subdivision> GetAvailableSubdivionsForUser(IUnitOfWork uow, Type[] documentsTypes)
		{
			var validationResult = EntitySubdivisionForUserPermissionValidator.Validate(uow, documentsTypes);

			var subdivisionsList = new List<Subdivision>();
			foreach(var item in documentsTypes)
			{
				subdivisionsList.AddRange(validationResult
					.Where(x => x.GetPermission(item).Read)
					.Select(x => x.Subdivision)
				);
			}

			return subdivisionsList.Distinct();
		}

		public IEnumerable<int> GetAllSubdivisionsIds(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Subdivision>()
				.Select(s => s.Id)
				.List<int>();
		}

		/// <summary>
		/// Возвращает список подразделений, в которых имеются машины с заданным типом (ларгус, фура, газель) и принадлежностью (компания, авто в раскате, авто водителя)
		/// Результирующий список отсортирован по имени
		/// </summary>
		/// <param name="uow">UoW</param>
		/// <param name="carTypeOfUses">Тип автомобиля</param>
		/// <param name="carOwnTypes">Принадлежность автомобиля</param>
		/// <returns>Список данных подразделений(Код, Наименование)</returns>
		public IList<NamedDomainObjectNode> GetAvailableSubdivisionsInAccordingWithCarTypeAndOwner(
			IUnitOfWork uow, CarTypeOfUse[] carTypeOfUses, CarOwnType[] carOwnTypes)
		{
			Car car = null;
			Subdivision subdivision = null;
			Employee driverEmployee = null;
			CarModel carModel = null;
			CarVersion carVersion = null;
			NamedDomainObjectNode resultAlias = null;

			var availableSubdivisions = uow.Session
				.QueryOver(() => subdivision)
				.JoinEntityAlias(() => driverEmployee, () => driverEmployee.Subdivision.Id == subdivision.Id)
				.JoinEntityAlias(() => car, () => car.Driver.Id == driverEmployee.Id)
				.JoinAlias(() => car.CarModel, () => carModel)
				.JoinAlias(() => car.CarVersions, () => carVersion)
				.Where(() =>
					!car.IsArchive
					&& carVersion.EndDate == null
					&& driverEmployee.Status == EmployeeStatus.IsWorking
					&& driverEmployee.DateFired == null
					&& driverEmployee.Category == EmployeeCategory.driver)
				.WhereRestrictionOn(() => carModel.CarTypeOfUse).IsIn(carTypeOfUses)
				.WhereRestrictionOn(() => carVersion.CarOwnType).IsIn(carOwnTypes)
				.SelectList(list => list
					.SelectGroup(() => subdivision.Id).WithAlias(() => resultAlias.Id)
					.Select(() => subdivision.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>())
				.OrderBy(() => subdivision.Name).Asc
				.List<NamedDomainObjectNode>();

			return availableSubdivisions;
		}
	}
}
