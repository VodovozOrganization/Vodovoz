using System;
using PersonType = Vodovoz.Core.Domain.Clients.PersonType;
using PersonTypeV1 = Vodovoz.RobotMia.Contracts.Responses.V1.PersonType;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="PersonType"/>
	/// </summary>
	public static class PersonTypeExtensions
	{
		/// <summary>
		/// Маппинг типа контрагента в <see cref="PersonTypeV1"/>
		/// </summary>
		/// <param name="personType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static PersonTypeV1 MapToApiPersonTypeV1(this PersonType personType)
			=> personType switch
			{
				PersonType.natural => PersonTypeV1.Natural,
				PersonType.legal => PersonTypeV1.Legal,
				_ => throw new ArgumentException("Неподдерживаемый тип контрагента", nameof(personType)),
			};
	}
}
