using System.Data;
using Microsoft.Data.SqlClient;

namespace SQLWordAPI.Repositories
{
    internal abstract class BaseRepository
    {
        private readonly string _connectionString;
        protected BaseRepository(IConfiguration config)
        {
            IConfiguration configuration = config;
            _connectionString = configuration.GetConnectionString("SqlConnection")!;
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
