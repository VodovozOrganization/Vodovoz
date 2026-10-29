using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Пользовательский элемент проблемы, связанный с GTIN.
	/// </summary>
	public class EdoProblemGtinItem : EdoProblemCustomItem
	{
		private GtinEntity _gtin;

		public override EdoProblemCustomItemType Type => EdoProblemCustomItemType.Gtin;

		/// <summary>
		/// Сущность GTIN, связанная с проблемой.
		/// </summary>
		[Display(Name = "Gtin")]
		public virtual GtinEntity Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}
	}
}
