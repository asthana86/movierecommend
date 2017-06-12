using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace movierecommender.Models
{
    public class Movies
    {
        public readonly static int moviestorecommend = 6;
        private IHostingEnvironment _env;

        public List<MovieList> MoviesList { get; set; }
        public class MovieList
        {
            public int MovieID { get; set; }
            public string MovieName { get; set; }
        }

        public Movies(IHostingEnvironment env)
        {
            _env = env;
        }

        public Movies()
        {
            MoviesList = new List<MovieList>();
            Stream fileReader = File.OpenRead("Content/IMDB Movie Titles.csv");
            StreamReader reader = new StreamReader(fileReader);
            try
            {
                bool header = true;
                int index = 0;
                var line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    int MovieID = Int32.Parse(fields[0].ToString().TrimStart(new char[] { '0' }));
                    string MovieName = fields[1].ToString();
                    MoviesList.Add(new MovieList() { MovieID = MovieID, MovieName = MovieName });
                    index++;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose(); 
                }
            }
        }
    }
}