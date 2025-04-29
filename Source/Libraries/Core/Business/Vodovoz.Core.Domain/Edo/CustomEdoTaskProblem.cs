using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class CustomEdoTaskProblem : EdoTaskProblem
	{
		private string _customMessage;

		[Display(Name = "Произвольное описание")]
		public virtual string CustomMessage
		{
			get => _customMessage;
			set => SetField(ref _customMessage, value);
		}
	}
}
