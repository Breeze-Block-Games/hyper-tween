# Unity

The Unity directory contains multiple sub-directories each with their own AssemblyDefinition file, designed to encapsulate
code sensibly. For example, certain server code should not be compiled ito the client to avoid hackers getting access to
sensitive code. References between the AssemblyDefinitions are already configured

## Client

Only included in builds when `UNITY_SERVER` is _not_ defined.

## Editor

Only included in builds when `UNITY_EDITOR` is defined.

## Server

Only included in builds when `UNITY_SERVER` is defined.

## Shared

Always included in builds, referenced by all other AssemblyDefinitions.

Each of the above directories also contain a Tests directory with an AssemblyDefinition configured for inclusion of
playmode tests.