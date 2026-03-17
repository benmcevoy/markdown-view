# PRD: Router

## Introduction

Implement a routing system that maps HTTP request paths to markdown files located in the configured root directory (e.g., `/sample`). The router will analyze the request path, construct the file path, and return the appropriate markdown content or error response.

## Goals

- Map request paths (e.g., `/folder/page.md`) to files in the root directory
- Support both file and directory requests
- Return appropriate HTTP status codes (200 for success, 404 for not found)
- Handle URL normalization (remove trailing slashes)
- Support optional file selection within directories (e.g., `/folder/` serves `index.md`)

## User Stories

### US-001: Basic path-to-file mapping
**Description:** As a user, I want to request a markdown file by its path so that I can view its content.

**Acceptance Criteria:**
- [x] Request `/sample/page1.md` returns content of `sample/page1.md`
- [x] Request `/sample/topic/topic1.md` returns content of `sample/topic/topic1.md`
- [x] Request `/sample/index.md` returns content of `sample/index.md`
- [x] Return 200 OK with markdown content for valid paths
- [x] Typecheck passes

### US-002: Handle missing files
**Description:** As a user, I want to receive an error when requesting a non-existent file.

**Acceptance Criteria:**
- [x] Request `/sample/nonexistent.md` returns 404 Not Found
- [x] Request `/sample/folder/` where folder doesn't exist returns 404
- [x] 404 response includes error message
- [x] Typecheck passes

### US-003: Directory default file
**Description:** As a user, I want to access a directory and have its default file (e.g., `index.md`) served automatically.

**Acceptance Criteria:**
- [x] Request `/sample/topic/` returns content of `sample/topic/index.md` if it exists
- [x] If no `index.md` exists in the directory, return 404
- [x] Typecheck passes

### US-004: URL normalization
**Description:** As a user, I want to request URLs with or without trailing slashes and get consistent results.

**Acceptance Criteria:**
- [x] Request `/sample/page1.md/` is normalized to `/sample/page1.md`
- [x] Request `/sample/topic/` is normalized to `/sample/topic/`
- [x] Normalized paths produce consistent responses
- [x] Typecheck passes

## Functional Requirements

- FR-1: Parse the request path to extract the markdown file path relative to the root directory
- FR-2: Remove query strings from the request path before processing
- FR-3: Normalize paths by removing trailing slashes (except for root `/`)
- FR-4: Construct the full file path by combining root directory with relative path
- FR-5: Check if the file exists in the file system
- FR-6: If file exists, read and return its content with 200 status
- FR-7: If file doesn't exist, return 404 with error message
- FR-8: For directory paths, check for `index.md` as default file
- FR-9: Handle URL-encoded paths (decode before processing)
- FR-10: Validate that the resolved path is within the root directory (prevent directory traversal)

## Non-Goals

- No URL rewriting or rewrite rules
- No query parameter routing
- No dynamic route parameters (e.g., `/blog/:slug`)
- No static asset routing (CSS, JS, images)
- No authentication/authorization checks
- No middleware pipeline for the router

## Design Considerations

- Router should be a separate component from content generation
- Consider caching file existence checks to improve performance
- Path matching should be case-sensitive (Linux convention) or configurable

## Technical Considerations

- **Root Directory:** Configurable via command-line argument (default: `/home/agent/hello-world/sample`)
- **Path Resolution:**
  1. Extract path from request
  2. Remove query string and trailing slash
  3. Decode URL-encoded characters
  4. Construct file path: `{root}/{path}`
  5. Check if path ends with `.md`
  6. If path is a directory (no `.md` extension), append `/index.md`
  7. Verify file exists
  8. Return file content or 404
- **Security:**
  - Validate resolved path is within root directory
  - Reject paths containing `..` sequences
  - Reject paths starting with `/../`

## Success Metrics

- All valid markdown files are accessible via their path
- 404 responses for invalid paths are returned promptly
- URL normalization works consistently
- No directory traversal vulnerabilities
- Response time under 50ms for file lookups

## Open Questions

- Should we support case-insensitive file matching on case-insensitive file systems?
- Should we add a configuration option for default file name (currently hardcoded to `index.md`)?
- Should we implement caching for frequently accessed files?
