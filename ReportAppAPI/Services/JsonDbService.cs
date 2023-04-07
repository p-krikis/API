using Dapper;
using ReportAppAPI.Models;
using System.Data.SqlClient;

namespace ReportAppAPI.Services
{
    public class JsonDbService
    {
        private readonly IConfiguration _configuration;

        public JsonDbService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<int> SaveFileAsync(string name, string jsonString)
        {
            int id;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sqlQuery = "INSERT INTO [dbo].[jsonDBtable] (Name, jsonString) OUTPUT INSERTED.Id VALUES (@Name, @jsonString)";
                id = await connection.QuerySingleOrDefaultAsync<int>(sqlQuery, new { Name = name, jsonString = jsonString });
            }
            return id;
        }
        public async Task<List<(int id, string name)>> GetAllJsonFilesAsync()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT id, name FROM jsonDBtable", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var result = new List<(int id, string name)>();

                        while (await reader.ReadAsync())
                        {
                            result.Add((reader.GetInt32(0), reader.GetString(1)));
                        }

                        return result;
                    }
                }
            }
        }
        public async Task<string> GetJsonFileByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT jsonString FROM jsonDBtable WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetString(0);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        public async Task DeleteJsonFileByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new SqlCommand("DELETE FROM jsonDBtable WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
