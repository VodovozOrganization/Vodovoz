using System.ComponentModel;

namespace TaxcomEdoApi.Library.Models
{
	public enum EmployeePermissionData
	{
		[Description("нет")] None = 0,
		[Description("Администратор")] Administrator = 1,
		[Description("Управление подразделениями")] ManageDepartment = 2,
		[Description("Использовать данные права для доступа к документам дочерних подразделений")] InheritToChild = 16, // 0x00000010
		[Description("Создавать документы")] CreateDocument = 32, // 0x00000020
		[Description("Согласовывать документы")] ApproveDocument = 64, // 0x00000040
		[Description("Подписывать документы")] SignDocument = 128, // 0x00000080
		[Description("Передавать на подпись и согласование")] SendDocumentToSign = 256, // 0x00000100
		[Description("Перемещать документы")] MoveDocument = 512, // 0x00000200
		[Description("Импортировать и создавать документы")] ImportCreateDocument = 1024, // 0x00000400
		[Description("Отправлять документы")] SendDocument = 4096, // 0x00001000
		All = -1, // 0xFFFFFFFF
	}
}
