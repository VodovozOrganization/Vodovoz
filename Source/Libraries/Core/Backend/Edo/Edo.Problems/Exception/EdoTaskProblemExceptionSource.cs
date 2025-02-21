using QS.DomainModel.Entity;
using Renci.SshNet.Messages;
using System;
using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception
{

	public abstract class EdoTaskProblemExceptionSource : EdoTaskProblemExceptionSourceEntity
	{
		public abstract override string Name { get; }
		public abstract override string Exception { get; }
		public abstract override string Description { get; }
		public abstract override string Recommendation { get; }
		public abstract override EdoProblemImportance Importance { get; }

		public virtual string GetTemplatedMessage(EdoTask edoTask)
		{
			return Exception;
		}
	}


	//public class EdoTaskProblemCustomSource : EdoTaskProblemDescriptionSource
	//{
	//	private string _message;

	//	/// <summary>
	//	/// Сообщение
	//	/// </summary>
	//	public virtual string Message
	//	{
	//		get => _message;
	//		set => SetField(ref _message, value);
	//	}
	//}

	//public abstract class EdoTaskProblemDescriptionSource : PropertyChangedBase
	//{
	//	private string _name;
	//	private string _description;
	//	private string _recommendation;
	//	private EdoProblemImportance _importance;

	//	/// <summary>
	//	/// Имя
	//	/// </summary>
	//	public virtual string Name
	//	{
	//		get => _name;
	//		set => SetField(ref _name, value);
	//	}

	//	/// <summary>
	//	/// Описание
	//	/// </summary>
	//	public virtual string Description
	//	{
	//		get => _description;
	//		set => SetField(ref _description, value);
	//	}

	//	/// <summary>
	//	/// Рекомендация
	//	/// </summary>
	//	public virtual string Recommendation
	//	{
	//		get => _recommendation;
	//		set => SetField(ref _recommendation, value);
	//	}

	//	/// <summary>
	//	/// Важность проблемы
	//	/// </summary>
	//	public virtual EdoProblemImportance Importance
	//	{
	//		get => _importance;
	//		set => SetField(ref _importance, value);
	//	}
	//}


}
