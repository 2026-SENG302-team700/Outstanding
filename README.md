# SENG302  Project Overview

Welcome to the  project for SENG302 2026. In this README we have included some useful information to help you get started. We advise you take some time reading through this entire document, as doing so may save you many headaches down the line!

## Dependencies

This project requires `dotnet` 10.0 or later, [click here to download the lastest release from Microsoft](https://dotnet.microsoft.com/en-us/download) as well as, `node` v24 (`npm` v11) or later [click here to download the latest node version](https://nodejs.org/en/download). You can also use your package manager (e.g., `apt`, `brew`, `chocolatey`).

## Technologies

This project makes use of several technologies that you will have to work with. Specifically, you will be creating a web application using C# ASP.Net for the _"back-end"_ (as a REST API) and Sveltekit for the _"front-end"_ (as a single-page-app).

For many of you, this will be the first time using C# and or Sveltekit, so we have included some helpful links below to get you started.

### Front-end

- [Sveltekit](https://kit.svelte.dev/docs) - Used for the front-end framework
- [Using Sveltekit as an SPA](https://khromov.se/the-missing-guide-to-understanding-adapter-static-in-sveltekit/) - An interesting article about using Sveltekit as a single page application
- [Bootstrap 5.3](https://getbootstrap.com/docs/5.3/getting-started/introduction) - Used within the Sveltekit front-end for styling
- [Bootstrap Icons 1.13.1](https://getbootstrap.com/docs/5.3/getting-started/introduction/) - Used within the Sveltekit front-end for icons

### Back-end

- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [C# vs Java](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/tips-for-java-developers)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/overview?view=aspnetcore-10.0) - Used for the back-end web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - Used for database access
- [SQLite Documentation](https://sqlite.org/docs.html) - Used for development databases
- [PostgreSQL 18 Documentation](https://www.postgresql.org/docs/18/index.html) - Used for production databases (from Sprint 3 onwards)

For those of you who prefer more traditional tutorial style guides, we have included some links below:

- [Learn C#](https://learn.microsoft.com/en-us/collections/yz26f8y64n7k07) - Microsoft's official C# learning resources
- [Learn C# Video Series](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oULFjxrOagaERVAMbmG20Xe) - A series of short youtube vidoes covering a number of C# development topics

## Integrated Development Environment (IDE)

Within SENG301 you are recommended to use Visual Studio Code as your IDE, Lab 1 includes more information about how to set up VS Code for C# projects. On top of the recommended plugins from SENG301, you should install the official [Sveltekit plugin](https://marketplace.visualstudio.com/items?itemName=svelte.svelte-vscode).

For those of you who have experience with IntelliJ IDEA, [Rider](https://www.jetbrains.com/rider/) by the same company is also a popular IDE for C#. You should install the [Svelte plugin](https://plugins.jetbrains.com/plugin/12375-svelte). Rider will also propose you to install other plugins for you, if needed.

## Quickstart Guide

### Building and running the project

#### 1 - Installing the dependencies

To install the C# dependencies, run the following command from the `SENG302.Api` project directory

```bash
dotnet restore
```

To install the Svelte dependencies, run the following command from the `SENG302.App` project directory

```bash
npm install
```

#### 2 - Running the project

To run the API, run the following command from the `SENG302.Api` project directory

```bash
dotnet run
```

The REST API should now be available on port 5000.

To run the app (UI), run the following commands from the `SENG302.App` project directory

```bash
npm run dev
```

The app should now be available on port 5173 and visible in your browser at <http://localhost:5173>.

#### 3 - What's included to play with

The  project comes with a small example book management system where the user can:

- view a list of books
- add new books
- view individual books
- delete books

We can also see the API specifications that our API provides by going to <http://localhost:5000/swagger/index.html>. From here we can even run example requests to see the output using the 'Try it out!' button. This _Swagger_ API page is only enabled in `Development` mode, and should **never be turned on publicly available servers**.

We have provided Bootstrap 5 for styling the front-end. You can see this styling in the `class` property of the HTML elements. Bootstrap uses pre-defined classes to style elements such as `card`, `btn`, `mb-2`, etc. The [Bootstrap docs](https://getbootstrap.com/docs/5.0/getting-started/introduction/) are an excellent place to see all the different classes available with examples.

### Running and implementing tests

By convention, C# applications place their tests in a separate project, in this case `SENG302.Api.Tests`. This is similar to how Java separates between `main` and `test` packages.

From the `SENG302.Api.Tests` project, run the command

```bash
dotnet test
```

To see coverage information in the terminal, run the command

```bash
dotnet test /p:CollectCoverage=true
```

**Note**: the `/p` argument is used to pass parameters to `dotnet`.

As we will discuss in SENG301, you may want to run only part of your tests for performance, or smoke testing reasons. There are several ways to run subsets of tests in C#, however a common approach is splitting by types of tests (i.e. unit and integration).

Within the project these types of tests are split into their own folders by convention, and we can specify which folder to run with our `dotnet test` command.

For example,

```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

will run all unit tests.

Later in the course, we will show you how to define `tags` to run staged tests. We will also teach you how to write different levels of tests, including integration tests. You can already take a look into the `Tests` project if you are curious.
