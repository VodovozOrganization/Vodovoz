using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace EmailDebtNotificationWorker.Services.Common.Generators
{
	public class EmailBodyGenerator : IEmailBodyGenerator
	{
		public string GenerateClaimEmailBody(
			Counterparty client,
			CounterpartyContract contract,
			decimal debt,
			string unsubscribeUrl)
		{
			return $@"
				<p>Добрый день!</p>
				<p>Информируем, что у Вашей компании {client.FullName} (ИНН: {client.INN}) образовалась просроченная задолженность по договору {contract.Number} от {contract.IssueDate:dd.MM.yyyy} на сумму {debt:F2} руб.</p> 
				<p>Настоятельно рекомендуем принять участие в мирном урегулировании данного вопроса, что позволит обеим сторонам сэкономить время и деньги.</p>
				<p>И позволит продолжить дальнейшее плодотворное сотрудничество наших компаний!</p>
				<p>______________</p>
				<p>Вы всегда можете отписаться от нашей рассылки, нажав соответствующую кнопку.</p>
				<p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
				<p><em>Это письмо отправлено автоматически.</em></p>";
		}

		public string GenerateClosingDeliveriesEmailBody(
			decimal debt,
			int daysBeforeClosingDeliveries,
			string unsubscribeUrl)
		{
			return $@"
				<p>Уважаемый клиент!</p>
				<p>
					Сообщаем, что на текущий момент по договору имеется просроченная задолженность. 
					В связи с тем, что срок просрочки превышает {daysBeforeClosingDeliveries} дней, мы вынуждены временно ограничить возможность оформления заказов по безналичному расчету до полного погашения задолженности.
				</p>
				<p><strong>Общая сумма задолженности по указанным заказам составляет {debt} руб.</strong></p>
				<p>
					Просим вас произвести оплату в ближайшее время. Счет на оплату приложен к письму.
				</p>
				<p>
					Обращаем внимание: в случае отсутствия оплаты в течение 2 дней в ваш адрес будет направлена претензия. 
					Если оплата уже произведена, пожалуйста, направьте платежное поручение в ответ на данное письмо.
				</p>
				<p>
					После поступления оплаты поставки будут возобновлены в полном объеме.
				</p>
				<p>
					Если у вас возникнут вопросы по сумме задолженности или срокам оплаты, пожалуйста, свяжитесь с нами — мы будем рады помочь.
				</p>
				<p>
					Благодарим за сотрудничество и рассчитываем на скорейшее урегулирование вопроса.
				</p>
				<p>
					С уважением,
				</p>
				<p>
				<strong>
					Отдел сопровождения клиентов
				</strong>
				</p>
				<p>
					+7 (812) 317-00-00, доб. 700
				</p>
				<p>
					client.buh@vodovoz-spb.ru
				</p>
				<p style='text-align: right; font-size: 11px; margin-top: 20px;'>
					<a href='{unsubscribeUrl}' style='font-size: 11px; color: #999; text-decoration: underline;'>
						Отписаться от рассылки
					</a>
				</p>";
		}

		public string GenerateDebtEmailBody(
			Counterparty client,
			IList<(Order Order, decimal Debt)> ordersWithDebt,
			Dictionary<int, string> documentNumbersDict,
			string unsubscribeUrl)
		{
			var ordersHtml = new StringBuilder();
			decimal totalDebt = 0;

			foreach(var (order, debt) in ordersWithDebt)
			{
				var deliveryDate = order.DeliveryDate ?? DateTime.Today;
				var dueDate = deliveryDate.AddDays(client.DelayDaysForBuyers);
				var daysOverdue = (DateTime.Today - dueDate).Days;
				var orderDebt = debt;

				totalDebt += orderDebt;

				var documentNumber = documentNumbersDict.GetValueOrDefault(order.Id);
				if(string.IsNullOrWhiteSpace(documentNumber))
				{
					documentNumber = order.Id.ToString();
				}

				ordersHtml.AppendLine($@"
            <tr>
                <td style='padding: 8px 0;'>№ {documentNumber}</td>
                <td style='padding: 8px 0; text-align: right;'>{orderDebt:N2} руб.</td>
                <td style='padding: 8px 0; text-align: center;'>{daysOverdue}</td>
            </tr>");
			}

			string organizationName = ordersWithDebt
				.FirstOrDefault()
				.Order?.Contract?.Organization?.FullName ?? "Не указана";

			return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; }}
                .content {{ margin: 20px 0; }}
                .debt-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                .debt-table th {{ background-color: #e9ecef; padding: 10px; text-align: left; }}
                .debt-table td {{ border-bottom: 1px solid #dee2e6; }}
                .total-row {{ font-weight: bold; background-color: #f8f9fa; }}
                .footer {{ margin-top: 30px; padding-top: 15px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
                .unsubscribe {{ color: #007bff; text-decoration: none; }}
                .phone {{ white-space: nowrap; }}
                .signature {{ margin-top: 20px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Уважаемый клиент!</h2>
                </div>

                <div class='content'>
                    <p>На данный момент у вас имеется задолженность перед <strong>{organizationName}</strong> по следующим заказам:</p>
            
                    <table class='debt-table'>
                        <thead>
                            <tr>
                                <th>№ заказа</th>
                                <th>Сумма заказа</th>
                                <th>Дней после истечения отсрочки</th>
                            </tr>
                        </thead>
                        <tbody>
                            {ordersHtml}
                        </tbody>
                        <tfoot>
                            <tr class='total-row'>
                                <td style='padding: 10px 0;'><strong>Общая задолженность:</strong></td>
                                <td style='padding: 10px 0; text-align: right;'><strong>{totalDebt:N2} руб.</strong></td>
                                <td style='padding: 10px 0;'></td>
                            </tr>
                        </tfoot>
                    </table>

                    <p>Просим оплатить задолженность в ближайшее время. Если вы уже произвели оплату, пожалуйста, направьте подтверждение платежа.</p>
            
                    <p>Обращаем внимание: если просрочка по оплате превысит 7 календарных дней, 
                        поставки продукции будут приостановлены до полного погашения долга в соответствии с условиями договора. 
                        После поступления оплаты поставки будут возобновлены.</p>
            
                    <p>Если у вас есть вопросы по сумме или срокам оплаты, свяжитесь с нами - мы будем рады помочь.</p>
            
                    <div class='signature'>
                        <p>С уважением,<br />
                        Отдел сопровождения клиентов<br />
                        <span class='phone'>+7(812) 3170000 доб. 700</span><br />
                        client.buh@vodovoz-spb.ru</p>
                    </div>
                </div>

                <div class='footer'>
                    <p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
                    <p>Вы можете отказаться от рассылки, воспользовавшись соответствующей ссылкой в письме.</p>
                    <p><em>Это письмо отправлено автоматически.</em></p>
                </div>
            </div>
        </body>
        </html>";
		}
	}
}
