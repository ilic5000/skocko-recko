using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Skocko.Api.Models.Responses;

namespace Skocko.Api.Services
{
    public class WordsService
    {
        #region Static stuff
        public static HashSet<string>? AllExisting5LetterWords { get; set; }
        // date,word
        public static Dictionary<DateTime, string>? WordsToBeGuessed { get; set; }
        #endregion

        public EncryptionService EncryptionService { get; }
        public IConfiguration Configuration { get; }

        static WordsService()
        {
            InitializeStaticFiles();
        }

        public WordsService(EncryptionService encryptionService, IConfiguration configuration)
        {
            EncryptionService = encryptionService;
            Configuration = configuration;
        }

        private static void InitializeStaticFiles()
        {
            var loadedWords = File.ReadAllLines($"{AppContext.BaseDirectory}Files{Path.DirectorySeparatorChar}lat-all-5-letter-words-in-serbian.txt").ToList();
            loadedWords = loadedWords.Select(word =>word.Trim().ToUpperInvariant()).ToList();
            loadedWords = loadedWords.Where(word => word.Length == 5).ToList();
            AllExisting5LetterWords = loadedWords.ToHashSet();

            var guessWordFileLoaded = File.ReadAllText($"{AppContext.BaseDirectory}Files{Path.DirectorySeparatorChar}guessWords.json");
            WordsToBeGuessed = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(guessWordFileLoaded);
        }

        public async Task<bool> DoesWordExistInSerbianLanguageAtAll(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            if (AllExisting5LetterWords != null)
            {
                word = word.ToUpperInvariant();
                return AllExisting5LetterWords.Contains(word);
            }

            return false;
        }

        internal async Task<string?> GetTodaysWordId()
        {
            var foundWord = GetTodaysWord();

            if (foundWord == null)
            {
                return null;
            }

            var encryptedWordOfTheDay = GetHashedWord(foundWord);

            return encryptedWordOfTheDay;
        }

        internal string? GetHashedWord(string? foundWord)
        {
            var hashKey = Configuration["HashKey"];
            var encryptedWordOfTheDay = EncryptionService.HashString(foundWord, hashKey);
            return encryptedWordOfTheDay;
        }

        internal string? GetTodaysWord()
        {
            var serbianDate = GetDateInSerbia();
            var foundWord = WordsService.WordsToBeGuessed?.GetValueOrDefault(serbianDate);
            return foundWord;
        }

        internal DateTime GetDateInSerbia()
        {
            var utcTime = DateTime.UtcNow;
            var serbianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            var serbianDate = TimeZoneInfo.ConvertTimeFromUtc(utcTime, serbianTimeZone).Date;
            return serbianDate;
        }

        internal async Task<GuessBodyResponse?> CheckLetters(string? wordOfTheDay, string? userAttempt)
        {
            wordOfTheDay = wordOfTheDay?.ToUpperInvariant();
            userAttempt = userAttempt?.ToUpperInvariant();

            if (string.IsNullOrEmpty(wordOfTheDay)
                || string.IsNullOrEmpty(userAttempt))
            {
                return null;
            }

            var wordOfTheDayAsArray = wordOfTheDay.ToList();
            var userAttemptAsArray = userAttempt.ToList();

            var result = new GuessBodyResponse()
            {
                FirstLetter = GetLetterStatus(wordOfTheDayAsArray, userAttemptAsArray, indexOfLetter:0),
                SecondLetter = GetLetterStatus(wordOfTheDayAsArray, userAttemptAsArray, indexOfLetter: 1),
                ThirdLetter = GetLetterStatus(wordOfTheDayAsArray, userAttemptAsArray, indexOfLetter: 2),
                FourthLetter = GetLetterStatus(wordOfTheDayAsArray, userAttemptAsArray, indexOfLetter: 3),
                FifthLetter = GetLetterStatus(wordOfTheDayAsArray, userAttemptAsArray, indexOfLetter: 4),
            };

            if (result.FirstLetter == LetterStatus.RightPlaceRightLetter
                && result.SecondLetter == LetterStatus.RightPlaceRightLetter
                && result.ThirdLetter == LetterStatus.RightPlaceRightLetter
                && result.FourthLetter == LetterStatus.RightPlaceRightLetter
                && result.FifthLetter == LetterStatus.RightPlaceRightLetter)
            {
                result.Status = GuessStatus.Success;
            }
            else
            {
                result.Status = GuessStatus.TryAgain;
            }

            return result;
        }

        private LetterStatus GetLetterStatus (List<char> word, List<char> attempt, int indexOfLetter)
        {
            var result = LetterStatus.DoesNotExistInWord;

            if (attempt[indexOfLetter] == word[indexOfLetter])
            {
                result = LetterStatus.RightPlaceRightLetter;
            }
            else if (word.Contains(attempt[indexOfLetter]))
            {
                result = LetterStatus.ExistsButWrongPlace;
            };

            return result;
        }
    }
}
