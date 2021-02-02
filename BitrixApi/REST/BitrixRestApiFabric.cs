namespace BitrixApi.REST
{
    public class BitrixRestApiFabric
    {
        public static IBitrixRestApi CreateBitrixRestApi(string token)
        {
            return new BitrixRestApi(token);
        }
    }
}