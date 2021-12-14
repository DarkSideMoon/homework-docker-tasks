using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorNewsReader.Data
{
    public interface IItcNewsReaderService
    {
        Task<IEnumerable<NewsModel>> GetNews();
    }

    public class ItcNewsReaderService : IItcNewsReaderService
    {
        private readonly HttpClient _httpClient;

        public ItcNewsReaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<NewsModel>> GetNews()
        {
            try
            {
                var feed = await _httpClient.GetStringAsync("https://feeds.feedburner.com/itcua");

                if(feed == null)
                    throw new Exception("Error! Couldn't read news!");

                var parsed = XDocument.Parse(feed);
                var items = parsed.Root
                    .Descendants()
                    .First(i => i.Name.LocalName == "channel")
                    .Elements()
                    .Where(i => i.Name.LocalName == "item")
                    .Select(x => new NewsModel 
                    { 
                        Title = x.Elements().First(i => i.Name.LocalName == "title").Value,
                        Link = x.Elements().First(i => i.Name.LocalName == "link").Value,
                        Text = x.Elements().First(i => i.Name.LocalName == "description").Value,
                        Author = x.Elements().First(i => i.Name.LocalName == "creator").Value,
                        PublishDate = Convert.ToDateTime(x.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                        Categories = x.Elements().Where(i => i.Name.LocalName == "category").Select(x => x.Value).ToArray()
                    });

                return items;
            }
            catch (Exception)
            {
                throw new Exception("Error!");
            }
        }
    }
}
