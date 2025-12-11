using System;
using System.Linq;
using MoreLinq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Controllers.Cash
{
	public class VatRateVersionController : IVatRateVersionController
	{
		private readonly Nomenclature _nomenclature;
		private readonly Organization _organization;

		public VatRateVersionController(Nomenclature nomenclature, Organization organization)
		{
			_nomenclature = nomenclature;
			_organization = organization;
		}

		/// <summary>
		///  Создаёт и добавляет новую версию НДС.
		/// </summary>
		/// <param name="vatRateVersionType">Тип ставки НДС (для какой сущности)</param>
		/// <param name="startDate">Дата начала действия новой версии. Если равно null, берётся текущая дата</param>
		public VatRateVersion CreateAndAddVersion(VatRateVersionType vatRateVersionType, DateTime? startDate = null)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var newVersion = new VatRateVersion
			{
				Organization = _organization,
				Nomenclature = _nomenclature,
			};

			return AddNewVersion(vatRateVersionType, newVersion, startDate.Value);
		}

		///  <summary>
		///  Добавляет новую версию НДС в список версий организации или номенклатуры контроллера.
		///  Если предыдущая версия не имела дату окончания или заканчивалась позже даты начала новой версии,
		///  то в этой версии выставляется дата окончания, равная дате начала новой версии минус 1 миллисекунду
		///  </summary>
		///  <param name="vatRateVersionType">Тип ставки НДС (для какой сущности)</param>
		///  <param name="newVatRateVersion">Новая версия организации. Свойство StartDate в newOrganizationVersion игнорируется</param>
		///  <param name="startDate">
		/// 	Дата начала действия новой версии. Должна быть минимум на день позже, чем дата начала действия предыдущей версии.
		/// 	Время должно равняться 00:00:00
		///  </param>
		private VatRateVersion AddNewVersion(VatRateVersionType vatRateVersionType, VatRateVersion newVatRateVersion, DateTime startDate)
		{
			if(newVatRateVersion == null)
			{
				throw new ArgumentNullException(nameof(newVatRateVersion));
			}
			if(startDate.Date != startDate)
			{
				throw new ArgumentException("Время даты начала действия новой версии не равно 00:00:00", nameof(startDate));
			}
			
			switch(vatRateVersionType)
			{
				case VatRateVersionType.Organization:
					if(newVatRateVersion.Organization == null || newVatRateVersion.Organization.Id != _organization.Id)
					{
						newVatRateVersion.Organization = _organization;
					}
					if(_organization != null && _organization.VatRateVersions.Any())
					{
						var currentLatestVersion = _organization.VatRateVersions.MaxBy(x => x.StartDate).First();

						if(startDate < currentLatestVersion.StartDate.AddDays(1))
						{
							throw new ArgumentException(
								"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
								nameof(startDate));
						}

						currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
				
						newVatRateVersion.StartDate = startDate;
						_organization.VatRateVersions.Insert(0, newVatRateVersion);
					}
					
					break;
				case VatRateVersionType.Nomenclature:
					if(newVatRateVersion.Nomenclature == null || newVatRateVersion.Nomenclature.Id != _nomenclature.Id)
					{
						newVatRateVersion.Nomenclature = _nomenclature;
					}
					
					if(_nomenclature != null && _nomenclature.VatRateVersions.Any())
					{
						var currentLatestVersion = _nomenclature.VatRateVersions.MaxBy(x => x.StartDate).First();

						if(startDate < currentLatestVersion.StartDate.AddDays(1))
						{
							throw new ArgumentException(
								"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
								nameof(startDate));
						}

						currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
				
						newVatRateVersion.StartDate = startDate;
						_nomenclature.VatRateVersions.Insert(0, newVatRateVersion);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(vatRateVersionType), vatRateVersionType, null);
			}
			
			return newVatRateVersion;
		}

		/// <summary>
		/// Меняет дату у версии
		/// </summary>
		/// <param name="version">Версия для изменения</param>
		/// <param name="newStartDate">Новая дата</param>
		/// <param name="vatRateVersionType">Тип ставки НДС (для какой сущности)</param>
		/// <exception cref="ArgumentNullException">Если версия не выбрана</exception>
		public void ChangeVersionStartDate(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}

			var previousVersion = GetPreviousVersionOrNull(version, vatRateVersionType);

			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}

			version.StartDate = newStartDate;
		}

		public bool IsValidDateForNewVatRateVersion(DateTime dateTime, VatRateVersionType vatRateVersionType) 
			=> _organization.OrganizationVersions.All(x => x.StartDate < dateTime);

		/// <summary>
		/// Валидность даты для версии
		/// </summary>
		/// <param name="version">Версия</param>
		/// <param name="newStartDate">Новая дата</param>
		/// <param name="vatRateVersionType">Тип ставки НДС (для какой сущности)</param>
		/// <returns>true\false</returns>
		public bool IsValidDateForVersionStartDateChange(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType)
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

			var previousVersion = GetPreviousVersionOrNull(version, vatRateVersionType);

			return previousVersion == null || newStartDate > previousVersion.StartDate;
		}
		
		/// <summary>
		/// Получить последнюю версию НДС
		/// </summary>
		/// <param name="currentVersion"></param>
		/// <param name="vatRateVersionType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private VatRateVersion GetPreviousVersionOrNull(VatRateVersion currentVersion, VatRateVersionType vatRateVersionType)
		{
			switch(vatRateVersionType)
			{
				case VatRateVersionType.Organization:
					return _organization.VatRateVersions
						.Where(x => x.StartDate < currentVersion.StartDate)
						.OrderByDescending(x => x.StartDate)
						.FirstOrDefault();
					break;
				case VatRateVersionType.Nomenclature:
					return _nomenclature.VatRateVersions
						.Where(x => x.StartDate < currentVersion.StartDate)
						.OrderByDescending(x => x.StartDate)
						.FirstOrDefault();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(vatRateVersionType), vatRateVersionType, null);
			}
		}

		
	}
}
