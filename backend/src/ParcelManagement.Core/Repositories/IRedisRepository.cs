namespace ParcelManagement.Core.Repositories
{
    public interface IRedisRepository
    {
        Task<bool> SetValueAsync(string key, string value, TimeSpan timeToLive);
        Task<string?> GetValueAsync(string key);
    }
}