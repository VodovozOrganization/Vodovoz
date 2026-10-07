using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Accounting;

namespace Vodovoz.Core.Domain.Rules.Edo
{
	/// <summary>
	/// Класс для хранения правила по обработке и отправке ЭДО по дате закрытия бухгалтерской отчетности
	/// </summary>
	public class CanProcessOrSendEdoByClosingAccountingDate
	{
		private readonly IAccountingSettings _accountingSettings;

		public CanProcessOrSendEdoByClosingAccountingDate(IAccountingSettings accountingSettings)
		{
			_accountingSettings = accountingSettings ?? throw new ArgumentNullException(nameof(accountingSettings));
		}
		
		/// <summary>
		/// Получение выражения с правилом
		/// </summary>
		/// <typeparam name="TTask">Обобщенный тип ЭДО таски</typeparam>
		/// <returns></returns>
		public Expression<Func<TTask, bool>> GetRuleExpression<TTask>()
			where TTask : OrderEdoTask
		{
			var checkingDate = GetCheckingDate();
			return edoTask => edoTask.FormalEdoRequest.Order.DeliveryDate >= checkingDate;
		}

		/// <summary>
		/// Получение выражения с правилом по заявке на ЭДО <see cref="FormalEdoRequest"/>
		/// </summary>
		/// <returns></returns>
		public Expression<Func<FormalEdoRequest, bool>> GetRuleExpressionByEdoRequest()
		{
			var checkingDate = GetCheckingDate();
			return edoRequest => edoRequest.Order.DeliveryDate >= checkingDate;
		}

		/// <summary>
		/// Получение выражения с правилом по задаче на ЭДО <see cref="OrderEdoTask"/>
		/// </summary>
		/// <returns></returns>
		public Expression<Func<OrderEdoTask, bool>> GetRuleExpressionByEdoTask()
		{
			var checkingDate = GetCheckingDate();
			return edoTask => edoTask.FormalEdoRequest.Order.DeliveryDate >= checkingDate;
		}

		/// <summary>
		/// Получение nhibernate условия с правилом
		/// </summary>
		/// <returns></returns>
		public ICriterion GetRuleOrderCriterion()
		{
			OrderEntity orderAlias = null;
			
			var checkingDate = GetCheckingDate();
			return Restrictions.Where(() => orderAlias.DeliveryDate >= checkingDate);
		}

		/// <summary>
		/// Проверка правила по заявке на ЭДО
		/// </summary>
		/// <param name="edoRequest">Заявка на ЭДО <see cref="FormalEdoRequest"/></param>
		/// <returns></returns>
		public bool Check(FormalEdoRequest edoRequest)
		{
			var closestDate = GetCheckingDate();
			return edoRequest.Order.DeliveryDate >= closestDate;
		}
		
		/// <summary>
		/// Проверка правила по заказу
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns></returns>
		public bool Check(OrderEntity order)
		{
			var closestDate = GetCheckingDate();
			return order.DeliveryDate >= closestDate;
		}

		private DateTime GetCheckingDate()
		{
			var today = DateTime.Today;
			var accountingClosingDates = _accountingSettings.GetAccountingPeriodClosingDates();
			DateTime closestDate;
			
			var firstDate = accountingClosingDates.FirstOrDefault();
			
			//для начального месяца подбираем конец прошлого года
			if(today.Month <= firstDate.Month
				&& today.Day < firstDate.Day)
			{
				closestDate = GetMaxMonthDate(today, accountingClosingDates);
			}
			else
			{
				closestDate = GetClosestMonthDate(today, accountingClosingDates);
			}

			return closestDate;
		}

		private DateTime GetClosestMonthDate(DateTime today, IEnumerable<DateTime> accountingClosingDates)
		{
			DateTime? date = null;
			
			foreach(var accountingClosingDate in accountingClosingDates)
			{
				if(today.Day < accountingClosingDate.Day)
				{
					if(today.Month > accountingClosingDate.Month)
					{
						date = accountingClosingDate;
					}
				}
				else
				{
					if(today.Month >= accountingClosingDate.Month)
					{
						date = accountingClosingDate;
					}
				}
			}
			
			ThrowIfDateNull(date);
			return new DateTime(today.Year, date.Value.Month, 1);
		}

		private DateTime GetMaxMonthDate(DateTime today, IEnumerable<DateTime> accountingClosingDates)
		{
			DateTime? date = null;
			
			foreach(var accountingClosingDate in accountingClosingDates)
			{
				if(date is null || accountingClosingDate.Month > date.Value.Month)
				{
					date = accountingClosingDate;
				}
			}
			
			ThrowIfDateNull(date);
			return new DateTime(today.AddYears(-1).Year, date.Value.Month, 1);
		}

		private static void ThrowIfDateNull(DateTime? date)
		{
			if(!date.HasValue)
			{
				throw new InvalidOperationException("Не смогли подобрать дату закрытия бухгалтерского периода!");
			}
		}
	}
}
