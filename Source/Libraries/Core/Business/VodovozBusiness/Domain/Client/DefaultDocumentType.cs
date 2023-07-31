using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz.Domain.Client
{
	public enum DefaultDocumentType
	{
		[ItemTitle("УПД")]
		[Display(Name = "УПД")]
		upd,
		[ItemTitle("ТОРГ-12 + Счет-Фактура")]
		[Display(Name = "ТОРГ-12 + Счет-Фактура")]
		torg12
	}
}
