using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Threading.Tasks;
using static GrpcServer.GreetingService;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new GreetingServiceClient(channel);
            var greeting = new Greeting()
            {
                FirstName = "Yavuz",
                LastName = "Altun"
            };

            await DoGreet(client, greeting);
            await DoGreetManyTimes(client, greeting);
            await DoLongGreet(client, greeting);
            await DoGreetEveryone(client, greeting);

            Console.Read();
        }

        static async Task DoGreet(GreetingServiceClient client, Greeting greeting)
        {
            var response = await client.GreetAsync(new GreetingRequest() { Greeting = greeting });
            Console.WriteLine(response.Result);
        }

        static async Task DoGreetManyTimes(GreetingServiceClient client, Greeting greeting)
        {
            var stream = client.GreetManyTimes(new GreetManyTimesRequest() { Greeting = greeting });
            while (await stream.ResponseStream.MoveNext())
            {
                Console.WriteLine("Client receiving:");
                Console.WriteLine(stream.ResponseStream.Current.Result);
            }

            //await foreach (var item in stream.ResponseStream.ReadAllAsync())
            //{
            //    Console.WriteLine(item.Result);
            //}
        }

        static async Task DoLongGreet(GreetingServiceClient client, Greeting greeting)
        {
            var stream = client.LongGreet();
            Console.WriteLine("Client sending:");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Greeting {i}: {greeting}");
                await stream.RequestStream.WriteAsync(new LongGreetRequest() { Greeting = greeting });
                await Task.Delay(1000);
            }

            await stream.RequestStream.CompleteAsync();
            var response = await stream.ResponseAsync;

            Console.WriteLine("Client received:");
            Console.WriteLine(response.Result);
        }

        static async Task DoGreetEveryone(GreetingServiceClient client, Greeting greeting)
        {
            var stream = client.GreetEveryone();
            var readerTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine($"Client received: {stream.ResponseStream.Current.Result}");
                }
            });

            for (int i = 0; i < 5; i++)
            {
                var request = new GreetEveryoneRequest() { Greeting = greeting };
                Console.WriteLine($"Client sending: {request}");
                await stream.RequestStream.WriteAsync(request);
                await Task.Delay(1000);
            }

            await stream.RequestStream.CompleteAsync();
            await readerTask;
        }
    }
}
