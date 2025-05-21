User Preference is an extension that enables plugin developers to provide a way for their users to customize their interface.

Plugins can use this extension by placing a button in their application that calls the API to show the user interface options modal.

Example:

```csharp
UserPreferenceUI.Show(this, player, onConfirm: () => Puts("Player clicked confirm!");
```

Actual theme usage is very simple:

```csharp
var userPreference = UserPreferenceData.Load(this, player);
var theme = userPreference.Theme;

cui.v2.CreatePanel(
    container: parent,
    position: LuiPosition.MiddleCenter,
    offset: new(-450, -275, 450, 275),
    color: theme.SurfaceContainer
);

cui.v2.CreateText(
    container: parent,
    position: LuiPosition.Top,
    offset: LuiOffset.None,
    color: theme.OnBackground,
    fontSize: 16,
    text: "Welcome!",
    alignment: TextAnchor.UpperCenter
);
```

Themes (color palettes) are based on Material Design 3 (Material You) - https://material-foundation.github.io/material-theme-builder/

The user simply selects a color, a display mode (light/dark), and a contrast level, and a palette will be automatically generated off their selections.

The available colors for use in theme are as follows:

Primary Colors
- Primary
- OnPrimary
- PrimaryContainer
- OnPrimaryContainer
- InversePrimary

Secondary Colors
- Secondary
- OnSecondary
- SecondaryContainer
- OnSecondaryContainer

Tertiary Colors
- Tertiary
- OnTertiary
- TertiaryContainer
- OnTertiaryContainer

Error Colors
- Error
- OnError
- ErrorContainer
- OnErrorContainer

Surface & Background
- Surface
- SurfaceBright
- SurfaceContainerLowest
- SurfaceContainerLow
- SurfaceContainer
- SurfaceContainerHigh
- SurfaceContainerHighest
- SurfaceVariant
- OnSurface
- OnSurfaceVariant
- Background
- OnBackground
- InverseSurface
- InverseOnSurface
- 
Outline & Effects
- Outline
- OutlineVariant
- SurfaceTint
- Shadow
- Scrim
- Transparent (static 0 opacity black)
