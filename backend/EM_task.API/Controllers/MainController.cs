using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EM_task.API.Controllers
{
    [ApiController]
    [Route("api/main")]
    public class MainController : ControllerBase
    {
        private static Dictionary<string, List<string>> platforms = new Dictionary<string, List<string>>();

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (file == null || file.Length == 0)
                    return BadRequest("Файл не выбран");

                if (!file.FileName.EndsWith(".txt"))
                    return BadRequest("Только txt файлы разрешены");


                string fileContent;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    fileContent = await reader.ReadToEndAsync();
                }

                var parsedData = ParseFileContent(fileContent);

                platforms.Clear();
                foreach (var item in parsedData)
                {
                    platforms[item.Key] = item.Value;
                }

                // вывод в консоль для проверки
                Console.WriteLine("=== ДАННЫЕ ЗАГРУЖЕНЫ В ОПЕРАТИВНУЮ ПАМЯТЬ ===");
                foreach (var platform in platforms)
                {
                    Console.WriteLine($"{platform.Key}: {string.Join(", ", platform.Value)}");
                }

                return Ok(new
                {
                    message = file.FileName,
                    totalPlatforms = platforms.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        private Dictionary<string, List<string>> ParseFileContent(string fileContent)
        {
            var platforms = new Dictionary<string, List<string>>();

            var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                //делим на части (платформа + локации)
                var parts = trimmedLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                //разделяем платформы и локации
                var platformName = parts[0].Trim();
                var locationsString = parts[1].Trim();

                //делим локации
                var locations = locationsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(l => l.Trim())
                                              .ToList();

                platforms[platformName] = locations;
            }

            return platforms;
        }

        [HttpPost("search")]
        public IActionResult SearchLocation([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Location))
                {
                    return BadRequest("Локация не указана");
                }

                // Проверяем, что словарь platforms существует и не пустой
                if (platforms == null || platforms.Count == 0)
                {
                    return BadRequest("Сначала загрузите файл с данными");
                }

                var result = SearchLocationInPlatforms(request.Location);

                return Ok(new { result = string.Join(", ", result) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при поиске: {ex.Message}");
            }
        }

        // класс для запроса поиска
        public class SearchRequest
        {
            public string Location { get; set; }
        }

        private List<string> SearchLocationInPlatforms(string locationPath)
        {
            var result = new List<string>();

            var pathParts = locationPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";

            for (int i = 0; i < pathParts.Length; i++)
            {
                currentPath += "/" + pathParts[i];

                foreach (var platform in platforms)
                {
                    foreach (var location in platform.Value)
                    {
                        if (location == currentPath)
                        {
                            if (!result.Contains(platform.Key))
                            {
                                result.Add(platform.Key);
                            }
                        }
                    }
                }

                if (i == 0 && !result.Any())
                {
                    result.Add("Для данной локации рекламных платформ нет");
                    return result;
                }
            }

            return result;
        }
    }
}