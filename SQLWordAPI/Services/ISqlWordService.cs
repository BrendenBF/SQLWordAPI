using SQLWordAPI.ResourceModels;

namespace SQLWordAPI.Services
{
    public interface ISqlWordService
    {
        Task<ResponseResult<IEnumerable<SqlWordResource>>> GetSqlWordsAsync(Guid? id);
        Task<ResponseResult> DeleteSqlWordAsync(Guid id);
        Task<ResponseResult> SaveSqlWordAsync(SaveSqlWordResource saveSqlWordResource, Guid? id = null);
        Task<ResponseResult<String>> RemoveSensitiveWords(string sentance);
    }
}
