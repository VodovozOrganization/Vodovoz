namespace Pacs.Server
{
	public interface IOperatorBreakController
	{
		bool CanStartBreak { get; }

		void EndBreak(int operatorId);
		void StartBreak(int operatorId);
	}
}