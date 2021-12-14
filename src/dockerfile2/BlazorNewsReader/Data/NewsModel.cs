using System;

namespace BlazorNewsReader.Data
{
    public class NewsModel
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public string Author { get; set; }

        public string Link { get; set; }

        public DateTime PublishDate { get; set; }

        public string[] Categories { get; set; }
    }
}
