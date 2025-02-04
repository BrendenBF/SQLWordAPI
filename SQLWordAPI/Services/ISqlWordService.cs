using SQLWordAPI.ResourceModels;

namespace SQLWordAPI.Services
{
    public interface ISqlWordService
    {
        Task<ResourceResponse<IEnumerable<SqlWordResource>>> GetSqlWordsAsync(Guid? id);
        Task<ResourceResponse> DeleteSqlWordAsync(Guid id);
        Task<ResourceResponse> SaveSqlWordAsync(SaveSqlWordResource saveSqlWordResource, Guid? id = null);
        Task<ResourceResponse<String>> RemoveSensitiveWords(string sentance);
    }
}
