using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Employees;

namespace VodovozBusiness.Services.Edo
{
	/// <summary>
	/// Создает ручные заявки на отправку документов клиенту по ЭДО.
	/// </summary>
	public class ManualEdoRequestFactory : IManualEdoRequestFactory
	{
		private readonly IEmployeeRepository _employeeRepository;

		public ManualEdoRequestFactory(
			IEmployeeRepository employeeRepository
		)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		/// <summary>
		/// Создает ручную заявку на отправку документов клиенту по ЭДО.
		/// </summary>
		/// <param name="order">Заказ, для которого создается заявка</param>
		/// <param name="productCodes">Коды маркировки, включаемые в заявку</param>
		/// <returns>Ручная заявка на отправку документов клиенту по ЭДО</returns>
		public ManualEdoRequest Create(
			IUnitOfWork uow,
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
				ProductCodes = new ObservableList<TrueMarkProductCode>(productCodes),
				Author = _employeeRepository.GetEmployeeForCurrentUser(uow)
			};
		}

		/// <summary>
		/// Создает ручную заявку на отправку документов клиенту по ЭДО без КМ
		/// </summary>
		/// <param name="order">Заказ, для которого создается заявка</param>
		/// <returns>Ручная заявка на отправку документов клиенту по ЭДО</returns>
		public ManualEdoRequest Create(
			IUnitOfWork uow,
			OrderEntity order
		)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			return new ManualEdoRequest
			{
				Type = CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				Author = _employeeRepository.GetEmployeeForCurrentUser(uow)
			};
		}
	}
}
