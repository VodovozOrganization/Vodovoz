using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core
{
	public class PromosetDuplicateFinder
	{
		private readonly IInteractiveService interactiveService;

		public PromosetDuplicateFinder(IInteractiveService interactiveService)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public bool RequestDuplicatePromosets(IUnitOfWork uow,  DeliveryPoint deliveryPoint, IEnumerable<Phone> phones)
		{
			if(phones == null) {
				throw new ArgumentNullException(nameof(phones));
			}

			IEnumerable<PromosetDuplicateInfoNode> deliveryPointResult = new List<PromosetDuplicateInfoNode>();
			if(deliveryPoint != null) {
				deliveryPointResult = GetDeliveryPointResult(uow, deliveryPoint);
			}
			var phoneResult = GetPhonesResult(uow, phones);

			if(!deliveryPointResult.Any() && !phoneResult.Any()) {
				return true;
			}

			string message = $"Найдены проданные промо-наборы по аналогичному адресу/телефону:{Environment.NewLine}";
			int counter = 1;
			foreach(var r in deliveryPointResult) {
				string date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}

			foreach(var r in phoneResult) {
				string date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}
			message += $"Продолжить сохранение?";

			return interactiveService.Question(message, "Найдены проданные промонаборы!");
		}

		private IEnumerable<PromosetDuplicateInfoNode> GetDeliveryPointResult(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			DeliveryPoint deliveryPointAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromosetDuplicateInfoNode resultAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias, () => !counterpartyPhoneAlias.IsArchive)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.Where(
					Restrictions.And(
						Restrictions.Where(() =>
							deliveryPointAlias.City == deliveryPoint.City
							&& deliveryPointAlias.Street == deliveryPoint.Street
							&& deliveryPointAlias.Building == deliveryPoint.Building
							&& deliveryPointAlias.Room == deliveryPoint.Room
						),
						Restrictions.Ge(Projections.Property(() => deliveryPointAlias.Room), 1)
					)
				)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)));

			var deliveryPointsResult = query.SelectList(list => list
				.SelectGroup(() => orderAlias.Id)
				.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
				.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
			).TransformUsing(Transformers.AliasToBean<PromosetDuplicateInfoNode>())
			.List<PromosetDuplicateInfoNode>();
			return deliveryPointsResult;
		}

		private IEnumerable<PromosetDuplicateInfoNode> GetPhonesResult(IUnitOfWork uow, IEnumerable<Phone> phones)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromosetDuplicateInfoNode resultAlias = null;

			var phonesArray = phones.Select(x => x.DigitsNumber).ToArray();
			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var counterpartyPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phonesArray),
				Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phonesArray),
				Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber),
				nullProjection
			);

			var counterpartyConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				counterpartyPhoneProjection,
				Projections.Constant(", ")
			);

			var deliveryPointConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				deliveryPointPhoneProjection,
				Projections.Constant(", ")
			);

			var concatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, ?2)"),
				NHibernateUtil.String,
				counterpartyConcatPhoneProjection,
				deliveryPointConcatPhoneProjection
			);

			var phoneResult = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.Where(
					Restrictions.Or(
						Restrictions.In(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phonesArray),
						Restrictions.In(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phonesArray)
					)
				)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(concatPhoneProjection).WithAlias(() => resultAlias.Phone)
				).TransformUsing(Transformers.AliasToBean<PromosetDuplicateInfoNode>())
			.List<PromosetDuplicateInfoNode>();
			return phoneResult;
		}

		private class PromosetDuplicateInfoNode
		{
			public DateTime? Date { get; set; }
			public string Client { get; set; }
			public string Address { get; set; }
			public string Phone { get; set; }
		}
	}
}
