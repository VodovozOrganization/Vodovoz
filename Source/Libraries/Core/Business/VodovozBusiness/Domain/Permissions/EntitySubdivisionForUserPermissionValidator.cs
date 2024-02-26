using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.Domain.Permissions
{
	public static class EntitySubdivisionForUserPermissionValidator
	{
		private static IEmployeeRepository _employeeRepository => ScopeProvider.Scope.Resolve<IEmployeeRepository>();
		private static ISubdivisionRepository _subdivisionRepository => ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
		private static IUserRepository _userRepository => ScopeProvider.Scope.Resolve<IUserRepository>();
		private static IPermissionRepository _permissionRepository => ScopeProvider.Scope.Resolve<IPermissionRepository>();

		/// <summary>
		/// Проверка прав доступа по списку сущностей для текущего пользователя
		/// </summary>
		/// <param name="entityTypes">Список сущностей</param>
		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, Type[] entityTypes)
		{
			var user = _userRepository.GetCurrentUser(uow);
			return Validate(uow, user.Id, entityTypes);
		}

		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, int userId, Type[] entityTypes)
		{
			var result = new List<EntitySubdivisionForUserPermissionValidationResult>();

			string[] entityNames = entityTypes.Select(x => x.Name).ToArray();
			var employee = _employeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();
			Subdivision mainSubdivision = employee?.Subdivision;

			if(mainSubdivision != null) {
				var mainTypesName = mainSubdivision.DocumentTypes.Select(x => x.Type);
				var mainAvailableTypes = entityTypes.Where(x => mainTypesName.Contains(x.Name));
				if(mainAvailableTypes.Any()) {
					EntitySubdivisionForUserPermissionValidationResult mainResultItem = new EntitySubdivisionForUserPermissionValidationResult(mainSubdivision, true);
					foreach(var mainAvailableType in mainAvailableTypes) {
						var mainPermission = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(mainAvailableType, userId);
						mainResultItem.AddPermission(
							mainAvailableType,
							new EntityPermission(
								mainPermission.CanCreate,
								mainPermission.CanRead,
								mainPermission.CanUpdate,
								mainPermission.CanDelete
							)
						);
					}
					result.Add(mainResultItem);
				}
			}

			var subdivisionsForEntities = _subdivisionRepository.GetSubdivisionsForDocumentTypes(uow, entityTypes);
			var specialPermissions = _permissionRepository.GetAllSubdivisionForUserEntityPermissionForSomeEntities(uow, userId, entityNames)
				.Where(x => subdivisionsForEntities.Contains(x.Subdivision) || Subdivision.ReferenceEquals(x.Subdivision, mainSubdivision));

			foreach(var entityType in entityTypes) {
				var mainPermission = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(entityType, userId);
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
							mainPermission.CanCreate && permissionitem.CanCreate,
							mainPermission.CanRead && permissionitem.CanRead,
							mainPermission.CanUpdate && permissionitem.CanUpdate,
							mainPermission.CanDelete && permissionitem.CanDelete
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
			var user = _userRepository.GetCurrentUser(uow);
			return Validate(uow, user.Id, entityType);
		}

		public static IEnumerable<IEntitySubdivisionForUserPermissionValidationResult> Validate(IUnitOfWork uow, int userId, Type entityType)
		{
			var result = new List<EntitySubdivisionForUserPermissionValidationResult>();
			var employee = _employeeRepository.GetEmployeesForUser(uow, userId).FirstOrDefault();
			var mainPermission = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(entityType, userId);
			Subdivision mainSubdivision = employee?.Subdivision;

			if(mainSubdivision != null) {
				var mainTypesName = mainSubdivision.DocumentTypes.Select(x => x.Type);
				if(mainTypesName.Contains(entityType.Name)) {
					EntitySubdivisionForUserPermissionValidationResult mainResultItem = new EntitySubdivisionForUserPermissionValidationResult(mainSubdivision, true);
					mainResultItem.AddPermission(
						entityType,
						new EntityPermission(
							mainPermission.CanCreate,
							mainPermission.CanRead,
							mainPermission.CanUpdate,
							mainPermission.CanDelete
						)
					);
					result.Add(mainResultItem);
				}
			}


			var subdivisionsForEntities = _subdivisionRepository.GetSubdivisionsForDocumentTypes(uow, new Type[] { entityType });
			var specialPermissions = _permissionRepository.GetAllSubdivisionForUserEntityPermissionForOneEntity(uow, userId, entityType.Name)
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
						mainPermission.CanCreate && permissionitem.CanCreate,
						mainPermission.CanRead && permissionitem.CanRead,
						mainPermission.CanUpdate && permissionitem.CanUpdate,
						mainPermission.CanDelete && permissionitem.CanDelete
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
