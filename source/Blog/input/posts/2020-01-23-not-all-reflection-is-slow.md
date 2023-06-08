---
Title:  "Not all reflection is slow"
Published:   2020-01-23 02:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [C#, .NET]
IsPost: False
---

In a [previous article](#), I mentioned that overriding the base implementation of `Equals()` for our own value types is important.  The base implementation uses a costly bit of reflection to compare by value. In this article, I'll be covering the just enough about reflection to make my argument, `//TOD FIX and which parts are slow and which are fast`

# What is Reflection?

Simply put, reflection is a way to use code to inspect our assemblies (think DLL's) and the types inside them to make decisions. We can also access instances, create instances, and invoke methods.

`System.Type` is our entry point to reflection.

# Reflection isn't slow

For what it does, reflection is actually very fast.  It's just slower than not using reflection in the first place.

# When should Reflection should be used

Reflection should be generally avoided. There are other patterns and practices available that won't send you down a potentially dangerous rabbit hole.

There are times, unfortunately that we are handed a steaming pile of spaghetti that we need to work with, and cannot modify the source code. Reflection can be a huge help in, but can also be quite dangerous.

Green field code shouldn't need any reflection at all, but you've got to start somewhere so it's perfectly acceptable to create a test or proof of concept app to sharpen your reflection skills.

# Least Performant

```
Activator.CreateInstance<T>
GetCustomAttributes()
GetPropertyInfo()
GetMethodInfo()
GetEventInfo()
GetFieldInfo()
Type.InvokeMember()
```
# Where Reflection really get's you into trouble
If you use reflection to load plugins during startup it's fine.

What will get you into trouble is calling reflection methods in a tight loop

```
List<MyType> listof1000items = Get1000Items();
foreach (var item in listof1000items) 
{
    var props = item.GetPropertyInfo();
} 
```
# Mistakes

```
typeof(T).IsAssignableFrom(obj.GetType())
```

use instead:

```
if (obj is T) { ... }
```