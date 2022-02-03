using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skocko.Api.Models.Requests;
using Skocko.Api.Models.Responses;
using Skocko.Api.Services;
using System.Configuration;

namespace Skocko.Api.Controllers
{
    [ApiController]
    public class WordsController : Controller
    {
        public WordsService WordsService { get; }
        public IConfiguration Configuration { get; }

        public WordsController(WordsService wordsService, IConfiguration configuration)
        {
            WordsService = wordsService;
            Configuration = configuration;
        }

        [HttpGet("api/words")]
        [AllowAnonymous]
        public ActionResult GetAllDictionaryWords()
        {
            return Ok(WordsService.AllExisting5LetterWords);
        }

        [HttpGet("api/words/check-if-exists-in-serbian-language")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckIfWordExistInDictionary([FromQuery] string word)
        {
            if (await WordsService.DoesWordExistInSerbianLanguageAtAll(word))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("api/words/word-of-the-day")]
        [AllowAnonymous]
        public async Task<ActionResult> GetWordOfTheDay()
        {
            var wordOfTheDay = await WordsService.GetTodaysWordId();

            if (string.IsNullOrEmpty(wordOfTheDay))
            {
                return NotFound();
            }

            var response = new WordOfTheDayResponse()
            {
                WordId = wordOfTheDay
            };

            return Ok(response);
        }

        [HttpPost("api/words/{wordId}/guess-word")]
        [AllowAnonymous]
        public async Task<ActionResult> Guess([FromRoute] string wordId, [FromBody] GuessRequest body)
        {
            var response = new GuessResponse();

            var doesWordExist = await WordsService.DoesWordExistInSerbianLanguageAtAll(body.Word);

            if (!doesWordExist)
            {
                response.DoesWordExist = WordExistsStatus.WordDoesNotExist;
                return Ok(response);
            }

            response.DoesWordExist = WordExistsStatus.Exists;

            // Check wordId

            var todaysWord = WordsService.GetTodaysWord();

            if (todaysWord == null)
            {
                return StatusCode(500, "Today's word not available");
            }

            // TODO: Add caching
            var hashedWordOfTheDay = WordsService.GetHashedWord(todaysWord);
           
            if (wordId != hashedWordOfTheDay)
            {
                // Client needs to refresh the view with new word of the day
                return StatusCode(400, "New word has been set for guessing");
            }

            response.TodayWordId = hashedWordOfTheDay;

            var checkedLettersResponse = await WordsService
                                                .CheckLetters(wordOfTheDay: todaysWord, 
                                                              userAttempt: body.Word);  ;

            if (checkedLettersResponse == null)
            {
                return StatusCode(500, "Something went wrong while comparing words");
            }

            response.Response = checkedLettersResponse;

            return Ok(response);
        }
    }
}
