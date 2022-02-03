using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Skocko.Api.Models.Responses
{
    public class GuessResponse
    {
        public string? TodayWordId { get; set; }
        public WordExistsStatus DoesWordExist { get; set; }
        public GuessBodyResponse? Response { get; set; }
    }

    public class GuessBodyResponse
    {
        public GuessStatus Status { get; set; }
        public LetterStatus FirstLetter { get; set; }
        public LetterStatus SecondLetter { get; set; }
        public LetterStatus ThirdLetter { get; set; }
        public LetterStatus FourthLetter { get; set; }
        public LetterStatus FifthLetter { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LetterStatus
    {
        DoesNotExistInWord,
        ExistsButWrongPlace,
        RightPlaceRightLetter
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GuessStatus
    {
        Success,
        TryAgain
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WordExistsStatus
    {
        WordDoesNotExist,
        Exists
    }
}