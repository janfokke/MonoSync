![banner](Images/banner.svg)

[![Nuget](https://badgen.net/nuget/v/MonoSync)](https://www.nuget.org/packages/MonoSync/)
[![Build Status](https://dev.azure.com/janfokkeurk/MonoSync/_apis/build/status/janfokke.MonoSync?branchName=master)](https://dev.azure.com/janfokkeurk/MonoSync/_build/latest?definitionId=1&branchName=master)
[![Azure DevOps tests](https://img.shields.io/azure-devops/tests/janfokkeurk/MonoSync/1)](https://dev.azure.com/janfokkeurk/MonoSync/_build?definitionId=1&_a=summary&view=runs)
[![Discord](https://img.shields.io/discord/670985266374115370)](https://discord.gg/GNnKY6j)

# What is MonoSync
MonoSync is a synchronization library that you can easily implement with attributes and helper functions.
MonoSync enables you to synchronize an object (e.g. a game world) just like you would with a JSON serializer. MonoSync automatically keeps track of all changes and references. Which you can synchronize periodically.

**What is MonoSync not**

MonoSync is not a Network library. The serialized data is provided as ```byte[]``` 
and the user is free to choose his own Network layer.

# Features

<dl>
  <dt>Delta compression</dt>
  <dd>Only changed values are synchronized.</dd>

  <dt>Object tracking</dt>
  <dd>New objects are automaticly tracked and synchronized.</dd>
  
  <dt>Interpolation</dt>
  <dd>Properties can be configured with a smooth transition between the current value and the newly received value</dd>
</dl>

# Getting started
https://github.com/janfokke/MonoSync/wiki/Getting-started

# Documentation
https://github.com/janfokke/MonoSync/wiki

# Demos
https://github.com/janfokke/MonoSync/tree/master/samples

# Gallery
Interpolation

![Interpolation sample](https://media.giphy.com/media/H1vs2LGitZ7iYeHYph/giphy.gif)
