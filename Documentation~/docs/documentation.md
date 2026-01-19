# Documentation

Package Base provides a base DocFX documentation project to facilitate ease of writing documentation for forked packages.
This helps to provide a consistent style for all Breeze Block Games documentation as well as a consistent pipeline for
uploading built documentation to [https://docs.breezeblock.games/package/version](https://docs.breezeblock.games).

To test building and viewing this documentation locally you can run the following from the root directory:

`docfx build Documentation~/docfx.json; docfx serve Documentation~/_site`

Note that you first need to install DocFX:

`dotnet tool install --global docfx`.