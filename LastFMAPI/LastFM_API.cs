using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fmapi
{
    class LastFM_API
    {
        private string BaseURL = "https://ws.audioscrobbler.com/2.0/";
        private string key { get; set; }
        public LastFM_API(string _key) { key = _key; }

        public async Task<string> AlbumSearch(Func<JObject, string, string, string> FindValue, string Album, string Artist = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Album))
                    throw new ArgumentNullException();

                string Url = $"{BaseURL}?method=album.search&album={UriEnc(Album)}";
                JObject Json = await JsonResponse(Url);

                return FindValue(Json, Artist, Album);
            }
            catch
            { 
                return "";
            }
        }

        public async Task<string> AlbumGetInfo(Func<JObject, string> FindValue, string Album, string Artist = null, string Track = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Album) | (string.IsNullOrWhiteSpace(Artist) & string.IsNullOrWhiteSpace(Track)))
                    throw new ArgumentNullException();

                string Url = $"{BaseURL}?method=album.getinfo&album={UriEnc(Album)}" + (!string.IsNullOrWhiteSpace(Track) ? $"&track={UriEnc(Track)}" : $"&artist={UriEnc(Artist)}");
                JObject Json = await JsonResponse(Url);

                return FindValue(Json);
            }
            catch
            {
                return "";
            }
        }

        private async Task<JObject> JsonResponse(string url)
        {
            using (HttpClient client = new())
            {
                HttpResponseMessage Resp = await client.GetAsync(url + $"&api_key={key}&format=json");
                if (Resp.IsSuccessStatusCode)
                    return JObject.Parse(await Resp.Content.ReadAsStringAsync());
            }
            throw new HttpRequestException();
        }

        private string UriEnc(string a)
        {
            return Uri.EscapeDataString(a);
        }
    }
}
