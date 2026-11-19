using System;
using System.Collections.Generic;
using System.Globalization;
using Mindbox.Library.Contracts;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Формирует идентификаторы клиента и заказа по контуру Mindbox
	/// </summary>
	public static class MindboxIdentifierBuilder
	{
		/// <summary>
		/// Формирует идентификаторы клиента
		/// </summary>
		/// <param name="settings">Настройки Mindbox</param>
		/// <param name="counterpartyId">Идентификатор контрагента ERP</param>
		/// <returns>Словарь идентификаторов клиента</returns>
		public static Dictionary<string, string> BuildCustomerIds(IMindboxSettings settings, int counterpartyId)
		{
			if(settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			return new Dictionary<string, string>(StringComparer.Ordinal)
			{
				[settings.CustomerIdName] = counterpartyId.ToString(CultureInfo.InvariantCulture)
			};
		}

		/// <summary>
		/// Формирует идентификаторы заказа
		/// </summary>
		/// <param name="settings">Настройки Mindbox</param>
		/// <param name="onlineOrderId">Идентификатор онлайн-заказа ERP</param>
		/// <returns>Словарь идентификаторов заказа</returns>
		public static Dictionary<string, string> BuildOrderIds(IMindboxSettings settings, int onlineOrderId)
		{
			if(settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			return new Dictionary<string, string>(StringComparer.Ordinal)
			{
				[settings.OrderIdName] = onlineOrderId.ToString(CultureInfo.InvariantCulture)
			};
		}

		/// <summary>
		/// Заполняет идентификаторы клиента с учётом контура
		/// </summary>
		/// <param name="ids">Контракт идентификаторов</param>
		/// <param name="settings">Настройки Mindbox</param>
		/// <param name="counterpartyId">Идентификатор контрагента ERP</param>
		public static void FillCustomerIds(MindboxCustomerIds ids, IMindboxSettings settings, int counterpartyId)
		{
			if(ids is null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if(settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			var value = counterpartyId.ToString(CultureInfo.InvariantCulture);

			if(settings.IsTest)
			{
				ids.ErpTestId = value;
				ids.ExternalId = null;
			}
			else
			{
				ids.ExternalId = value;
				ids.ErpTestId = null;
			}
		}
	}
}
