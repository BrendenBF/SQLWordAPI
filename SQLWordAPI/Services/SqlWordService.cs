using SQLWordAPI.Models;
using SQLWordAPI.Repositories;
using SQLWordAPI.ResourceModels;
using SQLWordAPI.DataMappings;
using Microsoft.Extensions.Caching.Memory;
using SQLWordAPI.Constants;
using System.Text.RegularExpressions;
using System.Text;

namespace SQLWordAPI.Services
{
    public class SqlWordService : ISqlWordService
    {
        private readonly ISqlWordRepository _sqlWordRepository;
        private readonly IMemoryCache _memoryCache;

        public SqlWordService(ISqlWordRepository sqlWordRepository, IMemoryCache cache)
        {
            _sqlWordRepository = sqlWordRepository;
            _memoryCache = cache;
        }

        /// <summary>
        /// This method is responsible for retuning SQL Words in an array - Either based on a unique identifier or if the identifier is not provided then it return the entire lot.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResourceResponse<IEnumerable<SqlWordResource>>> GetSqlWordsAsync(Guid? id)
        {
            BaseResult<IEnumerable<SqlWordDto>>? words;
            string cacheKey = id is not null ? string.Format(CacheKeyConstants.CacheKeySingle, id) : CacheKeyConstants.CacheKeyAll;

            if (!_memoryCache.TryGetValue(cacheKey, out words))
            {
                if (id is not null)
                {
                    var result = await _sqlWordRepository.CheckSqlWordExistsAsync(id);

                    if (!result.Success)
                        return new ResourceResponse<IEnumerable<SqlWordResource>>(result.ErrorMsg);
                }

                words = await _sqlWordRepository.GetSqlWordsAsync(id);
                _memoryCache.Set(cacheKey, words, GetCacheOptions);
            }

            return words!.Success ? new ResourceResponse<IEnumerable<SqlWordResource>>(words.Response?.DtoToSqlWordResource() ?? new List<SqlWordResource>()) { Message = words.Message } : new ResourceResponse<IEnumerable<SqlWordResource>>(words.ErrorMsg);
        }

        /// <summary>
        /// This method is responsbile for the deletion of a single SQL word based on a unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResourceResponse> DeleteSqlWordAsync(Guid id)
        {
            InvalidateCache(id);

            BaseResult result = await _sqlWordRepository.CheckSqlWordExistsAsync(id);

            if (!result.Success)
                return new ResourceResponse(result.ErrorMsg);

            result = await _sqlWordRepository.DeleteSqlWordAsync(id);
            return result.Success ? new ResourceResponse(result.Message) { Success = result.Success } : new ResourceResponse(result.ErrorMsg);
        }

        /// <summary>
        /// This method is responsbile for performing a upsert - If the record exists it updates it, if its is not found then it is deemed to be a insert query that gets executed.
        /// </summary>
        /// <param name="saveSqlWordResource"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResourceResponse> SaveSqlWordAsync(SaveSqlWordResource saveSqlWordResource, Guid? id = null)
        {
            InvalidateCache(id);

            if (id != null)
            {
                var checkResult = await _sqlWordRepository.CheckSqlWordExistsAsync(id);

                if (!checkResult.Success)
                    return new ResourceResponse(checkResult.ErrorMsg);
            }

            BaseResult result = await _sqlWordRepository.SaveSqlWordAsync(saveSqlWordResource.SaveSqlWordResourceToDto(id));
            return result.Success ? new ResourceResponse(result.Message) { Success = result.Success } : new ResourceResponse(result.ErrorMsg);
        }

        /// <summary>
        /// This method is used to remove sensitive words from a sentance against a predefined list of words loaded in the DB.
        /// </summary>
        /// <param name="sentance"></param>
        /// <returns></returns>
        public async Task<ResourceResponse<String>> RemoveSensitiveWords(string sentance)
        {
            var result = await GetSqlWordsAsync(null);
            var builder = new StringBuilder(sentance);

            result.Resource!.Select(x => x.SqlWord.ToLower()).ToList().ForEach(sqlWord =>
            {
                // Only check for matches if the regex finds a match case-insensitively
                if (Regex.IsMatch(builder.ToString(), $@"(?:^|\W){Regex.Escape(sqlWord)}(?:$|\W)", RegexOptions.IgnoreCase))
                {
                    // Replace the matching word with asterisks in the StringBuilder
                    int startIndex = 0;
                    while ((startIndex = builder.ToString().IndexOf(sqlWord, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        builder.Remove(startIndex, sqlWord.Length);
                        builder.Insert(startIndex, new string('*', sqlWord.Length));
                        startIndex += new string('*', sqlWord.Length).Length;
                    }
                }
            });

            return new ResourceResponse<String>(builder.ToString().Trim(), "Successfully transformed the string and removed the senentive characters where it was deemed applicable.");
        }

        /// <summary>
        /// Some in-memory cache configurations.
        /// </summary>
        private static MemoryCacheEntryOptions GetCacheOptions
        {
            get
            {
                return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetPriority(CacheItemPriority.Normal);
            }
        }

        /// <summary>
        /// Cache invalidation logic.
        /// </summary>
        /// <param name="id"></param>
        private void InvalidateCache(Guid? id)
        {
            if (id is not null)
                _memoryCache.Remove(string.Format(CacheKeyConstants.CacheKeySingle, id));

            _memoryCache.Remove(CacheKeyConstants.CacheKeyAll);
        }
    }
}
