using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtensionsAI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

     
            OpenAIClientOptions options = new OpenAIClientOptions();
            options.Endpoint =new Uri("https://api.routin.ai/v1");

            OpenAIClient aiClient = new OpenAIClient (new ApiKeyCredential("ak-3855a409097a4487bd7080383dac3442"), options);
         
            IChatClient chatClient = aiClient.GetChatClient("gpt-6-sol").AsIChatClient();
            //chatClient.AsIChatClient().GetResponseAsync("Hello, how are you?").ContinueWith(task =>
            //{
            //    if (task.IsCompletedSuccessfully)
            //    {
            //        var response = task.Result;
            //        Console.WriteLine($"Response: {response}");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"Error: {task.Exception?.Message}");
            //    }
            //}).Wait();


            ChatOptions chatOptions = new ChatOptions();

           // chatOptions.Temperature = 0.7f;    //控制输出的随机性。值越高（如接近 1.0 或 2.0），输出越随机、有创造性；值越低（接近 0），输出越确定、保守。
           // chatOptions.TopP = 0.9f;           //另一种控制随机性的方式。模型仅从累积概率达到 P 值的最可能 token 池中采样。通常建议与 Temperature 二选一进行调整。
           // chatOptions.TopK = 1;              //控制输出的多样性。模型仅从概率最高的 K 个 token 中采样。较低的值会导致更保守的输出，而较高的值会增加多样性。
           // chatOptions.MaxOutputTokens = 100;  //限制本次回复生成的最大 token 数量，用于控制成本或防止生成过长内容
           // chatOptions.FrequencyPenalty = 0.5f; //控制模型对重复 token 的惩罚程度。值越高，模型越不倾向于重复相同的 token，从而生成更丰富的内容。
           // chatOptions.PresencePenalty = 0.5f;  //控制模型对新话题的倾向。值越高，模型越倾向于引入新话题，而不是重复已有话题。
           
           // List<string> stopSequences = new List<string>();
           // stopSequences.Add("ZY");

           // chatOptions.StopSequences= stopSequences; //指定一个或多个停止序列，当模型生成这些序列时，回复将被截断。用于控制输出的结束点。

           // chatOptions.Seed = 42;               //设置随机种子，以便在相同输入下生成可重复的输出。对于调试或测试非常有用。
           //// chatOptions.ResponseFormat=new Microsoft.Extensions.AI.ChatResponseFormat().; //指定响应格式为 ProductInfo 类型
           // chatOptions.ModelId = "gpt-5.6-luna"; //指定使用的模型 ID



            IList<AITool>? availableTools = new List<AITool>
                {
                       AIFunctionFactory.Create(( [Description("产品名称，例如：杯子")] string productName,
    [Description("产品编号，例如：ZY001")] string productCode) =>
                        {
                        
                            return new ProductInfo2 { ProductName = productName, ProductCode = productCode };
                        }, "ProductInfoTool", "获取产品信息的工具,从评论中提取产品名称和编号"),
                };

            chatOptions.Tools = availableTools;
            chatOptions.ToolMode = ChatToolMode.RequireAny; //设置工具模式为自动，模型将根据需要调用工具来辅助生成回复。

            string message = "淘宝购买了一个杯子，杯子编号ZY001,杯子生产日期是2026/10/19，不知道会不会好用";

            //ChatResponse< ProductInfo> response =await chatClient.GetResponseAsync<ProductInfo>($"What's the ProductInfo of this review? {message}",chatOptions);

            //Console.WriteLine($"Product: {JsonSerializer.Serialize(response.Result)}");
            //Console.WriteLine($"Product Name: {response.Result.ProductName}");
            //Console.WriteLine($"Product Name: {response.Result.ProductCode}");

            //chatClient = chatClient.AsBuilder().UseFunctionInvocation(configure:options =>
            //    {
            //        options.FunctionInvoker = async (context, cancellationToken) =>
            //        {

            //            Console.WriteLine($"   内容: {context.CallContent}");
            //            Console.WriteLine($"   函数名: {context.Function.Name}");
            //            Console.WriteLine($"   参数: {JsonSerializer.Serialize(context.Arguments)}");

            //            var result = await context.Function.InvokeAsync(context.Arguments, cancellationToken);


            //            Console.WriteLine($"   结果: {result}");


            //            return result;


            //        };
            //      }

            //    ).Build();
            //try
            //{
            //    await foreach (var chumk in chatClient.GetStreamingResponseAsync($" What's the ProductInfo of this review? {message}", chatOptions))
            //    {
            //        Console.Write($"{chumk}");
            //    }
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine($"Error: {e.Message}");
            //}


            //ChatResponse response = await chatClient.GetResponseAsync($"{message}", chatOptions);
            ////Console.WriteLine($"Product: {JsonSerializer.Serialize(response.Result)}");
            //Console.WriteLine($"Product: {response.Text}");


            ChatClientMetadata? metadata = chatClient.GetService<ChatClientMetadata>();
            if (metadata != null)
            {
                Console.WriteLine($"Provider: {metadata.ProviderName}");
                Console.WriteLine($"DefaultModelId: {metadata.DefaultModelId}");
                Console.WriteLine($"ProviderUri: {metadata.ProviderUri}");
            }


            //获取 IEmbeddingGenerator 实例

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<OpenAIClient>(aiClient)
                .AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
                {
                    var client = sp.GetRequiredService<OpenAIClient>();
                    return client.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
                })
                .BuildServiceProvider();

            var documents = new[]
            {
                "Microsoft.Extensions.AI 为 .NET 提供统一 AI 抽象。",
                "向量嵌入可以用来执行语义搜索和相似度匹配。"
            };


            //IEmbeddingGenerator<string,Embedding<float>> embeddingGenerator = aiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();


            //创建Function


            //var tools = AIFunctionFactory.Create(([Description("城市名称")] string city) => {
            //    int temperature = Random.Shared.Next(-5, 36);
            //    bool willRain = Random.Shared.NextDouble() > 0.6;
            //    return $"The current weather in {city} is {(willRain ? "raining" : "sunny")} with a temperature of {temperature}°C.";

            //},"GetCityWeather", "查询指定城市的实时天气");

            //chatClient= chatClient.AsBuilder().UseFunctionInvocation(configure:options =>
            //{
            //    options.FunctionInvoker = async (context, cancellationToken) =>
            //    {
            //        Console.WriteLine($"   内容: {context.CallContent}");
            //        Console.WriteLine($"   函数名: {context.Function.Name}");
            //        Console.WriteLine($"   参数: {JsonSerializer.Serialize(context.Arguments)}");
            //        var result = await context.Function.InvokeAsync(context.Arguments, cancellationToken);
            //        Console.WriteLine($"   结果: {result}");
            //        return result;
            //    };
            //}).Build();

            //List<AITool> toolsList = new List<AITool> { tools };
            //string message2 = "上海今天气温多少?";

            //chatOptions = new ChatOptions();
            //chatOptions.Tools= toolsList;
            //chatOptions.ToolMode= ChatToolMode.RequireAny; //设置工具模式为自动，模型将根据需要调用工具来辅助生成回复。

            //ChatResponse response = await chatClient.GetResponseAsync($"{message2}");
            //Console.WriteLine($"weather: {response.Text}");



            #region 集合工具

            int iterationCount = 0;
            int functionCallCount = 0;
            // 创建模拟工具集
            var monitoringTools = new[]
            {
                AIFunctionFactory.Create((string city) =>
                {
                    Thread.Sleep(500); // 模拟API延迟
                    return new { Temperature = Random.Shared.Next(15, 35), Humidity = Random.Shared.Next(40, 80) };
                }, "get_weather", "获取指定城市的天气信息"),

                AIFunctionFactory.Create((string city) =>
                {
                    Thread.Sleep(300);
                    return new { Hotels = Random.Shared.Next(50, 200), AvgPrice = Random.Shared.Next(300, 1500) };
                }, "get_hotels", "查询指定城市的酒店数量和平均价格"),

                AIFunctionFactory.Create((int temperature) =>
                {
                    return temperature switch
                    {
                        < 15 => "建议穿冬装，携带保暖衣物",
                        < 25 => "建议穿春秋装，温度适宜",
                        _ => "建议穿夏装，注意防晒"
                    };
                }, "suggest_clothing", "根据温度推荐穿搭")
            };


            var monitoringOptions = new ChatOptions
            {
                ToolMode = ChatToolMode.Auto,
                AllowMultipleToolCalls = true,
                Tools = monitoringTools
            };
            var  monitoredClient = chatClient.AsBuilder().UseFunctionInvocation(configure: options =>
            {
                options.AllowConcurrentInvocation = true;
                options.MaximumIterationsPerRequest = 10;
                options.FunctionInvoker = async (context, cancellationToken) =>
                {
                    functionCallCount++;
                    var sw = Stopwatch.StartNew();

                    Console.WriteLine($"   [函数调用 #{functionCallCount}]");
                    Console.WriteLine($"   函数名: {context.Function.Name}");
                    Console.WriteLine($"   参数: {JsonSerializer.Serialize(context.Arguments)}");

                    var result = await context.Function.InvokeAsync(context.Arguments, cancellationToken);
                    sw.Stop();

                    Console.WriteLine($"   结果: {result}");
                    Console.WriteLine($"   耗时: {sw.ElapsedMilliseconds}ms");

                    return result;
                };
            }).Build();
            var monitoringResult = await monitoredClient.GetResponseAsync(
                "帮我查询北京和上海的天气，并根据北京的温度推荐穿搭",
                monitoringOptions
            );

            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("✅ 最终响应:");
            Console.WriteLine(monitoringResult.Text);
            Console.WriteLine($"\n📊 统计: 共 {iterationCount} 次迭代，{functionCallCount} 次函数调用");

            #endregion
        }


    }
}
