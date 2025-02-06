using SQLWordAPI.Models;

namespace SQLWordAPI.Repositories
{
    public interface ISqlWordRepository
    {
        Task<BaseResult<IEnumerable<SqlWordDto>>> GetSqlWordsAsync(Guid? id);
        Task<BaseResult> DeleteSqlWordAsync(Guid id);
        Task<BaseResult> SaveSqlWordAsync(SqlWordDto model);
        Task<BaseResult> CheckSqlWordExistsAsync(Guid? id);
    }
}
