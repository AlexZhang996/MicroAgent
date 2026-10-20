using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
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
         
            IChatClient chatClient = aiClient.GetChatClient("gpt-5.6-luna").AsIChatClient();
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

            chatClient = chatClient.AsBuilder().UseFunctionInvocation(configure:options =>
                {
                    options.FunctionInvoker = async (context, cancellationToken) =>
                    {

                        Console.WriteLine($"   内容: {context.CallContent}");
                        Console.WriteLine($"   函数名: {context.Function.Name}");
                        Console.WriteLine($"   参数: {JsonSerializer.Serialize(context.Arguments)}");

                        var result = await context.Function.InvokeAsync(context.Arguments, cancellationToken);


                        Console.WriteLine($"   结果: {result}");


                        return result;


                    };
                  }

                ).Build();
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


            ChatResponse response = await chatClient.GetResponseAsync($"{message}", chatOptions);
            //Console.WriteLine($"Product: {JsonSerializer.Serialize(response.Result)}");
            Console.WriteLine($"Product: {response.Text}");

        }
    }
}
