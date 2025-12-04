using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	/// <summary>
	/// Типы доверенностей
	/// </summary>
	public enum ProxyDocumentType
	{
		[Display(Name = "Доверенность на ТС")]
		CarProxy,
		[Display(Name = "Доверенность M-2")]
		M2Proxy
	}
}
