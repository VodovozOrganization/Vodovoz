namespace BitrixApi.REST
{
    public static class BitrixRestApiFactory
    {
        public static IBitrixRestApi CreateBitrixRestApi(string token)
        {
            return new BitrixRestApi(token);
        }
    }
}