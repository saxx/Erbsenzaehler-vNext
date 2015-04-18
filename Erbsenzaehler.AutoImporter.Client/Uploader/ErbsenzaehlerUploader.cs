﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using Erbsenzaehler.AutoImporter.Configuration;
using Newtonsoft.Json;

namespace Erbsenzaehler.AutoImporter.Client.Uploader
{
    public class ErbsenzaehlerUploader
    {
        private readonly ErbsenzaehlerConfiguration _config;


        public ErbsenzaehlerUploader(ErbsenzaehlerConfiguration config)
        {
            _config = config;
        }


        public void Upload(string filePath)
        {
            Console.WriteLine("Uploading file to Erbsenzähler ...");
            Console.WriteLine("Erbsenzähler base URL is {0}.", _config.Url);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.Url)
            };

            Console.WriteLine("Building POST content ...");
            var postContent = new ObjectContent(typeof (object), new
            {
                _config.Username,
                _config.Password,
                _config.Account,
                _config.Importer,
                File = File.ReadAllBytes(filePath)
            }, new JsonMediaTypeFormatter());

            Console.WriteLine("Uploading ...");
            var postResult = httpClient.PostAsync("Api/Import", postContent).Result;
            try
            {
                postResult.EnsureSuccessStatusCode();
            }
            catch
            {
                Console.WriteLine("Unable to upload to Erbsenzähler: " + postResult.StatusCode + " " + postResult.ReasonPhrase);
                return;
            }

            var importResult = JsonConvert.DeserializeObject<dynamic>(postResult.Content.ReadAsStringAsync().Result);
            Console.WriteLine("Upload completed. {0} lines imported, {1} lines ignored.", importResult.ImportedCount, importResult.IgnoredCount);
        }
    }
}