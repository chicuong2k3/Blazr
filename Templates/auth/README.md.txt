# Blazr Auth Starter

Register the services in `Program.cs`:

```csharp
builder.Services.AddBlazrDemoAuthentication();
```

Wrap your app with `CascadingAuthenticationState` and use `AuthorizeView` where needed.
Replace the demo provider with your real identity source when you move past the prototype stage.
