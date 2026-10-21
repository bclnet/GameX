ADR 0014: C# projects do not multi-target
===

* **Status:** Accepted
* **Date:** 2026-10-21

## Context

A C# project in this repo sets one `TargetFramework`. Family projects and `GameX.All` are `netstandard2.1`. Engine hosts such as `GameX.Engine.Mg` are `net10.0`.

Setting `TargetFrameworks` on one project builds more than one assembly under the same name. That was considered for `GameX.Origin` so MonoGame types could sit beside the `netstandard2.1` formats. It is rejected. `GameX.App.Client` would load two `GameX.Origin` assemblies: the netstandard one pulled by `GameX.Origin.UO`, and a `net10.0` one pulled by an engine. Static state such as `UltimaOnline.Current` would split. `MonoGame.Framework.DesktopGL` also does not target `netstandard2.1`.

## Decision

Do not set `TargetFrameworks`. A project has one `TargetFramework`. Code that needs a different runtime is a separate project.

The Ultima Online MonoGame painter is `GameX.Origin.Mg` (`net10.0`), a sibling of `GameX.Origin`, not a second target on `GameX.Origin`. `GameX.Origin` stays `netstandard2.1`.

The accepted exception is `dotnet/Apps/GameX.App.ExplorerM/GameX.App.ExplorerM.csproj`. It already sets `TargetFrameworks` for MAUI (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, and on Windows `net10.0-windows10.0.19041.0`). MAUI needs one target per OS. Leave it alone. Do not add another target there, and do not copy that pattern onto a library or a family project.

## Consequences

* Engine-specific UO code is a new single-target project per engine.
* Nobody multi-targets `GameX.Origin`, `GameX.Origin.UO`, or `GameX.Origin.Netcode`.
