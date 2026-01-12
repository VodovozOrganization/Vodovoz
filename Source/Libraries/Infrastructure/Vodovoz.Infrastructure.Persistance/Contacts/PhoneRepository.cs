using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using VodovozBusiness.Domain.Contacts;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Contacts
{
	internal sealed class PhoneRepository : IPhoneRepository
	{
		#region PhoneType

		public IList<PhoneType> GetPhoneTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType>().List<PhoneType>();
		}

		public PhoneType PhoneTypeWithPurposeExists(IUnitOfWork uow, PhonePurpose phonePurpose)
		{
			return uow.Session.QueryOver<PhoneType>()
				.Where(x => x.PhonePurpose == phonePurpose)
				.SingleOrDefault<PhoneType>();
		}

		#endregion

		#region Телефоны

		/// <summary>
		/// Возвращает список объектов которым принадлежит телефон
		/// </summary>
		/// <returns>Counterparty / DeliveryPoint / Employee.</returns>
		/// <param name="digitsPhone">Digits phone.</param>
		public object[] GetObjectByPhone(IUnitOfWork uow, string digitsPhone)
		{
			string number = digitsPhone.Substring(digitsPhone.Length - Math.Min(10, digitsPhone.Length));
			var sql = "SELECT * FROM phones WHERE digits_number = @phone";
			var list = uow.Session.Connection.Query(sql, new { phone = number });
			ArrayList _list = new ArrayList();
			foreach(var phone in list)
			{
				if(phone.counterparty_id != null)
				{
					Counterparty client = uow.GetById<Counterparty>((int)phone.counterparty_id);
					_list.Add(client);
				}
				if(phone.delivery_point_id != null)
				{
					DeliveryPoint deliveryPoint = uow.GetById<DeliveryPoint>((int)phone.delivery_point_id);
					_list.Add(deliveryPoint);
				}
				if(phone.employee_id != null)
				{
					Employee employee = uow.GetById<Employee>((int)phone.employee_id);
					_list.Add(employee);
				}
			}
			return _list.ToArray();
		}

		#endregion

		public IList<Phone> GetPhonesByNumber(IUnitOfWork uow, string digitsPhone)
		{
			return uow.Session.QueryOver<Phone>()
				.Where(p => p.DigitsNumber == digitsPhone)
				.And(p => p.Counterparty != null || p.DeliveryPoint != null)
				.And(p => !p.IsArchive)
				.List();
		}

		public IEnumerable<PhoneInfo> GetPhoneInfoByCounterpartiesIds(IUnitOfWork uow, IEnumerable<int> counterpartiesIds)
		{
			var result =
				from phone in uow.Session.Query<Phone>()
				where counterpartiesIds.Contains(phone.Counterparty.Id) && !phone.IsArchive
				select new PhoneInfo
				{
					ErpPhoneId = phone.Id,
					ErpCounterpartyId = phone.Counterparty.Id,
					Number = $"+7{phone.DigitsNumber}"
				};

			return result.ToList();
		}

		public bool PhoneNumberExists(IUnitOfWork unitOfWork, string phoneNumber, int? counterpartyId = null, int? deliveryPointId = null)
		{
			var phones = unitOfWork.Session.Query<Phone>()
				.Where(p => p.DigitsNumber == phoneNumber);

			if(counterpartyId.HasValue)
			{
				phones = phones.Where(p => p.Counterparty.Id == counterpartyId.Value);
			}

			if (deliveryPointId.HasValue)
			{
				phones = phones.Where(p => p.DeliveryPoint.Id == deliveryPointId.Value);
			}
			
			return phones.Any();
		}

		public IList<IncomingCallsAnalysisReportNode> GetLastOrderIdAndDeliveryDateByPhone(
			IUnitOfWork uow, IEnumerable<string> incomingCallsNumbers)
		{
			Phone phoneAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Order orderAlias = null;
			IncomingCallsAnalysisReportNode resultAlias = null;

			var query = uow.Session.QueryOver(() => phoneAlias)
				.Left.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => phoneAlias.DeliveryPoint, () => deliveryPointAlias)
				.Where(() => counterpartyAlias.Id != null || deliveryPointAlias.Id != null)
				.WhereRestrictionOn(() => phoneAlias.DigitsNumber).IsInG(incomingCallsNumbers);

			var lastOrderIdForCounterparty = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterpartyAlias.Id)
				.Select(o => o.Id)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);

			var lastOrderIdForDeliveryPoint = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(o => o.Id)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);

			var lastOrderDeliveryDateForCounterparty = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterpartyAlias.Id)
				.Select(o => o.DeliveryDate)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);

			var lastOrderDeliveryDateForDeliveryPoint = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(o => o.DeliveryDate)
				.OrderBy(o => o.DeliveryDate).Desc
				.Take(1);

			return query.SelectList(list => list
					.Select(() => phoneAlias.DigitsNumber).WithAlias(() => resultAlias.PhoneDigitsNumber)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.DeliveryPointId)
					.Select(Projections.Conditional(
						Restrictions.IsNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.SubQuery(lastOrderIdForDeliveryPoint),
						Projections.SubQuery(lastOrderIdForCounterparty))).WithAlias(() => resultAlias.LastOrderId)
					.Select(Projections.Conditional(
						Restrictions.IsNull(Projections.Property(() => counterpartyAlias.Id)),
						Projections.SubQuery(lastOrderDeliveryDateForDeliveryPoint),
						Projections.SubQuery(lastOrderDeliveryDateForCounterparty))).WithAlias(() => resultAlias.LastOrderDeliveryDate))
				.TransformUsing(Transformers.AliasToBean<IncomingCallsAnalysisReportNode>())
				.List<IncomingCallsAnalysisReportNode>();
		}
	}
}
