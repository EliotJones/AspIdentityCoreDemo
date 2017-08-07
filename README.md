# Asp Identity Without Entity Framework #

A simple implementation of ASP.NET Core Identity using text file based persistence.

The relevant classes are:

+ Core\User.cs - The class representing the replacement for ```ApplicationUser```
+ Core\Role.cs - A placeholder for user role support.

Then for data access:

+ DataAccess\\*.cs - classes for a hacky data access format which shouldn't be used anywhere outside of this demo.

And finally:

+ Services\UserPasswordStore.cs - The main logic for the identity replacement
+ Services\RoleStore.cs - A placeholder for user role support.

[See the tutorial](https://eliot-jones.com/2017/6/asp-net-core-identity-post-1 "Blog post on my site")
