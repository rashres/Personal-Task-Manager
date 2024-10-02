using MySql.Data.MySqlClient;
using System;

public class MySqlDatabase
{
    // Define the connection string for MySQL
    private string connectionString = "Server=localhost;Database=TaskManagementSystem;User ID=root;Password=your-own;";

    // Method to get the MySQL connection
    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }

    // Insert a new task into the database
    public void AddTaskToDatabase(string trelloCardId, string taskName, string taskDescription, string taskStatus)
    {
        using (var db = GetConnection())
        {
            db.Open();
            string query = "INSERT INTO Tasks (trello_card_id, task_name, task_description, task_status) VALUES (@cardId, @name, @description, @status)";

            using (MySqlCommand cmd = new MySqlCommand(query, db))
            {
                cmd.Parameters.AddWithValue("@cardId", trelloCardId);
                cmd.Parameters.AddWithValue("@name", taskName);
                cmd.Parameters.AddWithValue("@description", taskDescription);
                cmd.Parameters.AddWithValue("@status", taskStatus);

                cmd.ExecuteNonQuery();
            }
        }
    }

    // Fetch all tasks from the database
    public void GetAllTasksFromDatabase()
    {
        using (var db = GetConnection())
        {
            db.Open();
            string query = "SELECT * FROM Tasks";

            using (MySqlCommand cmd = new MySqlCommand(query, db))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["id"]}: {reader["task_name"]} - {reader["task_status"]}");
                    }
                }
            }
        }
    }

    // Update the status of a task in the database
    public void UpdateTaskStatusInDatabase(string trelloCardId, string newStatus)
    {
        using (var db = GetConnection())
        {
            db.Open();
            string query = "UPDATE Tasks SET task_status = @status WHERE trello_card_id = @cardId";

            using (MySqlCommand cmd = new MySqlCommand(query, db))
            {
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@cardId", trelloCardId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Delete a task from the database
    public void DeleteTaskFromDatabase(string trelloCardId)
    {
        using (var db = GetConnection())
        {
            db.Open();
            string query = "DELETE FROM Tasks WHERE trello_card_id = @cardId";

            using (MySqlCommand cmd = new MySqlCommand(query, db))
            {
                cmd.Parameters.AddWithValue("@cardId", trelloCardId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
