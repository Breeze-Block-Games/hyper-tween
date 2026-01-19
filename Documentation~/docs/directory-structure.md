# Directory Structure

The following directory structure is used in order to support packages that can be used for any purpose

* [.github](github.md) - workflows and workflow specific configuration files
* [Documentation~](documentation.md) - placeholder DocFX documentation and configuration
* [Editor](unity.md#editor) - An AssemblyDefinition root for Unity editor code
* [Service~](service.md) - dotnet project for the backend service associated with the package
* [Shared](shared.md) - An AssemblyDefinition root for code shared between Unity and the dotnet service
* [Shared~](shared.md) - the csproj for the shared code
* [Tests~](tests.md) - A csproj for non-Unity dependent tests that is ran before building the service
* [Tools~](tools.md) - a placeholder directory for tooling projects related to the package
* [UnityClient](unity.md#client) - A client specific AssemblyDefinition root such that server specific code is not compiled into the client
* [UnityServer](unity.md#server) - A server specific AssemblyDefinition root such that client specific code is not compiled into the server
* [UnityShared](unity.md#shared) - An AssemblyDefinition root referenced by the client, server and editor AssemblyDefinitions

Directories suffixed with `~` are prevented from appearing within Unity. The intention for editing this package is that
files visible to Unity should be edited by opening the `.sln` associated with the Unity project. For all other files, 
there is a `.sln` in the root of the package with links to relevant projects and folder.
