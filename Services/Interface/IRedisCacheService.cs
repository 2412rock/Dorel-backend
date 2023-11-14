namespace DorelAppBackend.Services.Interface
{
    public interface IRedisCacheService
    {
        public string GetValueFromCache(string key);

        public void SetValueInCache(string key, string value);

        public void RemoveValueFromCache(string key);
    }
}
