namespace BitrixApi.REST
{
    public static class BitrixRestApiFactory
    {
        public static IBitrixRestApi CreateBitrixRestApi(string userId, string token)
        {
            return new BitrixRestApi(userId, token);
        }
    }
}