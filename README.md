# Blazr

`blazr` is a Blazor-focused CLI for installing common project capabilities the way frontend teams use tools like `yarn add`.

Current commands:

```powershell
blazr list
blazr new
blazr new MyApp
blazr new MyApp --render-mode WebAssembly
blazr new MyApp --with-blueprint --with-tailwind --with-auth --with-pwa --with-besql --http-client refit
blazr add tailwind
blazr add blueprint
blazr add auth
blazr add pwa
blazr add besql
blazr add test
blazr add tailwind --project C:\src\MyBlazorApp
blazr add auth --dry-run
```

Current feature set:

- `tailwind`: scaffolds the Blazor Blueprint Tailwind v4 standalone CLI setup.
- `blueprint`: installs Blazor Blueprint packages and wires services, imports, styles, and portal host.
- `auth`: adds a starter authentication state provider and registration notes.
- `pwa`: installs Bit.Bswup and scaffolds service-worker update progress wiring.
- `besql`: installs Bit.Besql and scaffolds browser SQLite starter files.
- `httpclient`: adds a typed `HttpClient` starter.
- `refit`: adds Refit starter files and package wiring.
- `flurl`: adds Flurl starter files and package wiring.
- `test`: creates a sibling xUnit project linked to the target app.

Project creation:

- `blazr new` runs an interactive wizard like `yarn create`
- asks for project name, framework, render mode, showcase pages, HTTP client strategy, then lets you pick extras with a checkbox list
- shows a summary screen and asks `Proceed?` before running
- supports scripted usage with `--framework`, `--render-mode`, `--include-showcase`, `--http-client`, `--with-blueprint`, `--with-tailwind`, `--with-auth`, `--with-pwa`, `--with-besql`, `--with-test`, `--output`, `--skip-template-install`, and `--dry-run`

Project detection:

- detects `Blazor Web App`
- detects `Blazor Server`
- detects `Blazor WebAssembly`
- falls back to `Unknown` for non-Blazor or ambiguous projects

Planned direction:

- `blazr remove <feature>`
- versioned feature packages
- a remote feature catalog
- deeper source-code integration for `Program.cs`, layouts, and routing
