using System;

namespace Vodovoz.Attributes
{
	/// <summary>
	/// При смене версии языка удалить класс, сейчас не поддерживается интерполяция в тегах аттрибутов
	/// </summary>
	public class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
	{
		/// <summary>
		/// Конструктор для типов, которые имеют MaxValue 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="minimum"></param>
		public RangeAttribute(Type type, string minimum) : base(type, minimum, type.GetField("MaxValue").GetValue(null).ToString())
		{
		}

		public RangeAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
		{
		}

		public RangeAttribute(int minimum, int maximum) : base(minimum, maximum)
		{
		}
	}
}
