using System;
using TaxcomEdo.Contracts.Organizations;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow.Converters
{
	public interface IOrganizationConverter
	{
		/// <summary>
		/// Конвертация организации <see cref="OrganizationEntity"/> в информацию о ней для ЭДО <see cref="OrganizationInfoForEdo"/>
		/// </summary>
		/// <param name="organization">Организация</param>
		/// <param name="dateTime"></param>
		/// <returns>Информация об организации для ЭДО</returns>
		OrganizationInfoForEdo ConvertOrganizationToOrganizationInfoForEdo(OrganizationEntity organization, DateTime dateTime);
	}
}
