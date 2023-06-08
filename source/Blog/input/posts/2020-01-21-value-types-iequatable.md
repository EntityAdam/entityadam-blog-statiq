---
Title:  "Always use IEquatable<T> for Value Types"
Published:   2020-01-21 02:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: C#
---

You should always implement `IEquatable<T>` when checking for equality on value types. In this article I'll go into a bit of depth on how `Equals()` behaves on `System.Object` and one of it's derived class, `System.ValueType`.

> Changes  
[2020-1-28] Fixed some grammatical and spelling errors.

> Author's Note: There are a few 'broken' links in this article. If they are pointing to a non-existent article on my own site, I just haven't written it yet.  Please bear with me as I build out my articles.  I don't want to cover too much in any one post but there are important topics that would warrant an entire article.  Please keep checking back as I fill in the blanks!

# The Root of .NET
`System.Object` is the ultimate base class of all .NET classes. It provides several methods which can be useful.  I will at some point be covering all the methods of `System.Object` but this first article focuses on `Equals()` with specifically with Value Types.

# Value Types
When working in .NET, we either have a reference type, or a value type.  

Value types are:

- Integral types:
    - byte
    - int
    - long
    - etc.
- Floating-point numeric types:
    - float
    - double
    - decimal
- bool
- char

All of these types that we commonly use in C# as keywords like `int` below, are actually aliases to a .NET type defined with the `struct` keyword

```cs
int number = 42;
```

> Note: Ever wonder why 42 is always used in example code? In Douglas Adams' book, _The Hitchhiker's Guide to the Galaxy_ a super computer was constructed to answer the meaning of life. At the end of it's magnificent calculations the computer responded. "The answer to the ultimate question of life, the universe and everything is 42."

Value Types derive from `System.ValueType` which like all objects in .NET derive from `System.Object`. The `ValueType` type provides overrides to the `object.Equals()` method which are better suited to compare value types, but there's a catch.

# ValueType base implementation of Equals
Unlike it's parent, the base implementation on `Equals()` on `System.ValueType` does check for value equality.

