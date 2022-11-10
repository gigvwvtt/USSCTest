using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

[ApiController]
[Route("[controller]")]
public class LettersCountController : ControllerBase
{
    private readonly ILogger<LettersCountController> _logger;

    public LettersCountController(ILogger<LettersCountController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetLettersCount")]
    public List<PostData> LettersCount()
    {
        _logger.LogInformation($"{DateTime.Now} Получение данных из API");

        IEnumerable<PostData> data;
        try
        {
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