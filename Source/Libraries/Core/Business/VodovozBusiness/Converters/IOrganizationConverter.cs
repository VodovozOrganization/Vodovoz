using System;
using TaxcomEdo.Contracts.Organizations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Converters
{
	public interface IOrganizationConverter
	{
		/// <summary>
		/// Конвертация организации <see cref="Organization"/> в информацию о ней для ЭДО <see cref="OrganizationInfoForEdo"/>
		/// </summary>
		/// <param name="organization">Организация</param>
		/// <param name="dateTime"></param>
		/// <returns>Информация об организации для ЭДО</returns>
		OrganizationInfoForEdo ConvertOrganizationToOrganizationInfoForEdo(Organization organization, DateTime dateTime);
	}
}
