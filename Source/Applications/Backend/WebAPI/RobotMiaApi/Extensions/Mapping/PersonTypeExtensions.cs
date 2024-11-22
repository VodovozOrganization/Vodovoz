using System;
using Vodovoz.Core.Domain.Clients;
using PersonTypeV1 = RobotMiaApi.Contracts.Responses.V1.PersonType;

namespace RobotMiaApi.Extensions.Mapping
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
