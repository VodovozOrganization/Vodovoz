namespace YooKassaApi.Library.Models
{
	public static class YooKassaCancellationReason
	{
		public const string GeneralDecline = "general_decline";
		public const string InsufficientFunds = "insufficient_funds";
		public const string RejectedByPayee = "rejected_by_payee";
		public const string RejectedByTimeout = "rejected_by_timeout";
		public const string YooMoneyAccountClosed = "yoo_money_account_closed";
		public const string PaymentArticleNumberNotFound = "payment_article_number_not_found";
		public const string PaymentBasketIdNotFound = "payment_basket_id_not_found";
		public const string PaymentTruCodeNotFound = "payment_tru_code_not_found";
		public const string SomeArticlesAlreadyRefunded = "some_articles_already_refunded";
		public const string TooManyRefundingArticles = "too_many_refunding_articles";
	}
}
