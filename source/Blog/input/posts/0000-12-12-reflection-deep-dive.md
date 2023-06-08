---
Title: "Reflection deep dive"
Published: 2020-01-23 02:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [C#, .NET]
IsPost: False
---

https://www.codeproject.com/articles/125128/reflection-is-slow-or-fast-a-practical-demo

https://weblogs.asp.net/craigshoemaker/the-performance-of-everyday-things

# Making decisions by Type

```
abstract class Animal { }
class Dog : Animal { }
class Cat : Annimal { }

public static void Main() 
{
    Animal pet = new Dog();
    if (pet.GetType().ToString() == "Dog")
        Console.WriteLine("We can be friends");
    else
        Console.WriteLine("Achoo!");
}
```