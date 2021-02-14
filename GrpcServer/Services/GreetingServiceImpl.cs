using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class GreetingServiceImpl : GreetingService.GreetingServiceBase
    {
        public override Task<GreetingResponse> Greet(GreetingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GreetingResponse() { Result = $"Hello {request.Greeting.FirstName} {request.Greeting.LastName}" });
        }

        public override async Task GreetManyTimes(GreetManyTimesRequest request, IServerStreamWriter<GreetManyTimesResponse> responseStream, ServerCallContext context)
        {
            var response = new GreetManyTimesResponse()
            {
                Result = $"Hello {request.Greeting.FirstName} {request.Greeting.LastName}"
            };

            for (int i = 0; i < 5; i++)
            {
                await responseStream.WriteAsync(response);
                await Task.Delay(1000);
            }
        }

        public override async Task<LongGreetResponse> LongGreet(IAsyncStreamReader<LongGreetRequest> requestStream, ServerCallContext context)
        {
            var response = new LongGreetResponse();
            //while (await requestStream.MoveNext())
            //{
            //    response.Result += $"Hello {requestStream.Current.Greeting.FirstName} {requestStream.Current.Greeting.LastName}{Environment.NewLine}";
            //}

            await foreach (var item in requestStream.ReadAllAsync())
            {
                response.Result += $"Hello {item.Greeting.FirstName} {item.Greeting.LastName}{Environment.NewLine}";
            }

            return response;
        }

        public override async Task GreetEveryone(IAsyncStreamReader<GreetEveryoneRequest> requestStream, IServerStreamWriter<GreetEveryoneResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var response = new GreetEveryoneResponse()
                {
                    Result = $"Hello {requestStream.Current.Greeting.FirstName} {requestStream.Current.Greeting.LastName}"
                };

                await responseStream.WriteAsync(response);
                await Task.Delay(1000);
            }
        }
    }
}
