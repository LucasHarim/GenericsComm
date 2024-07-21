using System;

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

}