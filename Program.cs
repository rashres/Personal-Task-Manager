using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        
        string apiKey = Environment.GetEnvironmentVariable("MY_API_KEY");
        string token = Environment.GetEnvironmentVariable("MY_API_TOKEN");


        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(token))
        {
            Console.WriteLine("API key or token is missing. Please set the environment variables.");
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                string url = $"https://api.trello.com/1/members/me/boards?key={apiKey}&token={token}";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JArray boards = JArray.Parse(responseBody);

                if (boards == null || boards.Count == 0)
                {
                    Console.WriteLine("No boards found or unable to fetch boards.");
                    return;
                }

                // Find the specific board by name
                var board = boards.FirstOrDefault(b => b["name"].ToString() == "Daily Task Manager");

                if (board != null)
                {
                    Console.WriteLine($"Fetching tasks from board: {board["name"]}");
                    string boardId = board["id"].ToString();

                    // Fetch cards (tasks) for the board
                    await FetchBoardTasks(client, apiKey, token, boardId);

                    // Fetch and display lists for the board
                    await FetchListsForBoard(client, apiKey, token, boardId);

                    // Store tasks in MySQL database
                    var db = new MySqlDatabase(); // Initialize the MySQL database connection
                    db.AddTaskToDatabase("66e8b7a2ba3ddbf74c56c6cb", "Complete training course", "No description", "To Do");

                    // Example usage: Update a task's status and add a comment
                    Console.WriteLine("Updating a task's status and adding a comment...");

                    string cardId = "66e8b7a2ba3ddbf74c56c6cb";
                    string newListId = "66e8b7a2ba3ddbf74c56c698";

                    await UpdateTaskStatus(client, apiKey, token, cardId, newListId);
                    await AddCommentToTask(client, apiKey, token, cardId, "This is a sample comment.");
                }
                else
                {
                    Console.WriteLine("Board not found. Please check the board name.");
                }
            }
            catch (HttpRequestException e)
            {
                LogError($"Trello API request error: {e.Message}");
                Console.WriteLine($"Request error: {e.Message}");
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error: {ex.Message}");
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    // Function to fetch tasks (cards) for a specific board and display their IDs
    static async Task FetchBoardTasks(HttpClient client, string apiKey, string token, string boardId)
    {
        string url = $"https://api.trello.com/1/boards/{boardId}/cards?key={apiKey}&token={token}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JArray tasks = JArray.Parse(responseBody);

            Console.WriteLine($"Tasks for board {boardId}:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task["name"]} (ID: {task["id"]})");
                string description = task["desc"]?.ToString();
                Console.WriteLine($"Description: {(!string.IsNullOrEmpty(description) ? description : "No description available.")}");
                Console.WriteLine("--------------------------");
            }
        }
        catch (HttpRequestException e)
        {
            LogError($"Error fetching tasks from Trello: {e.Message}");
            Console.WriteLine($"Error fetching tasks: {e.Message}");
        }
    }

    // Function to fetch lists (statuses) for a specific board and display their IDs
    static async Task FetchListsForBoard(HttpClient client, string apiKey, string token, string boardId)
    {
        string url = $"https://api.trello.com/1/boards/{boardId}/lists?key={apiKey}&token={token}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JArray lists = JArray.Parse(responseBody);

            Console.WriteLine($"Lists for board {boardId}:");
            foreach (var list in lists)
            {
                Console.WriteLine($"- {list["name"]} (ID: {list["id"]})");
            }
        }
        catch (HttpRequestException e)
        {
            LogError($"Error fetching lists from Trello: {e.Message}");
            Console.WriteLine($"Error fetching lists: {e.Message}");
        }
    }

    // Function to update the status of a task (move to a new list)
    static async Task UpdateTaskStatus(HttpClient client, string apiKey, string token, string cardId, string newListId)
    {
        string url = $"https://api.trello.com/1/cards/{cardId}?key={apiKey}&token={token}&idList={newListId}";

        try
        {
            HttpResponseMessage response = await client.PutAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Task status updated successfully.");
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                LogError($"Failed to update task status. Error: {errorResponse}");
                Console.WriteLine($"Failed to update task status. Error: {errorResponse}");
            }
        }
        catch (HttpRequestException e)
        {
            LogError($"Error updating task status in Trello: {e.Message}");
            Console.WriteLine($"Error updating task status: {e.Message}");
        }
    }

    // Function to add a comment to a task
    static async Task AddCommentToTask(HttpClient client, string apiKey, string token, string cardId, string commentText)
    {
        string url = $"https://api.trello.com/1/cards/{cardId}/actions/comments?key={apiKey}&token={token}&text={commentText}";

        try
        {
            HttpResponseMessage response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Comment added to task successfully.");
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                LogError($"Failed to add comment. Error: {errorResponse}");
                Console.WriteLine($"Failed to add comment. Error: {errorResponse}");
            }
        }
        catch (HttpRequestException e)
        {
            LogError($"Error adding comment to Trello task: {e.Message}");
            Console.WriteLine($"Error adding comment: {e.Message}");
        }
    }

    // Error logging method to save logs to a file
    public static void LogError(string message)
    {
        using (StreamWriter sw = new StreamWriter("error_log.txt", true))  // Append to the log file
        {
            sw.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
