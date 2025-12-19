internal class StringHelper  
{
    public static List<string> find_chinese(string fullWord)
    {
        // string clxh = "亚特重工牌TZ5317GJBZG8";
        int index = -1;
        for (int i = fullWord.Length - 1; i >= 0; i--)
        {
            if (char.IsSurrogate(fullWord[i]) || (fullWord[i] >= 0x4e00 && fullWord[i] <= 0x9fa5))
            {
                index = i;
                break;
            }
        }
        List<string> list = new List<string>();
        if (index != -1)
        {
            list.Add(fullWord.Substring(index + 1));
            fullWord = fullWord.Substring(0, index + 1);
            list.Add(fullWord);
        }
        return list;
    }
}