using System;

namespace TaxcomEdo.Contracts.Responses
{
	public class TaxcomResponse
	{
		public bool Ok { get; set; }
		public string ErrorMessage { get; set; }

		protected void SetOk()
		{
			Ok = true;
			ErrorMessage = null;
		}
		
		protected void SetError(string error)
		{
			Ok = false;
			ErrorMessage = error;
		}
		
		public static TaxcomResponse Success()
		{
			var response = Create();
			response.SetOk();
			
			return response;
		}

		public static TaxcomResponse Error(string error)
		{
			var response = Create();
			response.SetError(error);
			
			return response;
		}

		public static TaxcomResponse Create() => new TaxcomResponse();
	}

	public class TaxcomResponse<T> : TaxcomResponse
	{
		public T Result { get; set; }

		protected void SetOk(T result)
		{ 
			Result = result;
			SetOk();
		}

		public static TaxcomResponse<T> Success(T result)
		{
			var response = new TaxcomResponse<T>();
			response.SetOk(result);
			
			return response;
		}
		
		public new static TaxcomResponse<T> Error(string error)
		{
			var response = new TaxcomResponse<T>();
			response.SetError(error);
			
			return response;
		}
	}
}
