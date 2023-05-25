using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionGateway
{
    public class VATDBContext : IVATDBContext
    {
        private IConfiguration _config;
        private IDbConnection _connection;

        public VATDBContext(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection GetConnection()
        {
            _connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return _connection;
        }
    }
}
