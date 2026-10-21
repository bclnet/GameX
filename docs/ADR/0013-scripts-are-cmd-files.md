ADR 0013: Scripts are .cmd files
===

* **Status:** Accepted
* **Date:** 2026-10-21

## Context

The scripted-edit warning lived in CLAUDE.md and named PowerShell. `Set-Content -Encoding utf8` adds a BOM, and a scripted edit must keep the file's line endings. A search of the repo found no `.ps1` files.

## Decision

Scripts that are part of this repo are `.cmd` files. Do not add PowerShell scripts (`.ps1`).

A script that writes a file keeps that file's existing newline style. It must not add a UTF-8 BOM to a file that does not already have one. `.cs` and `.csproj` stay CRLF with a UTF-8 BOM. `.md`, `.rs`, and `.tsv` stay LF without a BOM. `.py` keeps the endings that file already has.

This is about scripts checked into the repo. It does not change the MAUI app, the .NET SDK, or how a person runs `dotnet`.

## Consequences

* The PowerShell sentence in CLAUDE.md is replaced by this rule.
* New automation is a `.cmd` file.
