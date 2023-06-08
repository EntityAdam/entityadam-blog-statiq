---
Title:  "Restrict the use of certain Attributes"
Published:   2020-01-30 12:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [C#, .NET]
---

This is one of those 'take it or leave it' answers to an interesting question,"How to restrict the use of certain Attributes?"

# An interesting question

A developer asked the following question:

> Anyone know of a mechanism to enforce that certain Annotations are not used?
I’m building my EF core code-first model and would like to enforce that `[DatabaseGenerated]` is not used in the model.

With a little bit of duck duck luck, I found something relevant on StackOverFlow and adapted it to their purpose, and added some access modifiers.

This is what I came up with.

```cs
[Obsolete("Do not use!", true)]
internal sealed class DatabaseGeneratedAttribute : System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute
{
    public DatabaseGeneratedAttribute(DatabaseGeneratedOption databaseGeneratedOption) : base(databaseGeneratedOption)
    {
    }
}
```

So, we're basically deriving our own class from the class we want to restrict other developers from using.

- It's tagged with the `ObsoleteAttribute` so it causes a compiler build error.
- It's `internal` so it should be contained to this single assembly where it's being used.
- It's marked as `sealed` so it can't be further sub-classed.
- It has no implementation, so hopefully it's clear that it's not to be used.

# Thoughts

It was good enough for the developer who asked, but since this code is 'clever', I'm reluctant to recommend it.  However it was an interesting question and it is way easier than writing a Roslyn Analyzer.

It's interesting because If you're part of a team and hopefully have agreed on some design practice, how can you keep you and your team 'honest' and not allow certain things that your team has agreed on?

This is probably a decent use-case, because Entity Framework Core (EF Core) models certainly do have a habit of becoming littered with `DataAnnotations`. And IMO, usually unnecessarily if you have it in your power to stick with the [EF Core conventions](https://www.learnentityframeworkcore.com/conventions).