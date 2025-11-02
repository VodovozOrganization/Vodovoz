using MangoService;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class SearchTableEntity
	{
		public string Name { get; set; }
		public string Department { get; set; }
		public string Extension { get; set; }
		public bool IsReady { get; set; }
		public bool IsGroup { get; set; }

		public SearchTableEntity(PhoneEntry phoneEntry)
		{
			Name = phoneEntry.Name;
			Department = phoneEntry.Department;
			Extension = phoneEntry.Extension.ToString();
			IsReady = phoneEntry.PhoneState == PhoneState.Ready;
			IsGroup = phoneEntry.PhoneType == PhoneEntryType.Group;
		}
	}
}
