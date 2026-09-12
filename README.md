# Cartwheel

Cartwheel is a personal practice project for building out a .NET backend from the ground up. It currently exists as a bare solution skeleton — a domain class library with no logic yet and an accompanying test project — used as a sandbox for exploring project structure, testing setup, and .NET idioms before real application features are added.

The stack is C# targeting .NET 10, organized as a solution (`Cartwheel.slnx`) under [backend/](backend). [Cartwheel.Domain](backend/Cartwheel.Domain) is the (currently empty) domain class library, and [Cartwheel.Tests](backend/Cartwheel.Tests) is an xUnit test project (with `coverlet.collector` for coverage) that references it. Both projects have nullable reference types and implicit usings enabled.

To run it, you'll need the [.NET 10 SDK](https://dotnet.microsoft.com/download) installed. From the `backend` directory, restore and build with `dotnet build`, and run the test suite with `dotnet test`.
