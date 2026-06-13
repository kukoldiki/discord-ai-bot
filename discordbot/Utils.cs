namespace discordbot;

public class Utils
{
    public static List<string> SplitByLength(string text, int maxLength)
    {
        var result = new List<string>();

        for (int i = 0; i < text.Length; i += maxLength)
        {
            result.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
        }

        return result;
    }
}