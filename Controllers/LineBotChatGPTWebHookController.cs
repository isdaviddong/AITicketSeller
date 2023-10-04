using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace isRock.Template
{
    public class LineBotChatGPTWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        [Route("api/LineBotChatGPTWebHook")]
        [HttpPost]
        public IActionResult POST()
        {
            const string AdminUserId = "________________________"; //👉repleace it with your Admin User Id

            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = "_________________________________________"; //👉repleace it with your Channel Access Token
                //配合Line Verify
                if (ReceivedMessage.events == null || ReceivedMessage.events.Count() <= 0 ||
                    ReceivedMessage.events.FirstOrDefault().replyToken == "00000000000000000000000000000000") return Ok();
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                var responseMsg = "";
                //如果是文字訊息
                if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    if (LineEvent.message.text.Contains("/reset"))
                    {
                        ChatHistoryManager.DeleteIsolatedStorageFile();
                        responseMsg = "我已經把之前的對談都給忘了!";
                    }
                    else
                    {
                        var chatHistory = ChatHistoryManager.GetMessagesFromIsolatedStorage(LineEvent.source.userId);
                        responseMsg = ChatGPT.getResponseFromGPT(LineEvent.message.text, chatHistory);
                        //儲存聊天紀錄
                        ChatHistoryManager.SaveMessageToIsolatedStorage(
                            DateTime.Now, LineEvent.source.userId, LineEvent.message.text, responseMsg);
                    }
                }
                else if (LineEvent.type.ToLower() == "message")
                    responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
                else
                    responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, responseMsg);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //回覆訊息
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }

}
