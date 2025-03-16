using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.Data.SqlClient;

namespace DigitalWellbeing
{
    public class DBStorage
    {
        private readonly Configuration _configuration;
        private readonly IFirebaseConfig _fc;

        public IFirebaseClient firebaseClient;

        public DBStorage(Configuration configuration)
        {
            _configuration = configuration;

            _fc = new FirebaseConfig()
            {
                AuthSecret = _configuration.DBSecret,
                BasePath = _configuration.BasePath
            };

            firebaseClient = new FireSharp.FirebaseClient(_fc);
        }

        public async Task<bool> SaveToFireBase(Dictionary<string, TimeSpan> appDurations)
        {
            var tasks = new List<Task<bool>>();

            foreach (var entry in appDurations)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var data = new Activity()
                    {
                        Name = entry.Key,
                        Duration = entry.Value
                    };

                    try
                    {
                        var response = await firebaseClient.SetAsync(
                            $"AppUsage/{data.Date:yyyy-MM-dd}/{data.Name}", data);

                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);
            return results.All(success => success);
        }


        public async Task<bool> SaveToSqlServer(Dictionary<string, TimeSpan> appDurations)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.LocalConnectionString))
            {
                await conn.OpenAsync();
                
                foreach (var entry in appDurations)
                {
                    string query = "INSERT INTO AppUsage (Date, AppName, Duration) VALUES (@date, @appName, @duration)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@appName", entry.Key);
                        cmd.Parameters.AddWithValue("@duration", entry.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 0)
                        {
                            return false; 
                        }
                    }
                }

                return true;
            }
        }


    }
}
