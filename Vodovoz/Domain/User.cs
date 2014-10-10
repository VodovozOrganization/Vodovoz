using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Пользователи системы")]
	public class User
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Login { get; set; }
		#endregion

		public User()
		{
			Name = String.Empty;
			Login = String.Empty;
		}
	}
}

