using Bee.Api.Core.Messages;
using Bee.UI.Core;
using Custom.Contracts;

namespace JsonRpcClient
{
    internal static class Program
    {
        private static bool _isInitialized;

        private static async Task<int> Main()
        {
            Console.WriteLine("Bee.NET JSON-RPC Console Client");
            Console.WriteLine("================================");

            while (true)
            {
                PrintMenu();
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(input)) continue;

                try
                {
                    switch (input)
                    {
                        case "1": Initialize(); break;
                        case "2": Login(); break;
                        case "3": await CallEmployeeHelloAsync("Hello", PayloadFormat.Plain); break;
                        case "4": await CallEmployeeHelloAsync("HelloEncoded", PayloadFormat.Encoded); break;
                        case "5": await CallEmployeeHelloAsync("HelloEncrypted", PayloadFormat.Encrypted); break;
                        case "6": await CallEmployeeHelloAsync("HelloLocal", PayloadFormat.Plain); break;
                        case "q":
                        case "quit":
                        case "exit":
                            return 0;
                        default:
                            Console.WriteLine("Unknown command.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("  1) Initialize (set endpoint)");
            Console.WriteLine("  2) Login");
            Console.WriteLine("  3) Hello           (Plain, no auth)");
            Console.WriteLine("  4) HelloEncoded    (serialize + compress, requires login)");
            Console.WriteLine("  5) HelloEncrypted  (serialize + compress + encrypt, requires login)");
            Console.WriteLine("  6) HelloLocal      (local-only, no remote API)");
            Console.WriteLine("  q) Quit");
        }

        private static void Initialize()
        {
            Console.Write("Endpoint [http://localhost:5219/api]: ");
            var endpoint = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(endpoint))
                endpoint = "http://localhost:5219/api";

            ClientInfo.Initialize(endpoint);
            _isInitialized = true;
            Console.WriteLine("Initialization complete.");
        }

        private static void Login()
        {
            if (!EnsureInitialized()) return;

            var result = ClientInfo.SystemApiConnector.Login("jeff", "1234");
            ClientInfo.ApplyLoginResult(result);
            Console.WriteLine("Login complete.");
        }

        private static async Task CallEmployeeHelloAsync(string method, PayloadFormat format)
        {
            if (!EnsureInitialized()) return;

            var connector = ClientInfo.CreateFormApiConnector("Employee");
            var request = new HelloRequest { UserName = "Jeff" };

            var response = await connector.ExecuteAsync<HelloResponse>(method, request, format);
            Console.WriteLine($"Message: {response.Message}");
        }

        private static bool EnsureInitialized()
        {
            if (_isInitialized) return true;
            Console.WriteLine("Please initialize first (option 1).");
            return false;
        }
    }
}
