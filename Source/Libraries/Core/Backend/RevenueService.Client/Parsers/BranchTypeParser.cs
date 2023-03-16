using RevenueService.Client.Enums;
using System;
using Dadata.Model;

namespace RevenueService.Client.Parsers
{
	public class BranchTypeParsser
	{
		public BranchType Parse(PartyBranchType branchType)
		{
			switch(branchType)
			{
				case PartyBranchType.MAIN:
					return BranchType.Main;
				case PartyBranchType.BRANCH:
					return BranchType.Branch;
				default:
					throw new NotSupportedException($"{branchType} не поддерживается");
			}
		}
	}
}
