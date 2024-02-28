using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProtocol
{
    internal class Translator
    {
        private readonly Dictionary<string, Dictionary<string, string>> translations;
        public string CurrentLanguage { get; private set; } = "es-es";
        public Translator()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();

            // Carga las traducciones desde archivos JSON en la carpeta especificada
            string commandsFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "languages");
            if (Directory.Exists(commandsFolderPath))
            {
                LoadTranslations(commandsFolderPath);
            }
        }

        public void ChangeLanguage(MainWindow argWindowm, string language)
        {
            // Cambia el idioma actual
            if (translations.ContainsKey(language))
            {
                CurrentLanguage = language;
                argWindowm.TranslateAll();
                ChangeIconLanguage(argWindowm);
            }
        }

        private void ChangeIconLanguage(MainWindow argWindowm)
        {
            try
            {
                ///languages/flagsIcons/ES.png
                argWindowm.IconActualLanguage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "languages/flagsIcons/" + CurrentLanguage + ".png")));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void LoadTranslations(string languagesFolderPath)
        {
            // Lee cada archivo JSON en la carpeta de idiomas y carga las traducciones
            foreach (string filePath in Directory.GetFiles(languagesFolderPath, "*.json"))
            {
                string languageCode = Path.GetFileNameWithoutExtension(filePath);

                try
                {
                    // Lee el contenido del archivo JSON
                    string jsonContent = File.ReadAllText(filePath);

                    // Deserializa el JSON en un diccionario de traducciones
                    Dictionary<string, string> translationDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

                    // Agrega el diccionario al diccionario principal usando el código de idioma como clave
                    translations[languageCode] = translationDict;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar traducciones para el idioma {languageCode}: {ex.Message}");
                }
            }
        }

        public string Translate(string key, string language)
        {
            // Asegúrate de que el idioma proporcionado esté cargado
            if (!translations.ContainsKey(language))
            {
                throw new ArgumentException("Idioma no cargado", nameof(language));
            }

            // Obtiene el diccionario de traducciones correspondiente al idioma
            var translationDict = translations[language];

            // Obtiene la traducción utilizando la clave proporcionada
            var translation = translationDict.TryGetValue(key, out var value) ? value : null;

            // Devuelve la traducción o la clave si no se encuentra la traducción
            return translation ?? key;
        }
    }
}
