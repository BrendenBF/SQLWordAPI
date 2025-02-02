using Dapper;
using SQLWordAPI.Constants;
using SQLWordAPI.Models;
using System.Data;

namespace SQLWordAPI.Repositories
{
    internal class SqlWordRepository :  BaseRepository, ISqlWordRepository
    {
        private readonly ILogger<SqlWordRepository> _logger;
        public SqlWordRepository(IConfiguration configuration, ILogger<SqlWordRepository> logger) : base(configuration)
        {
            _logger = logger;
        }

        public async Task<BaseResult<IEnumerable<SqlWordDto>>> GetSqlWordsAsync(Guid? Id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    IEnumerable<SqlWordDto> sqlwords = await conn.QueryAsync<SqlWordDto>(SPConstants.SelSqlWordList, new { Id = Id }, commandType: CommandType.StoredProcedure);
                    return new BaseResult<IEnumerable<SqlWordDto>>(msg: "Sql words successfully retrieved.") { Response = sqlwords.ToList() };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggingConstants.LogError, GetType(), ex.StackTrace, ex.Message);
                return new BaseResult<IEnumerable<SqlWordDto>>(errorMsg: $"Error: {ex.StackTrace} || {ex.Message}");
            }
        }

        public async Task<BaseResult> DeleteSqlWordAsync(Guid id)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    return await conn.ExecuteAsync(SPConstants.DelSqlWord, new { Id = id }) > 0
                            ? new BaseResult(msg: "Sql word deleted successfully.")
                            : new BaseResult(errorMsg: "Failed to delete sql word.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggingConstants.LogError, GetType(), ex.StackTrace, ex.Message);
                return new BaseResult(errorMsg: $"Error: {ex.Message}");
            }
        }

        public async Task<BaseResult> SaveSqlWordAsync(SqlWordDto model)
        {
            using (var conn = CreateConnection())
            {
                try
                {
                    var sqlWordId = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;

                    return await conn.ExecuteAsync(SPConstants.SaveSqlWord, new { Id = sqlWordId, SqlWord = model.SqlWord }, commandType: CommandType.StoredProcedure) > 0
                            ? new BaseResult(msg: "Sql word saved successfully.")
                            : new BaseResult(errorMsg: "Failed to save sql word.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LoggingConstants.LogError, GetType(), ex.StackTrace, ex.Message);
                    return new BaseResult(errorMsg: $"Error: {ex.Message}");
                }
            }
        }

        public async Task<BaseResult> CheckSqlWordExistsAsync(Guid? id)
        {
            using (var conn = CreateConnection())
            {
                try
                {
                    return await conn.ExecuteScalarAsync<bool>(SPConstants.SelSqlWordExists, new { Id = id }, commandType: CommandType.StoredProcedure)
                            ? new BaseResult(msg: "Sql word does exist.")
                            : new BaseResult(errorMsg: "Sql word does not exist in the system.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LoggingConstants.LogError, GetType(), ex.StackTrace, ex.Message);
                    return new BaseResult(errorMsg: $"Error: {ex.Message}");
                }
            }
        }
    }
}
