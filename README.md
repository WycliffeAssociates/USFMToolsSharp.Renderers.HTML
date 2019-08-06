# USFMToolsSharp.Renderers.HTML
A .net HTML rendering tool for USFM.

# Description
USFMToolsSharp.Renderers.HTML is a HTML renderer for USFM. 

# Installation

You can install this package from nuget https://www.nuget.org/packages/USFMToolsSharp.Renderers.HTML/

# Requirements

We targeted .net standard 2.0 so .net core 2.0, .net framework 4.6.1, and mono 5.4 and
higher are the bare minimum.

# Building

With Visual Studio just build the solution. With the .net core tooling use `dotnet build`

# Contributing

Yes please! A couple things would be very helpful

- Testing: Because I can't test every single possible USFM document in existance. If you find something that doesn't look right in the parsing or rendering please submit an issue.
- Adding support for other markers to the parser. There are still plenty of things in the USFM spec that aren't implemented.
- Adding support for other markers to the HTML renderer

# Usage

There a couple useful classes that you'll want to use

## HtmlRenderer

This class transforms a USFMDocument into an html string

Example:
```csharp
var contents = File.ReadAllText("01-GEN.usfm");
USFMDocument document = parser.ParseFromString(contents);
string html = renderer.Render(document);
File.WriteAllText("output.html", html);
```
