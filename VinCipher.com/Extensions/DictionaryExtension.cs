using Domaine.Entities;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace VinCipher.Extensions
{
    public static class DictionaryExtension
    {
        const string _PATH_TO_BIN = "Files";
        public static void SaveToFile(this Dictionary<string, string> dic, string vin)
        {
            string jsonString = JsonSerializer.Serialize(dic);
            File.WriteAllText(GetPath(vin), jsonString);
        }
        public static Dictionary<string, string> loadBinFile(string vin)
        {
            FileStream fs = null;
            try
            {
                string jsonText = File.ReadAllText(GetPath(vin));
               return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            }
            catch (Exception e)
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }

                return null;
            }
        }

        public static string GetPath(string vin)
        {
            var folderName = Path.Combine(_PATH_TO_BIN, vin.Substring(0, 3));
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            bool exists = System.IO.Directory.Exists(pathToSave);
            if (!exists)
                System.IO.Directory.CreateDirectory(pathToSave);
            var fileName = vin + ".json";
            return Path.Combine(pathToSave, fileName);

        }

      
    }
}