Here's a snippet from mscorlib (just the part that I'm making a point about, check out the full source here: [Microsoft Resource Source](https://referencesource.microsoft.com/#mscorlib/system/valuetype.cs,915ba3e46633f948)

```cs
FieldInfo[] thisFields = thisType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

for (int i=0; i<thisFields.Length; i++) {
    thisResult = ((RtFieldInfo)thisFields[i]).UnsafeGetValue(thisObj);
    thatResult = ((RtFieldInfo)thisFields[i]).UnsafeGetValue(obj);
    
    if (thisResult == null) {
        if (thatResult != null)
            return false;
    }
    else
    if (!thisResult.Equals(thatResult)) {
        return false;
    }
}
```

As you can see, this override of `Equals()` is using some Reflection. Although [Not all reflection is slow (Article pending)](#), this call to `Type.GetFields()` can be quite costly. Since we own the type, we can and should override the `Equals()` method.

> If you're using a `struct` or value type over a `class` in an attempt to try and finaegle some performance gains, check out this article on [Structs are not light-weight classes(Article pending)](#).

# Dude, my dad owns his own dealership

In the below example `struct`, we're going OO model a vehicle for a automotive dealership. In this representation of a vehicle it's going to have a Make (Manufacturer), Model and Vehicle Identification Number (VIN) which is a unique serial number for a vehicle.

```
struct Vehicle
{
    public Vehicle(string make, string model, string vin)
    {
        Make = make;
        Model = model;
        Vin = vin;
    }
    public string Make { get; }
    public string Model { get; }
    public string Vin { get; }
}
```
With this model the make and model really don't matter since we have a permanent unique identifier in the form of a VIN. So our `IEquatable<T>` implementation of `Equals(Vehicle other)` only cares about comparing one field by value. Where if we left .NET to it's own devices, it would check each non static property for value equality.

This is the key point in why we should override the base implementation. It's not entirely about performance. ***We are the domain experts, and we know what makes our objects equal.***

> **Aside**: Vehicle Identification Numbers are supposed to be permanent.  If someone takes a grinder to the VIN (as in, `Vin == null`), or there's a duplicate VIN because somone criminally swapped the VIN's, the best course of action is to have the application throw an exeption, and that's how this example will behave.  YMMV if your use-case is different. **When in doubt, test it out**.

I'm going to opt-out of showing you what it looks like to override `System.ValueType.Equals()` by itself. It just isn't necessary to show the extra code required to null check and type check. Here's (almost) all the code.

```cs
struct Vehicle : IEquatable<Vehicle>
{
    public Vehicle(string make, string model, string vin)
    {
        Make = make;
        Model = model;
        Vin = vin;
    }
    public string Make { get; }
    public string Model { get; }
    public string Vin { get; }

    public bool Equals(Vehicle other) =>
        string.Equals(Vin, other.Vin, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) =>
        obj is Vehicle v && Equals(v);

    public override int GetHashCode() => Vin.GetHashCode();
}
```

# The IEquatable<T> Implementation

```cs
    public bool Equals(Vehicle other) =>
        string.Equals(Vin, other.Vin, StringComparison.OrdinalIgnoreCase);
```

`IEquatable<T>` is only concerned that we implement this method, which is an overload of the `Equals()` that accepts a parameter of type `Vehicle`.

Since the VIN is a string, and is case-insensitive, I'm making sure the string comparison is also case-insensitive. 

I've also deliberatly chose `OrdinalIgnoreCase` because a VIN is an ISO standard format and has no culture-sensitivity. We're not an exotic dealership so we don't have to deal with foreign cars that don't participate in ISO.



Example: Calling the overload `Equals(Vehicle other)`
```
    var cruiser1 = new Vehicle("Ford", "Crown Victoria", "P71-ABCDEF")
    var cruiser2 = new Vehicle("Ford", "Crown Victoria", "P71-HIJKLM")
    if (cruiser1.Equals(cruiser2))
        Console.WriteLine("Vehicles Match");
    else 
        Console.WriteLine("Vehicles Do not match");

//output : Vehicles Do not match
```

> Fun Note: P71 is part of a VIN on Ford Crown Victoria vehicles which were issued as fleet vehicles for law enforcement. Commonly referred to as a [Crown Victoria Police Interceptor](https://en.wikipedia.org/wiki/Ford_Crown_Victoria_Police_Interceptor).

# Collections

No, this isn't about your late payment repossession.  You hopefully have more than one car in your dealership. Either waiting to be leased, sold, or brought one in for serice or trade-in.

If we're going to have a collection for examples, a `HashTable<Vehicle>` or a `Dictionary<Vehicle, Person>` we will additionally want to make sure our type can be used in a collection.  This is one major reason [MSDN](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1) urges us to also override the base implementation of `Equals()` and `GetHashCode()` on our type if we implement `IEquatable<T>`

```cs
    public override bool Equals(object obj) =>
        obj is Vehicle v && Equals(v);

    public override int GetHashCode() => Vin.GetHashCode();
```

The base implementation accepts and `object` as a parameter instead of a the `Vehicle` type we declared. I'm using the `is` keyword as a [type pattern](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/is#type-pattern) to check if the incoming `obj` can be converted to a `Vehicle`.  If it can't, it will return false.  If it can, it will assign the converted `obj` to the variable `v`, and pass that to our `IEquatable<T>` implementation.

The `GetHashCode()` override must always be consistent with our `Equals()` override. Since we are only checking for equality on the unique VIN, I'm only returning `Vin.GetHashCode()`, which `System.String` knows how to do for me.

# Smooth operator

We've essentially completed our task. However, on the note of consistency, if we've gone through the trouble of implementing `IEquatable<T>`, overriding `GetHashCode` and `Equals()` properly then we should also properly override the `==` and `!=` operators.  I'm sure there are valid reasons not to do so, but not wanting to write more code is not one of them, and in my opinion YAGNI doesn't apply here.  If we've come this far I would consider overridding the operators, and we can remove them later if there's a valid reason to.

```cs
struct Vehicle : IEquatable<Vehicle>
{
    public Vehicle(string make, string model, string vin)
    {
        Make = make;
        Model = model;
        Vin = vin;
    }
    public string Make { get; }
    public string Model { get; }
    public string Vin { get; }

    public bool Equals(Vehicle other) =>
        string.Equals(Vin, other.Vin, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) =>
        obj is Vehicle v && Equals(v);

    public override int GetHashCode() => Vin.GetHashCode();

    public static bool operator==(Vehicle v1, Vehicle v2) =>
        v1.Equals(v2);
    public static bool operator!=(Vehicle v1, Vehicle v2) =>
        !v1.Equals(v2);
}
```

# Spot free rinse

I know that was a long article for something that should be pretty simple. That's classic .NET for you. We covered the root of the object heirarchy, value types and some of the in's and out's of equality.  Most importantly I hope I made the point that we should always use `IEquatable<T>` on our value types. If you forget the rest of the article that's OK, just remember this bit: The main reason why we should define equality for our types, is because ***We are the domain experts, and we know what makes our types equal.***

```
Other articles referenced:
Pending Article: Not all reflection is slow.
Pending Article: struct's are not light-weight classes.
Pending Article: What to think about when comparing strings.
```