![GitHub](https://img.shields.io/github/license/WycliffeAssociates/USFMToolsSharp.Renderers.HTML?color=blue)
![Travis (.com) branch](https://img.shields.io/travis/com/WycliffeAssociates/USFMToolsSharp.Renderers.HTML/master)
![Nuget](https://img.shields.io/nuget/v/USFMToolsSharp.Renderers.HTML?color=blue)
![Nuget](https://img.shields.io/nuget/dt/USFMToolsSharp.Renderers.HTML?color=blue)


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

## Basic Usage

The library provides two main classes for rendering USFM to HTML:

### HtmlRenderer

This class transforms a USFMDocument into an HTML string.

**Basic Example:**
```csharp
using USFMToolsSharp;
using USFMToolsSharp.Renderers.HTML;

// Create a parser and renderer
var parser = new USFMParser();
var renderer = new HtmlRenderer();

// Parse USFM content
var contents = File.ReadAllText("01-GEN.usfm");
USFMDocument document = parser.ParseFromString(contents);

// Render to HTML
string html = renderer.Render(document);
File.WriteAllText("output.html", html);
```

## Configuration

### HTMLConfig

The `HTMLConfig` class allows you to customize the HTML output. You can pass it to the `HtmlRenderer` constructor to configure rendering behavior.

**Configuration Options:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `divClasses` | `List<string>` | Empty list | CSS class names to wrap the content in div elements |
| `separateChapters` | `bool` | `false` | Insert page breaks between chapters |
| `separateVerses` | `bool` | `false` | Insert line breaks between verses |
| `blankColumn` | `bool` | `false` | Add a blank column for notes (creates a two-column table) |
| `partialHTML` | `bool` | `false` | Generate HTML fragment without `<html>`, `<head>`, and `<body>` tags |
| `renderTableOfContents` | `bool` | `false` | Generate a table of contents based on TOC2 markers |
| `ChapterIdPattern` | `string` | `null` | Format string for chapter div IDs (e.g., "chapter-{0}") |
| `VerseIdPattern` | `string` | `null` | Format string for verse span IDs |

**Configuration Example:**
```csharp
using USFMToolsSharp;
using USFMToolsSharp.Renderers.HTML;

// Create custom configuration
var config = new HTMLConfig
{
    divClasses = new List<string> { "scripture", "bible-text" },
    separateChapters = true,
    partialHTML = false,
    renderTableOfContents = true,
    ChapterIdPattern = "chapter-{0}"
};

// Use configuration with renderer
var parser = new USFMParser();
var renderer = new HtmlRenderer(config);

var contents = File.ReadAllText("01-GEN.usfm");
USFMDocument document = parser.ParseFromString(contents);
string html = renderer.Render(document);
File.WriteAllText("output.html", html);
```

### HtmlRenderer Properties

The `HtmlRenderer` class also provides additional properties for customization:

| Property | Type | Description |
|----------|------|-------------|
| `FrontMatterHTML` | `string` | HTML content to insert at the beginning of the body |
| `InsertedFooter` | `string` | HTML content to insert before closing body tag |
| `InsertedHead` | `string` | Custom HTML head content (replaces default head) |
| `UnrenderableTags` | `List<string>` | List of USFM markers that couldn't be rendered (populated after rendering) |

**Custom Head and Footer Example:**
```csharp
var renderer = new HtmlRenderer();
renderer.InsertedHead = @"
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bible Text</title>
    <link rel='stylesheet' href='custom-style.css'>
</head>";

renderer.FrontMatterHTML = "<div class='preface'>Custom introduction text</div>";
renderer.InsertedFooter = "<footer>© 2025 Bible Translation</footer>";

string html = renderer.Render(document);
```

## Advanced Usage Examples

### Generating Partial HTML

For embedding in existing web pages, use `partialHTML: true`:

```csharp
var config = new HTMLConfig { partialHTML = true };
var renderer = new HtmlRenderer(config);
string htmlFragment = renderer.Render(document);
// Returns only the content without <html>, <head>, or <body> tags
```

### Adding Page Breaks Between Chapters

Useful for print or PDF generation:

```csharp
var config = new HTMLConfig 
{ 
    separateChapters = true,
    separateVerses = false
};
var renderer = new HtmlRenderer(config);
string html = renderer.Render(document);
```

### Creating a Two-Column Layout

For study Bibles with a notes column:

```csharp
var config = new HTMLConfig 
{ 
    blankColumn = true 
};
var renderer = new HtmlRenderer(config);
string html = renderer.Render(document);
```

### Generating Table of Contents

Automatically create a table of contents from TOC2 markers:

```csharp
var config = new HTMLConfig 
{ 
    renderTableOfContents = true 
};
var renderer = new HtmlRenderer(config);
string html = renderer.Render(document);
```

## CSS Styling

The renderer uses CSS classes for styling. A default `style.css` file is referenced in the generated HTML. You can customize the appearance by creating your own stylesheet or using the included one.

**Common CSS Classes:**

- `.chapter` - Chapter container
- `.chaptermarker` - Chapter number
- `.verse` - Verse container  
- `.versemarker` - Verse number
- `.sectionhead-{n}` - Section headings (n = 1-4)
- `.poetry-{n}` - Poetry/quote indentation levels
- `.footnotes` - Footnote content
- `.cross-ref` - Cross reference content
- `.majortitle-{n}` - Major title (book name, etc.)
- `.intro-title-{n}` - Introduction titles
- `.table-block` - Tables

For a complete list of CSS classes, refer to the included `style.css` file.
