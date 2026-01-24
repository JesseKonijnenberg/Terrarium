# Project Intelligence & Style Guide: Terrarium Solution

## Tech Stack
- **Framework:** .NET 10 (C# 14)
- **UI Framework:** Avalonia (MVVM via CommunityToolkit.Mvvm)
- **Database:** EF Core 10 (Sqlite) with `IDbContextFactory` for desktop thread safety.
- **Testing:** xUnit with Moq (Standard Assertions).

## Module Architecture (Vertical Slices)
- **Feature Encapsulation (Path A):** Major features (e.g., KanbanBoard, GardenEconomy) must be their own self-contained **Module Projects**.
- **Module Ownership:** A module project owns its own Logic, ViewModels, and Views.
- **The Shell:** `Terrarium.Avalonia` is strictly a **Host**. It handles startup, global theming, and module discovery; it contains ZERO feature logic.
- **Loosely Coupled Communication:** Modules must NEVER reference each other's internal logic. Cross-module talk must happen via the `ITerrariumEventBus` (Core) or `WeakReferenceMessenger` (UI).
- **Shared Contracts:** Interfaces, domain events, and shared DTOs live in `Terrarium.Core`.
- **Shared Utilities:** Reusable, non-domain logic (e.g., formatting, math helpers) belongs in a `Terrarium.Shared` project to avoid code duplication.

## Connectivity & Synchronization
- **Offline-First:** The application must remain fully functional without an internet connection. All data is persisted locally via `Terrarium.Data` first.
- **Realtime Collaboration:** Use a "Multiplayer" mindset. Logic must account for remote changes arriving via the Event Bus or a Synchronization Service.
- **Conflict Resolution:** When generating logic for data updates, prioritize optimistic concurrency. Use the `Version` token in `EntityBase` to detect and handle sync conflicts.
- **State Management:** UI components must react to state changes pushed from the Logic layer, ensuring the view stays synchronized with the local DB and remote peer updates.

## Realtime & Sync Strategy (Multiplayer)
- **State Sovereignty:** The local SQLite DB is the single source of truth. UI always reflects the DB, and the DB is updated via Sync Workers.
- **Conflict Strategy:** Default to **Optimistic Concurrency** via the `Version` token. If a conflict is unresolvable, the UI must prompt the user or default to a "Last Write Wins" with a notification.
- **Eventual Consistency:** Modules must handle "Eventual Consistency"—assume that data from other modules (like the Garden) may arrive a few seconds late due to sync latency.
- **Presence:** When building Views, include slots for "Presence Indicators" (e.g., who is currently viewing/editing this item).

## Hosting & Multi-Host Strategy
- **Dual Hosts:** The solution supports two entry points:
  1. `Terrarium.Avalonia` (Desktop Host)
  2. `Terrarium.Server` (ASP.NET Core / SignalR Host)
- **Zero-Logic Hosts:** Both hosts must be "thin." They only register the modules and provide the transport layer (UI for Avalonia, API/WebSockets for Server).
- **Sync Protocol:** Use **SignalR** or **WebSockets** for realtime "multiplayer" updates.
- **Shared Modules:** Feature logic in `Modules.Kanban` must be "Host-Agnostic," meaning it can run on the Server or the Desktop without knowing which is which.

## Dynamic UI & Components
- **Component Architecture:** Prioritize reusable **Controls** in the `Controls/` folder over inline XAML.
- **Theming:** Use `{DynamicResource}` for all brushes and layout constants (e.g., `BgMain`, `AccentSage`, `MainCornerRadius`) defined in `ThemeManager`.
- **Performance:** Enforce `x:DataType` and `x:CompileBindings="True"` in all Views and Controls.
- **UI Responsibility:** Avalonia is strictly for **Presentation**. ZERO business logic or DB access.

## Architectural Patterns
- **Separation of Concerns:** - **Logic:** Business rules and orchestration (The "Brain").
  - **Data:** Persistence, Repositories, and EF Core (The "Hands").
- **MVVM:** Heavily utilize `[ObservableProperty]` and `[RelayCommand]` source generators. Use C# 14 partial properties where appropriate.
- **Thread Safety:** All DB operations MUST use the Context Factory via Repositories to handle multi-threaded desktop access.
- **Persistence:** Entities use `EntityBase` for Soft Deletes (`IsDeleted`) and Concurrency (`Version`).

## Documentation & C# 14 Style
- **Documentation:** XML summaries in `Core` interfaces; `<inheritdoc />` in implementations.
- **The 'field' Keyword:** Use C# 14 `field` keyword for property backing logic to eliminate manual private fields.
- **Extension Members:** Use C# 14 `extension` blocks to add feature-specific data or factory methods to Core models without modifying the Core project.
- **Naming:** Underscore prefix for private fields (`_service`); `Async` suffix for Tasks; `On` prefix for event handlers.

## Testing Standards (xUnit + Moq)
- **Clean Tests:** No `// Arrange/Act/Assert` comments. Use whitespace for separation.
- **Assertions:** Use standard xUnit `Assert` syntax only.
- **Structure:** `Terrarium.Tests` must mirror the solution's mirrored folder structure.

## AI Interaction Instructions
- **Stay in the Slice:** When adding a feature, keep all related code (Logic, VM, View) within that specific Module's project/namespace.
- **Logic Placement:** Orchestration in Logic; Persistence in Data; Presentation in Avalonia.
- **Modern Syntax:** Default to C# 14 `field`, `extension` blocks, and partial properties.
