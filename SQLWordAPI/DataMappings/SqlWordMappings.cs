using SQLWordAPI.Models;
using SQLWordAPI.ResourceModels;

namespace SQLWordAPI.DataMappings
{
    public static class SqlWordMappings
    {
        public static List<SqlWordResource> DtoToSqlWordResource(this IEnumerable<SqlWordDto> model)
        {
            if (model != null && model.Any())
            {
                List<SqlWordResource> sqlWordResources;

                sqlWordResources = model.Select(sqlWord => new SqlWordResource
                {
                    Id = sqlWord.Id!.Trim(),
                    SqlWord = sqlWord.SqlWord.Trim(),
                    DateCreated = sqlWord.DateCreated.ToString("MM/dd/yyyy hh:mm:ss tt").Trim(),
                    DateUpdated = sqlWord.DateUpdated.ToString("MM/dd/yyyy hh:mm:ss tt").Trim()
                }).ToList();

                return sqlWordResources;
            }

            return new();
        }

        public static SqlWordDto SaveSqlWordResourceToDto(this SaveSqlWordResource model, Guid? sqlWordId)
        {
            if (model != null)
            {
                SqlWordDto sqlWordDto;

                sqlWordDto = new SqlWordDto
                {
                    Id = sqlWordId.ToString()!.Trim(),
                    SqlWord = model.SqlWord.Trim()
                };

                return sqlWordDto;
            }

            return new();
        }
    }
}
