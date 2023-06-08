---
Title: "Visual Studio Tricks: Increase signal to noise in your debugger"
Published: 2020-04-21 02:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [C#, .NET, Visual Studio]
---

Inspecting your objects in Visual Studio's debugger can sometimes be tedious having to expand objects, arrays and lists trying to find that 'problem child' of yours. Fortunately for us, there are some handy tricks to reducing the noise, and focusing in on the members of your object that are important.

# The demo code

For demonstration purposes, we have a slimmed down `Person` type with `Age`, `Name`, `Nickname` and and a list of addresses in the form of `List<Address>`. The `Address` object has strings for `State`, `City` and `PostalCode`, as well as an `enum` so we can tell if the address is a physical, mailing or work address.

```cs
internal class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
    public string Nickname { get; set; }
    public List<Address> Addresses { get; set; }
}

internal class Address
{
    public AddressType AddressType { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}

enum AddressType
{
    Physical,
    Mailing,
    Work
}
```

# Drilling down into complex objects or lists of object can be painful.

When we have a list with a bunch of people, the list contains a bunch of `{DebuggerDisplayDemo.Person}` which isn't really informative. If you need to discern which `Person` is causing a particular issue or are looking for a specific person you might be clicking a lot of little expand arrows to get to what you're looking for.  This is what it looks like when we inspect the variable

![alt text][debugger-list-of-people1]

# Bring the important stuff to the forefront
An easy way to increase our signal to noise ratio is to leverage the `DebuggerDisplay` attribute.

## The Person Object
```cs
[DebuggerDisplay("Name = {Name}, Age = {Age}")]
internal class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
    public string Nickname { get; set; }
    public List<Address> Addresses { get; set; }
}
```

By adding the `DebuggerDisplay` attribute, we can now pass in an interpolated string which can access properties of the object. Now instead of simply displaying the type name of `{DebuggerDisplayDemo.Person}` we now can see the properties of each person without having to drill down another level.

![alt text][debugger-list-of-people2]

## The Address Object
We can do the same thing to the `Address` type to bubble the important information up. The nice thing is you can format an address appropriate for your culture. Shown here is the U.S. format of `State, City Zip`.  (Yes, we call it a zip code instead of a postal code, but I generally always code it as PostalCode)

```cs
[DebuggerDisplay("{AddressType}: {City}, {State} {PostalCode}")]
internal class Address
{
    public AddressType AddressType { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}
```

# Bring the contents of an array or list forward
So far we've made some good improvements and now when inspecting our objects we get more information at first glance.  We can still take this a step further with the `Addresses` property which is a list of complex `Address` objects. If we drill down into a `Person` object, the debugger by default only shows us the `Count` with the number of items in the list. 

![alt text][debugger-list-of-addresses1]

## DebuggerBrowsableState.RootHidden

We can improve this by using the `DebuggerBrowsable` attribute hiding the root object, in this case the `List` itself, which will bring the lists content to the forefront. Also, not only is the contents of the list shown, it's also showing us the format we specified in the previous step!

```cs
[DebuggerDisplay("Name = {Name}, Age = {Age}")]
internal class Person
{
    public int Age { get; set; }
    public string Name { get; set; }
    public string Nickname { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<Address> Addresses { get; set; }
}
```
## DebuggerBrowsableState.Never
The other thing the `DebuggerBrowsable` attribute can do is to set the BrowsableState to `DebuggerBrowsableState.Never` which will hide it from the debugger to further decrease noise. As an example, the `Nickname` property is completely useless in our debugging experience and we can hide it.

```cs
[DebuggerDisplay("Name = {Name}, Age = {Age}")]
internal class Person
{
    public int Age { get; set; }
    public string Name { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string Nickname { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<Address> Addresses { get; set; }
}
```

Which cleans up our debugger display a little bit to this

![alt text][debugger-person-without-nickname]

Hiding this one property isn't a huge deal in this case, but I'm sure you can imagine how this could clean up the noise for an object with tons of properties.


# Fin.
I've covered a few things in this article, the `DebuggerDisplay` attribute as well as the `DebuggerBrowsable` attribute.  I wanted to highlight the most useful bits of tailoring the debugger display and object inspector to get the most bang for your buck.  I didn't go into a great deal of depth, but if you want to dive a little deeper here is the reference [Microsoft Docs](http://localhost:4000/c%23/2020/04/21/getting-started-debugger-display/) link




[debugger-list-of-people1]: /img/posts/20200421-001-debugger-list-of-people.png "Debugger List of People"
[debugger-list-of-people2]: /img/posts/20200421-002-debugger-list-of-people.png "Debugger List of People"
[debugger-list-of-addresses1]: /img/posts/20200421-001-debugger-list-of-addresses.png "Debugger List of Addresses"
[debugger-list-of-addresses2]: /img/posts/20200421-002-debugger-list-of-addresses.png "Debugger List of Addresses"
[debugger-person-without-nickname]: /img/posts/20200421-001-person-without-nickname.png "Debugger Person without nickname"