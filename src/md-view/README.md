# TODO

- process start browser on startup
- use async
- in memory cache for navigation
- config and .mdview settings (see admin)

## navigation

- generate nav structure and cache
- navigation is highlighting issues with rendering and routing
- i see feature envy


## front matter and metadata

- what is front matter? a YAMLy set of keyvalues

```
---  or +++ or ;
title: my title   -or- title = My title  -or- 
tags: a,b,c
---
```
make configurable to show or hide
leverage for search

markdig has YAML support

- need titles for navigation


file name


## raw view

## admin

- set port?
- theme
- store in a config file specific to MD folder , e.g.  .mdview
- view log

## more handlers

- images
- pdf
- json and xml pretty
- js, code, .env, .config pretty
- allowed extensions etc in .mdview


## skillz

dotnet format
- nice that it also respects .editorconfig, as does vscode
- 

review for state and side effects
- look for static
- look for non readonly module level
- aim for small s singleton

review for pattern usage
- per file or feature
- suggest architecture
- reference to GoF, SOLID, etc.

review for SRP
- how long is this class
- how long each method
- review for spaghetti
- too much if/else
- pyramid of doom
- flagitis within methods

review for DRY
- just call it out for the human

review for pokemon exception handling

review for strength
- guard clause on public method args
- 

review for trust
- validate inputs




review compile errors
- for each suggest fix and apply
- and then review warnings
- treat warnings as errors

## chat

  4. No Scanner Implementation
  - NavigationService.Build() is never called
  - No recursive directory scanning for markdown files
  - No navigation sidebar generation

  5. Template Engine Fragility
  - MainTemplate.Render() uses string Replace() instead of proper templating
  - Vulnerable to injection attacks
  - No variable escaping

  6. Add Middleware for Security

  Current: Security is only in Router.IsValidRequest() - naive string checking.

  Suggested: Add middleware for:
  - Path normalization (handle .. after normalization)
  - Rate limiting
  - Request logging
  - CORS headers

  ---
  7. Add Proper Error Handling

  Current: Generic 404 Not Found for all errors.

  Suggested: Custom error pages and proper exception handling:

  // wwwroot/html/error.html
  @{
      var error = context.Exception;
  }
  <div class="error">
      @error.Message
  </div>

  ---


