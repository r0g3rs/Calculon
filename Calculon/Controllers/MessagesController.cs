using Calculon.Models;
using Calculon.Resources;
using Calculon.Services;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Calculon.Controllers
{
    public class MessagesController : ApiController
    {
        private readonly UserService userService;
        private readonly WebClientService webClientService;

        public double Result { get; set; }

        public MessagesController()
        {
            userService = new UserService();
            webClientService = new WebClientService();
        }

        // POST api/messages
        public async Task<IHttpActionResult> Post(JObject jsonObject)
        {

            Console.WriteLine($"Message Received: {jsonObject}");

            //var message = new Message();

            //if (jsonObject["type"].Value<string>() == "conversationUpdate")
            //{
            //    message = new Message() { From = "User", To = jsonObject["from"].ToString() };
            //}
            //else
            //{

            //    message = new Message() { Content = jsonObject["text"].Value<string>(), From = "User", To = jsonObject["from"].ToString() };
            //}

            var message = JsonConvert.DeserializeObject<Message>(jsonObject.ToString(), JsonNetSerializer.Settings);

            var plainText = (message.Content as PlainDocument)?.Value ?? (message.Content as PlainText)?.Text;

            var messageContent = plainText;

            var user = await userService.GetUserAsync(new User { Node = message.From });

            if (string.IsNullOrEmpty(messageContent))
            {
                await webClientService.SendMessageAsync(Messages.FirstMessage, message.From);

                await ChangeUserStateAsync(user, Models.SessionState.FirstAccess);

                return Ok();
            }

            switch (user.Session.State)
            {
                case Models.SessionState.FirstAccess:

                    await webClientService.SendMessageAsync(Messages.FirstMessage, message.From);

                    await webClientService.SendMessageAsync(Messages.InstructionsMessage, message.From);

                    await ChangeUserStateAsync(user, Models.SessionState.FirstNumber);

                    break;

                case Models.SessionState.FirstNumber:

                    await webClientService.SendMessageAsync(Messages.FirstNumberMessage, message.From);

                    await ChangeUserStateAsync(user, Models.SessionState.SecondNumber);

                    break;

                case Models.SessionState.SecondNumber:

                    user.FirstNumber = double.Parse(message.Content.ToString());

                    await webClientService.SendMessageAsync(Messages.SecondNumberMessage, message.From);

                    await ChangeUserStateAsync(user, Models.SessionState.Operation, user.FirstNumber);

                    break;

                case Models.SessionState.Operation:

                    user.SecondNumber = double.Parse(message.Content.ToString());

                    await SendMenuOperationsAsync(Messages.OperationMessage, message.From);

                    await ChangeUserStateAsync(user, Models.SessionState.Answering, user.FirstNumber, user.SecondNumber);

                    break;
                case Models.SessionState.Answering:

                    user.Operation = int.Parse(message.Content.ToString());

                    var result = ResolveMath(user.FirstNumber, user.SecondNumber, user.Operation);

                    await webClientService.SendMessageAsync(string.Format(Messages.ResultMessage, ReturnOperation(user.Operation), result), message.From);

                    await SendMenuRestartAsync(Messages.RestartingMessage, message.From);

                    await ChangeUserStateAsync(user, Models.SessionState.Restart, user.FirstNumber, user.SecondNumber, user.Operation);

                    break;

                case Models.SessionState.Restart:

                    if (int.Parse(message.Content.ToString()) == 1)
                    {
                        await webClientService.SendMessageAsync(Messages.FirstNumberMessage, message.From);

                        await ChangeUserStateAsync(user, Models.SessionState.SecondNumber);
                    }
                    else
                    {
                        await webClientService.SendMessageAsync(Messages.EndMessage, message.From);

                        await ChangeUserStateAsync(user, Models.SessionState.End);
                    }

                    break;
                case Models.SessionState.End:
                    
                    await ChangeUserStateAsync(user, Models.SessionState.FirstAccess);

                    break;
            }

            return Ok();
        }

        private string ReturnOperation(int operation)
        {
            var result = string.Empty;

            switch (operation)
            {
                case 1://Sum
                    result = Messages.SumActionText;
                    break;
                case 2://Subtract
                    result = Messages.SubtractActionText;
                    break;
                case 3://Multiply
                    result = Messages.MultiplyActionText;
                    break;
                case 4://Divide
                    result = Messages.DivideActionText;
                    break;
                default:
                    break;
            }

            return result;
        }

        private async Task SendMenuOperationsAsync(string text, Node to)
        {
            var select = new Select
            {
                Text = text,
                Options = new[]
                {
                    new SelectOption
                    {
                        Order = 1,
                        Text = Messages.SumActionText
                    },
                    new SelectOption
                    {
                        Order = 2,
                        Text = Messages.SubtractActionText
                    },
                    new SelectOption
                    {
                        Order = 3,
                        Text = Messages.MultiplyActionText
                    },
                    new SelectOption
                    {
                        Order = 4,
                        Text = Messages.DivideActionText
                    }
                }
            };
            await webClientService.SendMessageAsync(select, to);
        }

        private async Task SendMenuRestartAsync(string text, Node to)
        {
            var select = new Select
            {
                Text = text,
                Options = new[]
                {
                    new SelectOption
                    {
                        Order = 1,
                        Text = Messages.RestarYesActionText
                    },
                    new SelectOption
                    {
                        Order = 2,
                        Text = Messages.RestarNoActionText
                    }
                }
            };
            await webClientService.SendMessageAsync(select, to);
        }

        private async Task ChangeUserStateAsync(User user, Models.SessionState newState)
        {
            user.Session.State = newState;
            await userService.UpdateUserSessionAsync(user);
        }

        private async Task ChangeUserStateAsync(User user, Models.SessionState newState, double firstNumber, double secondNumber = 0.0)
        {
            user.Session.State = newState;
            user.FirstNumber = firstNumber;
            user.SecondNumber = secondNumber;
            await userService.UpdateUserSessionAsync(user);
        }

        private async Task ChangeUserStateAsync(User user, Models.SessionState newState, double firstNumber, double secondNumber, int operation)
        {
            user.Session.State = newState;
            user.FirstNumber = firstNumber;
            user.SecondNumber = secondNumber;
            user.Operation = operation;
            await userService.UpdateUserSessionAsync(user);
        }

        private double ResolveMath(double firstNumber, double secondNumber, int operation)
        {
            var result = 0.0;

            switch (operation)
            {
                case 1://Sum
                    result = firstNumber + secondNumber;
                    break;
                case 2://Subtract
                    result = firstNumber - secondNumber;
                    break;
                case 3://Multiply
                    result = firstNumber * secondNumber;
                    break;
                case 4://Divide
                    if (secondNumber != 0)
                    {
                        result = firstNumber / secondNumber;
                    }
                    else
                    {
                        result = 0;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
