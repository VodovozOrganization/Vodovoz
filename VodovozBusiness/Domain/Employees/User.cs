using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase
	{
		public virtual string WarehouseAccess { get; set; }

		[Display(Name = "Требуется смена пароля")]
		bool needPasswordChange;
		public virtual bool NeedPasswordChange {
			get => needPasswordChange;
			set => SetField(ref needPasswordChange, value);
		}
	}
}

