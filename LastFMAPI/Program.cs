using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Web;
using IO = System.IO;
using TagLib;

using fmapi;

namespace LastFMAPI
{
    class LastFMAPI
    {
        static string[] Extensions = new string[] { "wav", "mp3", "flac" };
        static string imageSize = "large"; // small, medium, large, extralarge, mega
        static string apikey = "74b915bc1074ab60ca43e9d1ff4e3b7d";

        static bool IsMusicFile(string file)
        {
            string FileExtension = file.Split('.').Last();
            foreach (var item in Extensions)
                if (FileExtension == item)
                    return true;

            return false;
        }

        static async Task Main(string[] args)
        {
            string path = ((args.Length == 0) ? Console.ReadLine() ?? "" : args[0]).Replace("\"", "");

            LastFM_API fmApi = new LastFM_API(apikey);
         
            if (IO.File.Exists(path))
            {
                if (IsMusicFile(Path.GetFileName(path)))
                {
                    TagLib.File file = TagLib.File.Create(path, ReadStyle.Average);

                    string Album = file.Tag.Album;
                    string Artist = file.Tag.Artists.Last();

                    Console.WriteLine($"Album: {Album}");
                    Console.WriteLine($"Artist: {Artist}");

                    string url = "";

                    Console.WriteLine("Method: album.search");
                    url = await fmApi.AlbumGetInfo_GetAlbumImage(Artist, Album, AlbumGetInfo_FindAlbumImg);

                    if (string.IsNullOrEmpty(url))
                    {
                        Console.WriteLine("Method: album.getinfo");
                        url = await fmApi.AlbumSearch_GetAlbumImage(Album, AlbumSearch_FindAlbumImg);
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        using (WebClient client = new())
                        {
                            client.DownloadFile(url, "cover.png");
                            Console.WriteLine($"Url: {url}");
                        }
                    }
                    else
                        Console.WriteLine("Unable to find any images.");
                }
            }
        }

        static string AlbumSearch_FindAlbumImg(JObject Json, string AlbumRequest)
        {
            Dictionary<string, string> ImageList = new Dictionary<string, string>();

            dynamic DJson = Json;

            JArray Albums = DJson.results.albummatches.album;

            foreach (dynamic Album in Albums)
            {
                string name = Album.name;
                JArray Images = Album.image;
                if 
                (
                    name == AlbumRequest |
                    name.ToLower() == AlbumRequest.ToLower() |
                    name.ToLower().Replace(" ", "") == AlbumRequest.ToLower().Replace(" ", "")
                )
                {
                    foreach (dynamic Image in Images)
                    {
                        string url = Image["#text"];
                        string size = Image["size"];
                        if (!string.IsNullOrEmpty(url) & !string.IsNullOrEmpty(size))
                            ImageList.Add(size, url);
                    }
                    if (ImageList.Count > 0)
                        break;
                }
            }

            if (ImageList.Count == 0)
                return "";

            return ImageList.ContainsKey(imageSize) ? ImageList[imageSize] : ImageList.Values.Last();
        }

        static string AlbumGetInfo_FindAlbumImg(JObject Json)
        {
            Dictionary<string, string> ImageList = new Dictionary<string, string>();

            dynamic DJson = Json;

            JArray Images = DJson.album.image;

            foreach (dynamic Image in Images)
            {
                string url = Image["#text"];
                string size = Image["size"];
                if (!string.IsNullOrEmpty(url) & !string.IsNullOrEmpty(size))
                    ImageList.Add(size, url);
            }

            if (ImageList.Count == 0)
                return "";

            return ImageList.ContainsKey(imageSize) ? ImageList[imageSize] : ImageList.Values.Last();
        }
    }
}