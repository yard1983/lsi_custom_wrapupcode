using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.PS.DataAccess
{
    public class RestAPI
    {
        public RestAPI()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public async Task<string> GetAccessToken(string serviceUri, string clientID, string clientSecret)
        {
            var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });
            var basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(string.Format("{0}:{1}", clientID, clientSecret)));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            var response = await client.PostAsync(serviceUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> Post(string serviceUri, string accessToken, string data, string contentType = "application/json", Dictionary<string, string> headers = null)
        {
            var client = new HttpClient();

            var content = new StringContent(data, Encoding.UTF8, contentType);
            if(!string.IsNullOrEmpty(accessToken))
            { 
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);                
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                
            }
            var response = await client.PostAsync(serviceUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            string reponseContent = await response.Content.ReadAsStringAsync();
            return reponseContent;

        }

      

        public async Task<string> Put(string serviceUri, string accessToken, string data, string contentType = "application/json")
        {
            var client = new HttpClient();

            var content = new StringContent(data, Encoding.UTF8, contentType);
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);                
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var response = await client.PutAsync(serviceUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            string reponseContent = await response.Content.ReadAsStringAsync();
            return reponseContent;

        }

        public async Task<string> Get(string serviceUri, string accessToken, string contentType = "application/json")
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);                
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var response = await client.GetAsync(serviceUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            string reponseContent = await response.Content.ReadAsStringAsync();
            return reponseContent;

        }

        public async Task<string> Patch(string serviceUri, string accessToken, string data, string contentType = "application/json")
        {
            var client = new HttpClient();

            var content = new StringContent(data, Encoding.UTF8, contentType);
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);                
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var response = await client.PatchAsync(serviceUri, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            string reponseContent = await response.Content.ReadAsStringAsync();
            return reponseContent;

        }

        public async Task<string> Delete(string serviceUri, string accessToken, string contentType = "application/json")
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);                
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var response = await client.DeleteAsync(serviceUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ToString());
            }

            string reponseContent = await response.Content.ReadAsStringAsync();
            return reponseContent;

        }

    }
}
