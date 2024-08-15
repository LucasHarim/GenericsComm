using System;
using System.Collections.Generic;


public class ServicesTest : ServicesBase
{
    public ServicesTest()
    {
        
    }

    public static void SayHello()
    {
        Console.Write($"[{DateTime.Now}] - Hello from NetMQServer!\n");
    }

    public static string CheckMsg(string msg)
    {
        return $"[{DateTime.Now}] - I got the message: '{msg}'";
    }

    public static int Sum(int a, int b)
    {
        return a+b;
    }

    public static double Divide(double a, double b)
    {
        return a/b;
    }

    public static List<int> GetList()
    {
        return new List<int> {1, 2, 3};
    }

    public static Dictionary<string, float> GetDictionary()
    {
        Dictionary<string, float> dict = new();
        dict.Add("x", 10.0f);
        dict.Add("y", 20.0f);
        dict.Add("z", 30.0f);
        
        return dict;
    }

}