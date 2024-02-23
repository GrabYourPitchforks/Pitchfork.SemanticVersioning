# Pitchfork.SemanticVersioning

Helpers for parsing SemVer strings.

This library implements version 2.0.0 of the SemVer specification. See https://semver.org/ for more information. Strings are parsed strictly according to the specification and must have `<major>.<minor>.<patch>[-<prerelease>][+<build>]` format.

Sample usage:

```cs
using Pitchfork.SemanticVersioning;

SemanticVersion a = new SemanticVersion("1.2.3-preview.1");
SemanticVersion b = new SemanticVersion("1.2.3");
Console.WriteLine(a < b); // prints "True"
```
