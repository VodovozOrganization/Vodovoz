using System;
using Vodovoz.Core.Domain.Cash;

namespace VodovozBusiness.Controllers.Cash
{
	public interface IVatRateVersionController
	{
		VatRateVersion CreateAndAddVersion(VatRateVersionType vatRateVersionType, DateTime? startDate = null);
		
		void ChangeVersionStartDate(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType);
		
		bool IsValidDateForNewVatRateVersion(DateTime dateTime, VatRateVersionType vatRateVersionType);
		
		bool IsValidDateForVersionStartDateChange(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType);

		///  <summary>
		///  Добавляет новую версию НДС в список версий организации или номенклатуры контроллера.
		///  Если предыдущая версия не имела дату окончания или заканчивалась позже даты начала новой версии,
		///  то в этой версии выставляется дата окончания, равная дате начала новой версии минус 1 миллисекунду
		///  </summary>
		///  <param name="vatRateVersionType">Тип ставки НДС (для какой сущности)</param>
		///  <param name="newVatRateVersion">Новая версия организации. Свойство StartDate в newOrganizationVersion игнорируется</param>
		///  <param name="startDate">
		/// 	Дата начала действия новой версии. Должна быть минимум на день позже, чем дата начала действия предыдущей версии.
		/// 	Время должно равняться 00:00:00
		///  </param>
		VatRateVersion AddNewVersion(VatRateVersionType vatRateVersionType, VatRateVersion newVatRateVersion, DateTime startDate);
	}
}
