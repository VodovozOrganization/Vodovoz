using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblemValidatorSourceEntity : EdoTaskProblemDescriptionSourceEntity
	{
		public override EdoTaskProblemType Type =>
			EdoTaskProblemType.Validation;

		public virtual string Message { get; set; }

		public override bool Equals(object obj)
		{
			return obj is EdoTaskProblemValidatorSourceEntity entity &&
				   base.Equals(obj) &&
				   Message == entity.Message;
		}

		public override int GetHashCode()
		{
			int hashCode = -750736358;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
			return hashCode;
		}

		public static bool operator ==(EdoTaskProblemValidatorSourceEntity left, EdoTaskProblemValidatorSourceEntity right)
		{
			return EqualityComparer<EdoTaskProblemValidatorSourceEntity>.Default.Equals(left, right);
		}

		public static bool operator !=(EdoTaskProblemValidatorSourceEntity left, EdoTaskProblemValidatorSourceEntity right)
		{
			return !(left == right);
		}
	}
}
