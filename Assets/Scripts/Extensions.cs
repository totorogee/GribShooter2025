using System;

public static class Extensions
{

    public static T NextEnum<T>(this T src) where T : Enum
    {
        T[] Arr = (T[])Enum.GetValues(typeof(T));
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static T PrevEnum<T>(this T src) where T : Enum
    {
        T[] Arr = (T[])Enum.GetValues(typeof(T));
        int j = Array.IndexOf<T>(Arr, src) - 1;
        return (j == -1) ? Arr[Arr.Length - 1] : Arr[j];
    }

    public static T RandomEnum<T>(this T src) where T : Enum
    {
        Array values = (T[])Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }
}