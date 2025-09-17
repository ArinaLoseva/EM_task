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
                    return BadRequest("���� �� ������");

                if (!file.FileName.EndsWith(".txt"))
                    return BadRequest("������ txt ����� ���������");


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

                // ����� � ������� ��� ��������
                Console.WriteLine("=== ������ ��������� � ����������� ������ ===");
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
                return StatusCode(500, $"������ ��� �������� �����: {ex.Message}");
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

                //����� �� ����� (��������� + �������)
                var parts = trimmedLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                //��������� ��������� � �������
                var platformName = parts[0].Trim();
                var locationsString = parts[1].Trim();

                //����� �������
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
                    return BadRequest("������� �� �������");
                }

                // ���������, ��� ������� platforms ���������� � �� ������
                if (platforms == null || platforms.Count == 0)
                {
                    return BadRequest("������� ��������� ���� � �������");
                }

                var result = SearchLocationInPlatforms(request.Location);

                return Ok(new { result = string.Join(", ", result) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"������ ��� ������: {ex.Message}");
            }
        }

        // ����� ��� ������� ������
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
                    result.Add("��� ������ ������� ��������� �������� ���");
                    return result;
                }
            }

            return result;
        }
    }
}