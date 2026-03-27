
## admin

- set port?
- store in a config file specific to MD folder , e.g.  .mdview
- view log


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


