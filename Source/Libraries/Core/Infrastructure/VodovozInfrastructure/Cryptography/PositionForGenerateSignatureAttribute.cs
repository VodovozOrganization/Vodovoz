using System;

namespace VodovozInfrastructure.Cryptography
{
	public class PositionForGenerateSignatureAttribute : Attribute
	{
		public PositionForGenerateSignatureAttribute(int position)
		{
			Position = position;
		}
		
		public int Position { get; }
	}
}
