using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.Edo
{
	/// <summary>
	/// Создает ручные заявки на отправку документов клиенту по ЭДО.
	/// </summary>
	public static class ManualEdoRequestFactory
	{
		public static ManualEdoRequest Create(
			OrderEntity order,
			IEnumerable<TrueMarkProductCode> productCodes)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(productCodes is null)
			{
				throw new ArgumentNullException(nameof(productCodes));
			}

			return new ManualEdoRequest
			{
				Type = CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				ProductCodes = new ObservableList<TrueMarkProductCode>(productCodes)
			};
		}
	}
}
