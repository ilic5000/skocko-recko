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
            loadedWords = loadedWords.Select(word => word.Trim().ToUpperInvariant()).ToList();
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

            var result = new GuessBodyResponse()
            {
                Status = GuessStatus.TryAgain,
                FirstLetter = LetterStatus.DoesNotExistInWord,
                SecondLetter = LetterStatus.DoesNotExistInWord,
                ThirdLetter = LetterStatus.DoesNotExistInWord,
                FourthLetter = LetterStatus.DoesNotExistInWord,
                FifthLetter = LetterStatus.DoesNotExistInWord
            };

            var usedUpCharacters = new bool[5];
            var wordOfTheDayAsArray = wordOfTheDay.ToArray();
            var userAttemptAsArray = userAttempt.ToArray();

            // Check first correct guesses

            if (wordOfTheDayAsArray[0] == userAttemptAsArray[0])
            {
                result.FirstLetter = LetterStatus.RightPlaceRightLetter;
                usedUpCharacters[0] = true;
            }

            if (wordOfTheDayAsArray[1] == userAttemptAsArray[1])
            {
                result.SecondLetter = LetterStatus.RightPlaceRightLetter;
                usedUpCharacters[1] = true;
            }

            if (wordOfTheDayAsArray[2] == userAttemptAsArray[2])
            {
                result.ThirdLetter = LetterStatus.RightPlaceRightLetter;
                usedUpCharacters[2] = true;
            }

            if (wordOfTheDayAsArray[3] == userAttemptAsArray[3])
            {
                result.FourthLetter = LetterStatus.RightPlaceRightLetter;
                usedUpCharacters[3] = true;
            }

            if (wordOfTheDayAsArray[4] == userAttemptAsArray[4])
            {
                result.FifthLetter = LetterStatus.RightPlaceRightLetter;
                usedUpCharacters[4] = true;
            }

            // Check cointains in string somewhere

            var letterIndex = 0;
            if (!usedUpCharacters[letterIndex])
            {
                for (int i = 0; i < 5; i++)
                {
                    if (letterIndex == i || usedUpCharacters[i])
                    {
                        continue;
                    }

                    if (wordOfTheDayAsArray[i] == userAttemptAsArray[letterIndex])
                    {
                        usedUpCharacters[letterIndex] = true;
                        result.FirstLetter = LetterStatus.ExistsButWrongPlace;
                    }
                }
            }

            letterIndex = 1;
            if (!usedUpCharacters[letterIndex])
            {
                for (int i = 0; i < 5; i++)
                {
                    if (letterIndex == i || usedUpCharacters[i])
                    {
                        continue;
                    }

                    if (wordOfTheDayAsArray[i] == userAttemptAsArray[letterIndex])
                    {
                        usedUpCharacters[letterIndex] = true;
                        result.SecondLetter = LetterStatus.ExistsButWrongPlace;
                    }
                }
            }

            letterIndex = 2;
            if (!usedUpCharacters[letterIndex])
            {
                for (int i = 0; i < 5; i++)
                {
                    if (letterIndex == i || usedUpCharacters[i])
                    {
                        continue;
                    }

                    if (wordOfTheDayAsArray[i] == userAttemptAsArray[letterIndex])
                    {
                        usedUpCharacters[letterIndex] = true;
                        result.ThirdLetter = LetterStatus.ExistsButWrongPlace;
                    }
                }
            }

            letterIndex = 3;
            if (!usedUpCharacters[letterIndex])
            {
                for (int i = 0; i < 5; i++)
                {
                    if (letterIndex == i || usedUpCharacters[i])
                    {
                        continue;
                    }

                    if (wordOfTheDayAsArray[i] == userAttemptAsArray[letterIndex])
                    {
                        usedUpCharacters[letterIndex] = true;
                        result.FourthLetter = LetterStatus.ExistsButWrongPlace;
                    }
                }
            }

            letterIndex = 4;
            if (!usedUpCharacters[letterIndex])
            {
                for (int i = 0; i < 5; i++)
                {
                    if (letterIndex == i || usedUpCharacters[i])
                    {
                        continue;
                    }

                    if (wordOfTheDayAsArray[i] == userAttemptAsArray[letterIndex])
                    {
                        usedUpCharacters[letterIndex] = true;
                        result.FifthLetter = LetterStatus.ExistsButWrongPlace;
                    }
                }
            }

            if (result.FirstLetter == LetterStatus.RightPlaceRightLetter
                && result.SecondLetter == LetterStatus.RightPlaceRightLetter
                && result.ThirdLetter == LetterStatus.RightPlaceRightLetter
                && result.FourthLetter == LetterStatus.RightPlaceRightLetter
                && result.FifthLetter == LetterStatus.RightPlaceRightLetter)
            {
                result.Status = GuessStatus.Success;
            }

            return result;
        }
    }
}
