using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

[ApiController]
[Route("[controller]")]
public class VkPostsJobController : ControllerBase
{
    private readonly ILogger<VkPostsJobController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IVkDataRepository _repository;

    public VkPostsJobController(ILogger<VkPostsJobController> logger, IConfiguration configuration, IVkDataRepository repository)
    {
        _logger = logger;
        _configuration = configuration;
        _repository = repository;
    }

    [HttpPut(Name = "PutDataToDb")]
    public List<PostData> PutDataToDb()
    {
        var posts = LettersCount();
        
        try
        {
            foreach (var post in posts)
            {
                if (_repository.GetPost(post).id == post.id)
                    _repository.PutPost(post);
                else
                    _repository.AddPosts(post);
                _logger.LogInformation($"{DateTime.Now} Успешно добавлен в базу данных элемент с id - {post.id}");
            }
            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.Now} Ошибка сохранения в базе данных");
            return posts;
        }
    }
    
    private IEnumerable<PostData>? GetVkPosts(int user_id = 1)
    {
        var list = new List<PostData>();

        var token =  _configuration["VkToken"];
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(
            $"https://api.vk.com/method/wall.get?access_token={token}&owner_id={user_id}&count=5&v=5.131");
        httpWebRequest.Method = "POST";
        var response = (HttpWebResponse)httpWebRequest.GetResponse();
        
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            var responseResult = reader.ReadToEnd();
            
            using (var document = JsonDocument.Parse(responseResult))
            {
                var itemsElement = document.RootElement.GetProperty("response").GetProperty("items");
                list = JsonSerializer.Deserialize<List<PostData>>(itemsElement);
            }
            
            return list;
        }
    }
    
    private List<PostData> LettersCount()
    {
        _logger.LogInformation($"{DateTime.Now} Получение данных из API");

        IEnumerable<PostData> data;
        try
        {
            data = GetVkPosts();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{DateTime.Now} Не удалось получить данные");
            throw;
        }

        var listOfTextsAndIds = data.Select(post => new {post.text,  post.id}).Where(y => y.text.Length > 0).ToList();
        if (listOfTextsAndIds.Count == 0)
        {
            var exception = new Exception();
            _logger.LogError(exception, $"{DateTime.Now} Все тексты пусты!");
            throw exception;
        }

        var posts = data.ToList();

        _logger.LogInformation($"{DateTime.Now} Подсчёт повторяющихся букв в {listOfTextsAndIds.Count} текстах начался");

        for (var j = 0; j < listOfTextsAndIds.Count; j++)
        {
            var text = listOfTextsAndIds[j].text.ToLower().Where(char.IsLetter).OrderBy(x => x).ToArray();
            //Array.Sort(text);

            var cyrillicLength = 'я' - 'а' + 2; // первое значение + буква ё
            var latinLength = 'z' - 'a' + 1;
            var array = new int[cyrillicLength + latinLength];

            foreach (var ch in text)
            {
                //латинские символы 
                if (ch < 'а')
                {
                    array[ch - 'a']++;
                }
                //кириллические символы
                else
                {
                    if (ch == 'ё')
                        array[latinLength + 6]++;
                    else
                    {
                        var index = latinLength + ch - 'а';
                        if (ch <= 'е')
                            array[index]++;
                        else
                            array[index + 1]++;
                    }
                }
            }

            var result = new StringBuilder();
            for (var i = 0; i < cyrillicLength + latinLength; i++)
            {
                if (array[i] != 0)
                {
                    if (i < 26)
                        result.Append($"{(char)('a' + i)} - {array[i]}");
                    else
                    {
                        var index = 'а' + i - latinLength;

                        if (index <= 1077) //индекс до буквы ё
                            result.Append($"{(char)index} - {array[i]}");
                        else if (index == 1105) //индекс буквы ё
                            result.Append($"{(char)index} - {array[i]}");
                        else //индекс после буквы ё
                            result.Append($"{(char)(index - 1)} - {array[i]}");
                    }
                    if (i != cyrillicLength + latinLength - 1) result.Append(", ");
                }
            }

            foreach (var post in posts.Where(post => post.id == listOfTextsAndIds[j].id))
            {
                post.lettersInfo = result.ToString();
            }
        }

        _logger.LogInformation($"{DateTime.Now} Подсчёт повторяющихся букв в {listOfTextsAndIds.Count} текстах закончился");

        return posts;
    }
}