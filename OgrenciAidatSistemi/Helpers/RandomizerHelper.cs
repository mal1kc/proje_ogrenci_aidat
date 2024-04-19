using System.Text;

public static class RandomizerHelper
{
    public static readonly Random random = new Random();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(int length)
    {
        var stringBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }

    /* public static int GenerateRandomNumber(int min, int max) */
    /* { */
    /*     return random.Next(min, max + 1); */
    /* } */
}
