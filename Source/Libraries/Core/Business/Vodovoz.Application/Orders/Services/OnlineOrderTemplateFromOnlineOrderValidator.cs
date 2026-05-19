using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderTemplateFromOnlineOrderValidator : IOnlineOrderTemplateFromOnlineOrderValidator
	{
		private IList<Error> _validationResults;
		
		public Result Validate(OnlineOrder onlineOrder, CreatingOrderTemplate creatingTemplate)
		{
			if(creatingTemplate is null)
			{
				return Result.Success();
			}
			
			_validationResults = new List<Error>();

			if(onlineOrder.Counterparty is null)
			{
				_validationResults.Add(OnlineOrderErrors.IsEmptyCounterparty);
			}

			if(onlineOrder.DeliveryPoint is null)
			{
				_validationResults.Add(OnlineOrderErrors.IsEmptyDeliveryPoint);
			}

			if(onlineOrder.DeliverySchedule is null)
			{
				_validationResults.Add(OnlineOrderErrors.IsEmptyDeliverySchedule);
			}
			
			return !_validationResults.Any() ? Result.Success() : Result.Failure(_validationResults);
		}
	}
}
