using System;
namespace Mango.Client.DTO.Group
{
	public class GroupRequest
	{
		public int? group_id { get; set; }
		public int? operator_id { get; set; }
		public string operator_extension { get; set; }
		public int? show_users { get; set; }
	}
}
