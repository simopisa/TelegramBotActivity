using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Unidea_Activity_Telegram_bot
{
    public class Function1
    {

        private ProductInfoHeaderValue userAgent;
        private HttpClient httpClient = HttpClientFactory.Create();
        private const string IG_BASE_URL = "https://www.instagram.com/api/v1/";
        private const string CHAT_ID = "-1001714171921";
        private const string CHAT_ID_demo = "-1001982336248";
        ILogger logger;

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 0 10 * * 3")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger = log;
            var _botClient = GetTelegramBotClient();

            var msg = await this.ComposeMessage();

            await _botClient.SendPhotoAsync(CHAT_ID, InputFile.FromUri(msg.displayImage), caption: msg.Caption);

        }



        [FunctionName("FunctionProva")]
        public async Task RunProva([TimerTrigger("1 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            logger = log;
            var _botClient = GetTelegramBotClient();


            var msg = await this.ComposeMessage();

            await _botClient.SendPhotoAsync(CHAT_ID_demo, InputFile.FromUri(msg.displayImage), caption: msg.Caption);

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }


        public async Task<MessageContent> ComposeMessage()
        {

            var userinfo = await this.GetProfileAsync("unidea_tn");


            List<Edge> feed = userinfo.data.user.edge_owner_to_timeline_media.edges;

            MessageContent theOne = feed.Where(x => x.node.edge_media_to_caption.edges[0].node.text.Contains("mercoledì")).Select(x => x.node).Select(x => new MessageContent
            {
                Caption = $"📣i nostri amici di unidea_tn vi propongono diverse 💡IDEE💡per questo mercoledì 💙🧡 \n https://instagram.com/p/{x.shortcode}",
                displayImage = x.thumbnail_src,
                PostLink = $"https://instagram.com/p/{x.shortcode}",
            }).FirstOrDefault();

            return theOne;
        }


        #region Instagram
        public async Task<WebProfileInfoViewModel?> GetProfileAsync(string username)
        {
            userAgent = new ProductInfoHeaderValue("(Instagram 219.0.0.12.117 Android)");

            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            var url = BuildUrl($"users/web_profile_info/?username={username}");
            return await this.GetResponseAsync<WebProfileInfoViewModel>(url);
        }
        private async Task<T?> GetResponseAsync<T>(Uri requestUri) where T : class
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.UserAgent.Add(userAgent);


            HttpResponseMessage? response = await httpClient.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
                string? json = response.Content.ReadAsStringAsync().Result;
                logger.LogInformation(json);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                logger.LogError(ex.InnerException.Message);
                throw;
            }

        }
        private Uri BuildUrl(string relativeurl)
        {
            return new Uri($"{IG_BASE_URL}{relativeurl}");
        }
        #endregion


        private TelegramBotClient GetTelegramBotClient()
        {
            var token = Environment.GetEnvironmentVariable("telegramBotApiKey");

            return new TelegramBotClient(token);
        }
    }
}
