using System.Collections.Generic;
using Vodovoz.Domain.Payments;
using QS.DomainModel.UoW;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Orders;
using System.Linq;
using System.Text;

namespace Vodovoz.ViewModels
{
	public class AutoPaymentMatching
	{
		readonly IUnitOfWork UoW;

		public AutoPaymentMatching(IUnitOfWork uow)
		{
			UoW = uow;
		}

		public bool IncomePaymentMatch(Payment payment)
		{
			StringBuilder sb = new StringBuilder();
			List<Order> orders = new List<Order>();

			if(payment.Counterparty == null)
			{
				return false;
			}

			var str = CheckPaymentPurpose(payment);

			if(str.Any())
			{
				foreach(string st in str)
				{
					var order = UoW.GetById<Order>(int.Parse(st));

					if(order == null)
					{
						return false;
					}

					orders.Add(order);
				}

				var result = orders.Sum(x => x.ActualTotalSum);

				if(payment.Total != result)
				{
					return false;
				}

				if(payment.Total == result)
				{
					foreach(var order in orders)
					{
						if(order.OrderPaymentStatus != OrderPaymentStatus.UnPaid)
						{
							return false;
						}
						else
						{
							payment.AddPaymentItem(order);
							sb.AppendLine(order.Id.ToString());
						}
					}
				}
			}
			else
			{
				return false;
			}

			payment.NumOrders = sb.ToString().TrimEnd(new[] { '\r', '\n' });
			return true;
		}

		private string[] CheckPaymentPurpose(Payment payment)
		{
			string pattern = @"([0-9]{6,7})";

			var matches = Regex.Matches(payment.PaymentPurpose, pattern);
			string[] str = new string[matches.Count];

			for(int i = 0; i < matches.Count; i++) {
				str[i] = matches[i].Groups[1].Value;
			}

			return str;
		}
	}
}
