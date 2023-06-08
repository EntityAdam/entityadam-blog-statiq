---
Title: "Server Side Blazor Code Re-Use using MVVM"
Published: 2019-04-20 12:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: Blazor
---

While MVVM is not 'officially' supported in Blazor, MVVM after all is just a design pattern and I'm going to demonstrate that you can actually use the MVVM pattern with Blazor and potentially share or re-use some of the code with WPF or Xamarin Forms.

# Server Side Blazor Code Re-Use using MVVM

Author: Adam Vincent

GitHub Repository: [HappyStorage](https://github.com/EntityAdam/HappyStorage)

## What is MVVM?
In a nutshell, MVVM is a design pattern derived from the Model-View-Presenter (MVP) pattern. The Model-View-Controller (MVC) pattern is also derived from MVP, but where MVC is suited to sit on top of a stateless HTTP protocol, MVVM is suited for user interface (UI) platforms with state and two way data binding.  MVVM is commonly implemented in Desktop (WPF / UWP), Web (Silverlight), and Mobile (Xamarin.Forms) applications.  Like the other frameworks, Blazor acts much like a Single Page Application (SPA) that has two way data binding, and can benefit from the MVVM pattern. So whether you have existing MVVM code in the form of a WPF or mobile application, or are starting green with new code you can leverage MVVM to re-use your existing code in Blazor, or share your code with other platforms respectively.

![](MVVMPattern.png)
> More information on MVVM: [Wikipedia](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel)

----
## Example Presentation Layer
### BindableBase
At the heart of MVVM is the `INotifyPropertyChanged` interface which notifies clients that a property has changed. It is through this interface that converts a user interaction into your code being called. Usually, all ViewModels, and some Models will implement `INotifyPropertyChanged` therefore, it is common to either use a library (Prism, MVVM Light, Caliburn) or create your own base class.  Here is a minimal implementation of INPC.

```cs
public abstract class BindableBase : INotifyPropertyChanged
{
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

In this simplified model class which derives from `BindableBase`, we have a `CustomerModel` with a single property `FirstName`.  In this context, we would probably have a Customer filling out an `<input>` within a `<form>` on a website where they must fill in their first name. This input would be bound to an instance of `CustomerModel`  on the ViewModel. While the customer is filling out the form since we are in a two way data binding scenario, each time the customer enters or removes a character from the forms input box, `SetField()` is called and will cause the `PropertyChanged` event to fire.

```cs
public class NewCustomerModel : BindableBase
{
    private string firstName;
    
    public string FirstName
    {
        get => firstName;
        set
        {
            SetField(ref firstName, value);
        }
    }
}
```

> **More:** If you need to know more about `INotifyPropertyChanged` the [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=netframework-4.8) cover this topic very well.


### Model
With `INotifyPropertyChanged` out of the way, here is the entire presentation model.


```cs
public class NewCustomerModel : BindableBase
{
    [Display(Name = "Customer Number")]
    public string CustomerNumber { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    private string firstName;
    [Required]
    [Display(Name = "First Name")]
    public string FirstName
    {
        get => firstName;
        set
        {
            SetField(ref firstName, value);
            OnPropertyChanged(nameof(FullName));
        }
    }

    private string lastName;
    [Required]
    [Display(Name = "Last Name")]
    public string LastName
    {
        get => lastName;
        set
        {
            SetField(ref lastName, value);
            OnPropertyChanged(nameof(FullName));
        }
    }

    [Display(Name = "Address")]
    public string Address => $"{Street}, {City}, {State} {PostalCode}";

    private string street;

    [Required]
    [Display(Name = "Street Address")]
    public string Street
    {
        get => street;
        set
        {
            SetField(ref street, value);
            OnPropertyChanged(nameof(Address));
        }
    }
    private string city;

    [Required]
    [Display(Name = "City")]
    public string City
    {
        get => city;
        set
        {
            SetField(ref city, value);
            OnPropertyChanged(nameof(Address));
        }
    }
    private string state;

    [Required]
    [Display(Name = "State")]
    public string State
    {
        get => state;
        set
        {
            SetField(ref state, value);
            OnPropertyChanged(nameof(Address));
        }
    }
    private string postalCode;

    [Required]
    [Display(Name = "Zip Code")]
    public string PostalCode
    {
        get => postalCode;
        set
        {
            SetField(ref postalCode, value);
            OnPropertyChanged(nameof(Address));
        }
    }
}
```

There's a few things to point out in this presentation model. First, please note the use of the **Data Annotation** attribute such as `[Required]`.  You can decorate your properties to provide rich form validation feedback to your users. When the customer is filling out a form and misses a required field it will not pass the model validation and prevent the form from being submitted as well as provide an error message if configured. We will cover this more in the **View** section

The next thing I wanted to point out is I've covered `SetField()` in the INotifyPropertyChanged section, but there is an additional bit of complexity.


```cs
[Display(Name = "Full Name")]
public string FullName => $"{FirstName} {LastName}";
```

Note that the `FullName` property is a `{ get; }` only concatenation of the customers first and last name.  Since we are forcing the customer to fill out first and last name in separate form field, changing either the first or last name causes the `FullName` to change.  We want the ViewModel to be informed of these changes to `FullName`.  

```cs
private string firstName;
[Required]
[Display(Name = "First Name")]
public string FirstName
{
    get => firstName;
    set
    {
        SetField(ref firstName, value);
        OnPropertyChanged(nameof(FullName));
    }
}
```

After the `SetField()` is invoked in the base class, there is an additional call to `OnPropertyChanged()`, which let's the ViewModel know that in addition to `FirstName` changing, `FullName` has also changed. 

### Example ViewModel Interface
The example ViewModel below will expand on the above model, we'll be using a simplified user story of creating a new customer.


Blazor supports .NET Core's dependency injection out of the box, which makes makes injecting a ViewModel very simple.  In the following ViewModel interface, we'll need our concrete class to have an instance of `NewCustomer` as well as a method which knows how to create a new customer.


```cs
public interface ICustomerCreateViewModel
{
    NewCustomerModel NewCustomer { get; set; }
    void Create();
}
```

And the concrete implementation of `ICustomerCreateViewModel`


```cs
public class CustomerCreateViewModel : ICustomerCreateViewModel
{
    private readonly ICustomerService _customerService;

    public CustomerCreateViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public NewCustomerModel NewCustomer { get; set; } = new NewCustomerModel();

    public void Create()
    {
        //map presentation model to the data layer entity
        var customer = new NewCustomer()
        {
            CustomerNumber = Guid.NewGuid().ToString().Split('-')[0],
            FullName = $"{newCustomer.FirstName} {NewCustomer.LastName}",
            Address = $"{newCustomer.Address}, {NewCustomer.City}, {newCustomer.State} {NewCustomer.PostalCode}"
        };

        //create
        _customerService.AddNewCustomer(customer);
    }
}
```

### ViewModel Deep-Dive
In the constructor we're getting an instance of our `ICustomerService` which knows how to create new customers when provided the data layer entity called `NewCustomer`

I need to point out that `NewCustomer` and `NewCustomerModel` serve two different purposes.  `NewCustomer` is the data entity, and is a simple POCO. The data entity is the item that is persisted. Of note, in this example, we save the customers full name as a single column in a database, but on the form backed by the presentation model, we actually want the customer to fill out 'First Name' and 'Last Name'

In the ViewModel, the `Create()` method shows how a `NewCustomerModel` is mapped to a `NewCustomer`.  There are some tools that are very good at doing this type of mapping (like AutoMapper), but for this example the amount of code to map between the types is trivial. For reference, here is the data entity.

```cs 
public class NewCustomer
{
	public string CustomerNumber { get; set; }
	public string FullName { get; set; }
	public string Address { get; set; }
}
```

> **Opinionated Note:** Presentation models and data entities should be separated into their respective layers.  It is possible to create a single `CustomerModel` and use it for both presentation and data layers to reduce code duplication, but I highly discourage this practice.  

### View
The last and final piece to the MVVM pattern is the View.  The View in the context of Blazor is either a `Page` or `Component`, which is either a .razor file, or a .cshtml file and contains Razor code. Razor code is a mix of C# and HTML markup.  In the context of this article, our view will be a customer form that can be filled out, and a button that calls the ViewModel's `Create()` method when the form has been filled out properly according to the validation rules.

```cs
@page "/customer/create"
@using HappyStorage.Common.Ui.Customers
@using HappyStorage.BlazorWeb.Components
@inject Microsoft.AspNetCore.Components.IUriHelper UriHelper
@inject HappyStorage.Common.Ui.Customers.ICustomerCreateViewModel viewModel

<h1>Create Customer</h1>

<EditForm Model="@viewModel.NewCustomer" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <div class="form-group">
        <h3>Name</h3>
        <LabelComponent labelFor="@(() => viewModel.NewCustomer.FirstName)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.FirstName" />

        <LabelComponent labelFor="(() => viewModel.NewCustomer.LastName)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.LastName" />
    </div>
    <div class="form-group">
        <h3>Address</h3>

        <LabelComponent labelFor="@(() => viewModel.NewCustomer.Street)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.Street" />

        <LabelComponent labelFor="@(() => viewModel.NewCustomer.City)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.City" />

        <LabelComponent labelFor="@(() => viewModel.NewCustomer.State)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.State" />

        <LabelComponent labelFor="@(() => viewModel.NewCustomer.PostalCode)" />
        <InputText class="form-control" bind-Value="@viewModel.NewCustomer.PostalCode" />
    </div>
    <br />
    <button class="btn btn-primary" type="submit">Submit</button>
    <button class="btn" type="button" onclick="@ReturnToList">Cancel</button>
</EditForm>
```

The first thing to note is at the top of the code, and this is how we use dependency injection to get an instance of our ViewModel.

```cs
@inject HappyStorage.Common.Ui.Customers.ICustomerCreateViewModel viewModel
```

Easy!  Next we need to create the form.  The EditForm needs an instance of a model to bind to, which is provided by the ViewModel already, and a method to call when the user submits a valid form.

```cs
<EditForm Model="@viewModel.NewCustomer" OnValidSubmit="@HandleValidSubmit">
...
</EditForm>
```

Next we bind each property to their respective `<input>`'s, Blazor has some built in `<Input***></Input***>` helpers which help you accomplish the binding.  They are still under development and you may find some features are lacking at the time of writing. Please refer to the docs in the note below for more up to date info. 

> **Note** the `<LabelComponent />` is something I've created as a replacement for the `asp-for` tag-helper that retrieves the `DisplayAttribute` from the presentation model classes. The code is available in the GitHub repository listed at the top.

```cs
<LabelComponent labelFor="@(() => viewModel.NewCustomer.FirstName)" />
<InputText class="form-control" bind-Value="@viewModel.NewCustomer.FirstName" />

<LabelComponent labelFor="(() => viewModel.NewCustomer.LastName)" />
<InputText class="form-control" bind-Value="@viewModel.NewCustomer.LastName" />
```

The magic here is `bind-Value` which binds our `<InputText />` text box to the value of the ViewModels instance of the `NewCustomerModel` presentation model.

> **Note:** Full documentation on [Blazor Forms and Validation](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-3.0)

Last but not least we'll need some code to call our ViewModel's `Create()` method when the form is submitted and valid, as well as the `onclick=ReturnToList` I've defined for the **Cancel** button.

```cs
@functions {
    private void HandleValidSubmit()
    {
        viewModel.Create();
        ReturnToList();
    }

    private void ReturnToList()
    {
        UriHelper.NavigateTo("/customers");
    }
}
```

### Conclusion 

That's it! In summary, I've covered what MVVM is, how Blazor can benefit from it as well as an in depth look at a simple example of how we can create a form with validation and rich feed back to the user.  It is also important to reiterate that this example works not only in Blazor, but can be re-used in Windows Presentation Foundation (WPF) desktop applications as well as other platforms.  Please check out the [GitHub repository](https://github.com/EntityAdam/HappyStorage) as I continue to develop and expand on this concept.
