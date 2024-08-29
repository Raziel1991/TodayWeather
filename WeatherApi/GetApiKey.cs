using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WeatherApi
{
    internal class GetApiKey
    {
        private string _filePath;

        public GetApiKey(string filePath)
        {
            _filePath = filePath;
        }

        public string ReadAPIKey()
        {
            try
            {
                string apiKey = File.ReadAllText(_filePath);
                return apiKey.Trim();
            } catch (Exception ex) 
            {
                Console.WriteLine("sorry an error occurred: " +  ex.Message);
                return null;
            }            
        }
    }
}
