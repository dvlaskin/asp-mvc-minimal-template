
<h1 align="center">
  <br>
  <br>
    A minimal ASP.NET MVC template with Identity for a quick start
  <br>
</h1>

<h3>
Are you looking for a lightweight and easy-to-customize template for building ASP.NET MVC web applications with authentication using Identity? Look no further than this open-source project!
This template includes the basic setup for user authentication and registration, as well as a simple example page to help you get started.
</h3>
<br>


## Key features

- The application uses SQLite database and EntityFrameworkCore ORM.
- Authentication and authorization are handled by MS AspNetCore Identity.
- Existing controllers include:
  - Home - basic homepage, 
  - Account - registration new account, login, logout, reset password, etc., 
  - AccountManager - change email, password, etc. of logged-in user, 
  - AdminUser - view and change basic information about user accounts, 
  - AdminUserRole - add and remove the UserRole of user accounts,
  - AdminEntity - simple editor for database entities

## Technical Description
This is an ASP.NET web application built on the Microsoft.NET.Sdk.Web SDK. The application targets .NET 6.0 and includes support for nullable reference types and implicit usings.

References:     
- "AutoMapper" Version="12.0.1"
- "Microsoft.AspNetCore.Identity" Version="2.2.0"
- "Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.11"
- "Microsoft.EntityFrameworkCore" Version="6.0.11"
- "Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.11"
- "Microsoft.EntityFrameworkCore.Tools" Version="6.0.11"
- "Newtonsoft.Json" Version="13.0.3"

## Customize as you need

The template is designed to be easy to customize to suit your specific requirements. You can modify the following files or add additional controllers, views, and models to the project as needed to add your own features and functionality:

- The path to the database file is specified in appsettings.json in the ConnectionStrings section.
- The name of the application, which will be displayed on the navigation bar, footer, etc., can be found in the class WebApp.Domain.Models.AppBaseConstants.
- The names of roles can also be found and changed in this class.
- Account password settings, such as password length, can be set in the file Program.cs, in the part of code marked as "Configuration User's data." You can also add attributes to the DTO classes, for example, WebApp.Models.Dto.Account.RegisterDto.
- To send an email with a confirmation link of a newly created account or reset the password, you must implement your own IEmailSender class. This template uses a fake email sender that doesn't send any email, it just imitates email sending.
- The default admin role user: 
  - login: admin@webapp.io
  - password: 123


## Contribute to the project

Contributions to this open-source project are welcome! If you find a bug or want to suggest an improvement, please create a new issue on the repository or submit a pull request with your changes.


## License

This project is licensed under the MIT License, so feel free to use it for your own projects without any restrictions.