
using System.ComponentModel.DataAnnotations;

namespace Pacs.Server
{
	public enum OperatorBreakType
	{
		[Display(Name = "Большой")]
		Long,
		[Display(Name = "Малый")]
		Short
	}
}
