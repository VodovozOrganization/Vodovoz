using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запросов получения данных об адресе
	/// </summary>
	public class WaterTypeHandler : GetRequestHandlerBase
	{
		private readonly RoboatsRepository _roboatsRepository;

		public override string Request => RoboatsRequestType.WaterType;

		public WaterTypeHandler(RoboatsRepository roboatsRepository, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
		}

		public override string ErrorMessage => $"ERROR. request={Request}&show={RequestDto.RequestSubType}";

		public override string Execute()
		{
			if(RequestDto.RequestSubType != "quantity")
			{
				return ErrorMessage;
			}

			var waterTypes = _roboatsRepository.GetWaterTypes();
			if(!waterTypes.Any())
			{
				return ErrorMessage;
			}

			return string.Join('|', waterTypes.Select(x => x.RoboatsId));
		}
	}

}
