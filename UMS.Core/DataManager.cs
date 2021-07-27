using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using UMS.Dto;

namespace UMS.Core
{

    public class DataManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private string _compName;
        private string _connectionString;
        private CurrentUser currentUser;

        public DataManager(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            currentUser = (CurrentUser)_httpContextAccessor.HttpContext.Items["User"];
        }

        public enum ExecutionType
        {
            Reader,
            NonQuery,
            Scaler
        }



        #region NewConnection
        private void GetConnectionString(string compName = null)
        {
            if (compName != null)
            {
                _compName = compName;
            }
            else if (currentUser != null)
            {
                _compName = currentUser.CompName;
            }
            else
            {
                _compName = "ind";
            }
            var key = _config["Key"];
            var connStr = Cryptography.Decrypt(_config.GetConnectionString(_compName), key);
            _connectionString = connStr + ";" + "Connect Timeout=0";

        }
        #endregion




        public async Task<object> ExecuteReaderWithSP(string procedure, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);

            return await ExecuteAsync<object>(ExecutionType.Reader, CommandType.StoredProcedure, procedure, parameters);
        }

        public async Task<object> ExecuteScalarWithSP(string procedure, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);
            return await ExecuteAsync<object>(ExecutionType.Scaler, CommandType.StoredProcedure, procedure, parameters);
        }
        public async Task<int> ExecuteNonQueryWithSP(string procedure, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);
            return (int)await ExecuteAsync<object>(ExecutionType.NonQuery, CommandType.StoredProcedure, procedure, parameters);
        }

        public async Task<object> ExecuteReaderWithQuery(string query, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);

            return await ExecuteAsync<object>(ExecutionType.Reader, CommandType.Text, query, parameters);
        }

        public async Task<object> ExecuteScalarWithQuery(string query, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);
            return await ExecuteAsync<object>(ExecutionType.Scaler, CommandType.Text, query, parameters);
        }
        public async Task<int> ExecuteNonQueryWithQuery(string query, IEnumerable<ViewParam> parameters, string compName = null)
        {
            GetConnectionString(compName);
            return (int)await ExecuteAsync<object>(ExecutionType.NonQuery, CommandType.Text, query, parameters);
        }


        private async Task<object> ExecuteAsync<T>(ExecutionType executionType, CommandType commandType, string commandText, IEnumerable<ViewParam> parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(commandText, connection) { CommandType = commandType })
                {
                    foreach (var vp in parameters) command.Parameters.AddWithValue("@" + vp.Name.Trim(), vp.Value);
                    await connection.OpenAsync().ConfigureAwait(false);
                    command.CommandTimeout = 0;

                    try
                    {
                        object result;
                        switch (executionType)
                        {
                            case ExecutionType.Reader:
                                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                                using (reader)
                                {

                                    List<object> parentList = new();
                                    do
                                    {
                                        List<object> childList = new();
                                        while (reader.Read())
                                        {
                                            IDictionary<string, object> data = new ExpandoObject();

                                            for (int inc = 0; inc < reader.FieldCount; inc++)
                                            {
                                                data.Add(reader.GetName(inc), reader.GetValue(inc));
                                            }

                                            childList.Add(data);
                                        }
                                        parentList.Add(childList);
                                    }
                                    while (reader.NextResult());
                                    result = parentList;
                                }
                                break;
                            case ExecutionType.NonQuery:
                                result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                                break;
                            default:
                                result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                                break;
                        }
                        return result;
                    }
                    catch (Exception exception)
                    {
                        // _logger.Log(exception);
                        throw;
                    }
                }
            }
        }
    }

    public class ViewParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}