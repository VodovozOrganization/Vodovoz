using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;
using VodovozInfrastructure.Versions;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Models
{
	public class GeoGroupVersionsModel
	{
		private readonly IUserService _userService;
		private readonly IEmployeeService _employeeService;

		public GeoGroupVersionsModel(IUserService userService, IEmployeeService employeeService)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		}

		public void CreateVersion(IUnitOfWork uow, GeoGroup geoGroup)
		{
			var currentEmployee = _employeeService.GetEmployeeForUser(uow, _userService.CurrentUserId);

			var newVersion = new GeoGroupVersion();
			newVersion.GeoGroup = geoGroup;
			newVersion.Author = currentEmployee;
			newVersion.BaseLatitude = null;
			newVersion.BaseLongitude = null;
			newVersion.CashSubdivision = null;
			newVersion.Warehouse = null;
			newVersion.CreationDate = DateTime.Now;
			newVersion.Status = VersionStatus.Draft;

			geoGroup.ObservableVersions.Add(newVersion);
		}

		public void CopyVersion(IUnitOfWork uow, GeoGroup geoGroup, GeoGroupVersion copyFrom)
		{
			var currentEmployee = _employeeService.GetEmployeeForUser(uow, _userService.CurrentUserId);
			var cash = copyFrom.CashSubdivision != null ? uow.GetById<Subdivision>(copyFrom.CashSubdivision.Id) : null;
			var warehouse = copyFrom.Warehouse != null ? uow.GetById<Warehouse>(copyFrom.Warehouse.Id) : null;

			var newVersion = new GeoGroupVersion();
			newVersion.GeoGroup = geoGroup;
			newVersion.Author = currentEmployee;
			newVersion.BaseLatitude = copyFrom.BaseLatitude;
			newVersion.BaseLongitude = copyFrom.BaseLongitude;
			newVersion.CashSubdivision = cash;
			newVersion.Warehouse = warehouse;
			newVersion.CreationDate = DateTime.Now;
			newVersion.Status = VersionStatus.Draft;

			geoGroup.ObservableVersions.Add(newVersion);
		}

		public void ActivateVersion(GeoGroup geoGroup, GeoGroupVersion activatingVersion)
		{
			if(!geoGroup.Versions.Contains(activatingVersion))
			{
				throw new InvalidOperationException($"Активируемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			var activeVersion = geoGroup.Versions.FirstOrDefault(v => v.Status == VersionStatus.Active);

			if(activeVersion != null)
			{
				CloseVersion(geoGroup, activeVersion);
				activatingVersion.ActivationDate = activeVersion.ClosingDate.Value.AddMilliseconds(1);
			}
			else
			{
				activatingVersion.ActivationDate = DateTime.Now;
			}

			activatingVersion.Status = VersionStatus.Active;
		}

		public void CloseVersion(GeoGroup geoGroup, GeoGroupVersion closingVersion)
		{
			if(!geoGroup.Versions.Contains(closingVersion))
			{
				throw new InvalidOperationException($"Закрываемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			if(closingVersion.Status != VersionStatus.Active)
			{
				throw new InvalidOperationException($"Можно закрывать только версию данных части города в статусе {VersionStatus.Active.GetEnumTitle()}");
			}

			closingVersion.ClosingDate = DateTime.Now;
			closingVersion.Status = VersionStatus.Closed;
		}

		public void RemoveVersion(GeoGroup geoGroup, GeoGroupVersion deletingVersion)
		{
			if(!geoGroup.Versions.Contains(deletingVersion))
			{
				throw new InvalidOperationException($"Удаляемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			if(deletingVersion.Status != VersionStatus.Draft)
			{
				throw new InvalidOperationException($"Можно удалять только версию данных части города в статусе {VersionStatus.Draft.GetEnumTitle()}");
			}

			geoGroup.ObservableVersions.Remove(deletingVersion);
		}
	}
}
