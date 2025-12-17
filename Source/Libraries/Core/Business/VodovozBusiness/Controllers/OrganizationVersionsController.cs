using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Controllers
{
	public class OrganizationVersionsController : IOrganizationVersionsController
	{
		public OrganizationVersionsController(Organization organization)
		{
			Organization = organization ?? throw new ArgumentNullException(nameof(organization));
		}

		public Organization Organization { get; }

		/// <summary>
		///  Создаёт и добавляет новую версию организации в список версий.
		/// </summary>
		/// <param name="startDate">Дата начала действия новой версии. Если равно null, берётся текущая дата</param>
		public OrganizationVersion CreateAndAddVersion(DateTime? startDate = null)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new OrganizationVersion
			{
				Organization = Organization,
			};

			return AddNewVersion(newVersion, startDate.Value);
		}

		///  <summary>
		///  Добавляет новую версию организации в список версий организации контроллера.
		///  Если предыдущая версия не имела дату окончания или заканчивалась позже даты начала новой версии,
		///  то в этой версии выставляется дата окончания, равная дате начала новой версии минус 1 миллисекунду
		///  </summary>
		///  <param name="newOrganizationVersion">Новая версия организации. Свойство StartDate в newOrganizationVersion игнорируется</param>
		///  <param name="startDate">
		/// 	Дата начала действия новой версии. Должна быть минимум на день позже, чем дата начала действия предыдущей версии.
		/// 	Время должно равняться 00:00:00
		///  </param>
		public OrganizationVersion AddNewVersion(OrganizationVersion newOrganizationVersion, DateTime startDate)
		{
			if(newOrganizationVersion == null)
			{
				throw new ArgumentNullException(nameof(newOrganizationVersion));
			}
			if(startDate.Date != startDate)
			{
				throw new ArgumentException("Время даты начала действия новой версии не равно 00:00:00", nameof(startDate));
			}
			if(newOrganizationVersion.Organization == null || newOrganizationVersion.Organization.Id != Organization.Id)
			{
				newOrganizationVersion.Organization = Organization;
			}

			if(Organization.OrganizationVersions.Any())
			{
				var currentLatestVersion = Organization.OrganizationVersions.MaxBy(x => x.StartDate).First();

				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}

				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			newOrganizationVersion.StartDate = startDate;
			Organization.ObservableOrganizationVersions.Insert(0, newOrganizationVersion);
			return newOrganizationVersion;
		}
		
		public void ChangeVersionStartDate(OrganizationVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}

			var previousVersion = GetPreviousVersionOrNull(version);

			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}

			version.StartDate = newStartDate;
		}

		public bool IsValidDateForVersionStartDateChange(OrganizationVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				return false;
			}

			if(version.StartDate == newStartDate)
			{
				return false;
			}

			if(newStartDate >= version.EndDate)
			{
				return false;
			}

			var previousVersion = GetPreviousVersionOrNull(version);

			return previousVersion == null || newStartDate > previousVersion.StartDate;
		}

		public bool IsValidDateForNewOrganizationVersion(DateTime dateTime)
		{
			return Organization.OrganizationVersions.All(x => x.StartDate < dateTime);
		}

		private OrganizationVersion GetPreviousVersionOrNull(OrganizationVersion currentVersion)
		{
			return Organization.OrganizationVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}
	}
}
