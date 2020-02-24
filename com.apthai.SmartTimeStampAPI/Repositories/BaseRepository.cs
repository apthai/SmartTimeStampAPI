using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace com.apthai.SmartTimeStampAPI.Repositories
{
    public abstract class BaseRepository
    {
        private readonly string _ConnectionString;
        private readonly string _LogConnectionString;
        private readonly IConfiguration _config;
        protected int ConnctionLongQuerryTimeOut = 600;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BaseRepository(IHostingEnvironment environment, IConfiguration config)
        {
            _config = config;
            _hostingEnvironment = environment;
            _ConnectionString = Environment.GetEnvironmentVariable("DefaultConnection");
            if(_ConnectionString==null || _ConnectionString == "")
                _ConnectionString = _config.GetConnectionString("DefaultConnection");

        }
        protected IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_ConnectionString);
            }
        }
        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (IDbConnection connection = Connection)
                {
                    connection.Open(); //.OpenAsync(); // Asynchronously open a connection to the database
                    return await getData(connection); // Asynchronously execute getData, which has been passed in as a Func<IDBConnection, Task<T>>
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName), ex);
            }
        }
    }
}
