using System;
using Vodovoz.Domain.Documents;
using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Library.Converters
{
	public class CarLoadDocumentConverter
	{
		public CarLoadDocumentDto ConvertToApiCarLoadDocument(CarLoadDocument carLoadDocument, int loadPriority)
		{
			var carLoadDocumentDto = new CarLoadDocumentDto
			{
				Id = carLoadDocument.Id,
				Driver = carLoadDocument.RouteList.Driver?.FullName,
				Car = carLoadDocument.RouteList.Car?.RegistrationNumber,
				LoadPriority = loadPriority,
				State = (LoadOperationStateEnumDto)Enum.Parse(typeof(LoadOperationStateEnumDto), carLoadDocument.LoadOperationState.ToString())
			};

			return carLoadDocumentDto;
		}
	}
}
