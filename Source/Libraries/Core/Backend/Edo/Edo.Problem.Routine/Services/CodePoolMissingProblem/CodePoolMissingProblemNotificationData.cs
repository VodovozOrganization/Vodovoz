namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public class CodePoolMissingProblemNotificationData
	{
		public int OrderId { get; }
		public string Gtin { get; }
		public string NomenclatureName { get; }
		public int ProblemId { get; }

		public CodePoolMissingProblemNotificationData(
			int orderId,
			string gtin,
			string nomenclatureName,
			int problemId)
		{
			OrderId = orderId;
			Gtin = gtin;
			NomenclatureName = nomenclatureName;
			ProblemId = problemId;
		}
	}
}
