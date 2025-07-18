namespace Vodovoz.Core.Domain.SecureCodes
{
	public static class SecureCodeEmailHtmlTemplate
	{
		public static string GetTemplate(string code, string email, int codeLifeTimeMinutes)
		{
			return
				"<html lang=\"ru\">" +
				@"<head>" +
				"  <meta charset=\"UTF - 8\" />" +
				"  <meta name=\"viewport\" content=\"width = device - width, initial - scale = 1.0\" />" +
				@"  <title>Подтверждение почты</title>" +
				@"  <style>" +
				@"    body {" +
				@"      margin: 0;" +
				@"      padding: 40px;" +
				@"      background-color: #b2d8f7;" +
				@"      font-family: Arial, sans-serif;" +
				@"    }" +
				@"    .container {" +
				@"      max-width: 420px;" +
				@"      margin: 20px auto;" +
				@"      background-color: #ffffff;" +
				@"      border-radius: 12px;" +
				@"      padding: 24px;" +
				@"      text-align: center;" +
				@"      box-shadow: 0 0 8px rgba(0,0,0,0.1);" +
				@"    }" +
				@"    .logo {" +
				@"      font-family: 'Comic Sans MS', cursive, sans-serif;" +
				@"      font-size: 24px;" +
				@"      font-weight: bold;" +
				@"      color: #ff6600;" +
				@"      margin-bottom: 24px;" +
				@"    }" +
				@"    .message {" +
				@"      font-size: 16px;" +
				@"      color: #000000;" +
				@"      margin-bottom: 16px;" +
				@"    }" +
				@"    .email {" +
				@"      color: #0077cc;" +
				@"      text-decoration: none;" +
				@"    }" +
				@"    .code {" +
				@"      font-size: 32px;" +
				@"      font-weight: bold;" +
				@"      color: #0077cc;" +
				@"      margin: 16px 0;" +
				@"    }" +
				@"    .expiration {" +
				@"      font-size: 14px;" +
				@"      color: #444444;" +
				@"      margin-top: 8px;" +
				@"    }" +
				@"    .footer {" +
				@"      font-size: 12px;" +
				@"      color: #555555;" +
				@"      margin-top: 32px;" +
				@"    }" +
				@"  </style>" +
				@"</head>" +
				@"<body>" +
				"  <div class=\"container\">" +
				"    <div class=\"logo\">Весёлый водовоз</div>" +
				"    <div class=\"message\">" +
				@"      Подтвердите электронную почту" +
				$"      <a class=\"email\" href=\"mailto: {email}\">{email}</a><br />" +
				@"      с помощью кода:" +
				@"    </div>" +
				$"    <div class=\"code\">{code}</div>" +
				$"    <div class=\"expiration\">Срок действия кода – {codeLifeTimeMinutes} минут</div>" +
				"    <div class=\"footer\">" +
				"      ООО \"Весёлый Водовоз\"<br />" +
				@"      Это письмо отправлено автоматически.<br />" +
				@"      Пожалуйста, не отвечайте на него." +
				@"    </div>" +
				@"  </div>" +
				@"</body>" +
				@"</html>";
		}
	}
}
