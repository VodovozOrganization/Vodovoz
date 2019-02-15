using System;
using NHibernate.Mapping;
using System.Collections.Generic;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.Permissions;
using QS.DomainModel.UoW;
using System.Linq;
using QSProjectsLib;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Domain.Permissions
{
	public static class EntitySubdivisionForUserPermissionValidator
	{
		/// <summary>
		/// Проверка прав доступа по списку сущностей для текущего пользователя
		/// </summary>
		/// <param name="entityTypes">Список сущностей</param>
		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, Type[] entityTypes)
		{
			var user = UserRepository.GetCurrentUser(uow);
			return Validate(uow, user.Id, entityTypes);
		}


		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, int userId, Type[] entityTypes)
		{
			var result = new List<EntitySubdivisionForUserPermissionValidationResult>();

			string[] entityNames = entityTypes.Select(x => x.Name).ToArray();
			var employee = EmployeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();
			Subdivision mainSubdivision = employee == null ? null : employee.Subdivision;

			var subdivisionsForEntities = SubdivisionsRepository.GetSubdivisionsForDocumentTypes(uow, entityTypes);
			var sdsdsds = PermissionRepository.GetAllSubdivisionForUserEntityPermissionForSomeEntities(uow, userId, entityNames);
			var specialPermissions = sdsdsds
				.Where(x => subdivisionsForEntities.Contains(x.Subdivision) || Subdivision.ReferenceEquals(x.Subdivision, mainSubdivision));

			foreach(var entityType in entityTypes) {
				var mainPermission = PermissionsSettings.EntityPermissionValidator.Validate(entityType, userId);
				foreach(var permissionitem in specialPermissions.Where(x => x.TypeOfEntity.Type == entityType.Name)) {
					EntitySubdivisionForUserPermissionValidationResult resultItem = result.FirstOrDefault(x => x.Subdivision == permissionitem.Subdivision);
					if(resultItem == null) {
						var isMainSubdivision = permissionitem.Subdivision != null && mainSubdivision != null && permissionitem.Subdivision.Id == mainSubdivision.Id;
						resultItem = new EntitySubdivisionForUserPermissionValidationResult(permissionitem.Subdivision, isMainSubdivision);
						result.Add(resultItem);
					}
					resultItem.AddPermission(
						entityType,
						new EntityPermission(
							mainPermission.Create && permissionitem.CanCreate,
							mainPermission.Read && permissionitem.CanRead,
							mainPermission.Update && permissionitem.CanUpdate,
							mainPermission.Delete && permissionitem.CanDelete
						)
					);
				}
			} 

			return result;
		}

		/// <summary>
		/// Проверка прав доступа по одной сущности для текущего пользователя
		/// </summary>
		/// <param name="entityTypes">Список сущностей</param>
		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, Type entityType)
		{
			var user = UserRepository.GetCurrentUser(uow);
			return Validate(uow, user.Id, entityType);
		}

		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, int userId, Type entityType)
		{
			var result = new List<EntitySubdivisionForUserPermissionValidationResult>();
			var employee = EmployeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();
			var mainPermission = PermissionsSettings.EntityPermissionValidator.Validate(entityType, userId);
			Subdivision mainSubdivision = employee == null ? null : employee.Subdivision;

			var subdivisionsForEntities = SubdivisionsRepository.GetSubdivisionsForDocumentTypes(uow, new Type[] { entityType });
			var specialPermissions = PermissionRepository.GetAllSubdivisionForUserEntityPermissionForOneEntity(uow, userId, entityType.Name)
				.Where(x => subdivisionsForEntities.Contains(x.Subdivision) || Subdivision.ReferenceEquals(x.Subdivision, mainSubdivision));

			foreach(var permissionitem in specialPermissions.Where(x => x.TypeOfEntity.Type == entityType.Name)) {
				EntitySubdivisionForUserPermissionValidationResult resultItem = result.FirstOrDefault(x => x.Subdivision == permissionitem.Subdivision);
				if(resultItem == null) {
					var isMainSubdivision = permissionitem.Subdivision != null && mainSubdivision != null && permissionitem.Subdivision.Id == mainSubdivision.Id;
					resultItem = new EntitySubdivisionForUserPermissionValidationResult(permissionitem.Subdivision, isMainSubdivision);
					result.Add(resultItem);
				}
				resultItem.AddPermission(
					entityType,
					new EntityPermission(
						mainPermission.Create && permissionitem.CanCreate,
						mainPermission.Read && permissionitem.CanRead,
						mainPermission.Update && permissionitem.CanUpdate,
						mainPermission.Delete && permissionitem.CanDelete
					)
				);
			}
				

			return result;
		}


	}

	public class EntitySubdivisionForUserPermissionValidationResult : IEntitySubdivisionForUserPermissionValidationResult
	{
		public bool IsMainSubdivision { get; private set; }
		public Subdivision Subdivision { get; private set; }
		private Dictionary<Type, EntityPermission> Permissions { get; set; }

		public EntitySubdivisionForUserPermissionValidationResult(Subdivision subdivision, bool isMainSubdivision)
		{
			Permissions = new Dictionary<Type, EntityPermission>();
			Subdivision = subdivision;
		}

		public void AddPermission(Type type, EntityPermission permission)
		{
			if(Permissions.ContainsKey(type)) {
				return;
			}

			Permissions.Add(type, permission);
		}

		public EntityPermission GetPermission(Type entityType)
		{
			if(!Permissions.ContainsKey(entityType)) {
				return EntityPermission.AllDenied;
			}
			return Permissions[entityType];
		}
	}

	public interface IEntitySubdivisionForUserPermissionValidationResult
	{
		bool IsMainSubdivision { get; }
		Subdivision Subdivision { get; }
		EntityPermission GetPermission(Type entityType);
	}
}
