# Hearth Remedy

*Home remedies, shared by the families who use them.*

Hearth Remedy is a cross-platform mobile app where anyone can share simple home remedies, vote on the ones that work, and sort or filter the feed to find a fix quickly. There are no accounts and no sign-up: open the app and you land on the feed.

**Team:** Morgan Davis, Kaden Fox, Aiden Davis
**Repository:** [github.com/QuietHealer/Hearth-Remedy-](https://github.com/QuietHealer/Hearth-Remedy-)
**Final presentation:** [`HomeRemedyApp.pptx`](HomeRemedyApp.pptx)

> **Disclaimer:** Hearth Remedy is a community sharing app, not a source of medical advice. Remedies are written by users and are not verified by medical professionals. See a doctor or pharmacist for serious or persistent symptoms, and for any health concern involving a child.

---

## Table of Contents

- [The Problem](#the-problem)
- [Key Features](#key-features)
- [Technologies Used](#technologies-used)
- [Architecture](#architecture)
- [Setup Instructions](#setup-instructions)
- [Using the App](#using-the-app)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [Known Limitations](#known-limitations)
- [Future Work](#future-work)
- [Retrospective](#retrospective)
- [Competitive Landscape](#competitive-landscape)

---

## The Problem

When people or their kids come down with something minor, they often don't want to see a doctor and don't want to dig through scattered articles to find something that might help. Existing remedy apps tend to be fixed catalogs written by the developer, and some hide content behind a paywall or a long questionnaire.

Hearth Remedy takes a community approach: people post the remedies their families actually use, and votes push the best ones to the top.

## Key Features

- **Browse the feed:** scroll through remedy posts, each showing its title, category, usage method, description, and a preview of the ingredients.
- **Sort:** newest first or top voted.
- **Filter:** by remedy type (Cold & Flu, Throat, Stomach, Headache, Skin, Sleep, Stress & Anxiety, Other) and by how the remedy is used (Drink, Food / Solid, On the Skin, Inhale, Bath, Other). Filters apply instantly from the top bar and can be cleared with one tap.
- **Vote:** upvote or downvote any post from the feed or the detail page. Tapping the same arrow again removes your vote, and tapping the other arrow switches it.
- **Post detail page:** full ingredient list and numbered steps for each remedy.
- **Create a remedy:** submit a title, description, ingredients, steps, category, and usage method. Input is validated and cleaned before it is saved (length limits, required fields, HTML tags stripped).
- **Anonymous by design:** each install gets a random device ID, stored on the device. The database allows only one vote per device per post.
- **Starter content:** the database is seeded with sample remedies so the feed is never empty.
- **Cross-platform project:** one C# codebase, configured to build for Android, iOS, Mac Catalyst, and Windows. Android is the platform we tested most.

## Technologies Used

| Area | Technology |
|---|---|
| Language | C# |
| Framework | .NET 10 |
| App | .NET MAUI Blazor Hybrid (Razor components inside a native MAUI shell) |
| Data access | Entity Framework Core 10 with code-first migrations |
| Database | PostgreSQL (via the Npgsql EF Core provider) |
| UI | Razor components with HTML/CSS |
| Tools | Visual Studio, Git/GitHub, Android Emulator, physical Android phone |

**Why these choices:** our team already knew C# and .NET, so we could spend our four weeks building features instead of learning a new language. MAUI Blazor Hybrid gave us a real mobile app without separate Android and iOS codebases. A hosted PostgreSQL database gives every device the same shared posts and vote counts.

**Emerging technology:** .NET MAUI Blazor Hybrid is a newer cross-platform approach that runs web-style UI inside a native app.

AI tools were used for brainstorming, mockups, research, and editing. Final design and implementation decisions were made by the team.

## Architecture

```
┌───────────────────────────────────────────────┐
│  HearthRemedies  (.NET MAUI Blazor Hybrid)    │
│  Razor pages · filter state · device ID       │
└──────────────────────┬────────────────────────┘
                       │ calls
┌──────────────────────▼────────────────────────┐
│  RemedyLib  (class library)                   │
│  RemedyService · EF Core models · migrations  │
└──────────────────────┬────────────────────────┘
                       │ EF Core / Npgsql
              ┌────────▼─────────┐
              │   PostgreSQL     │
              └──────────────────┘
```

- **`HearthRemedies`** is the app. Razor components never touch the database directly.
- **`RemedyLib`** holds the data models and `RemedyService`, which is the single place where posts, votes, and reports are read and written.
- The app talks to the PostgreSQL database directly through `RemedyService`; there is no separate web API. Because the database is remote, **the app needs an internet connection**.
- The database is the source of truth. After every vote the app re-reads the post so the on-screen count matches the server.

## Setup Instructions

### Prerequisites

- **Visual Studio** with the **.NET Multi-platform App UI development** workload, version 2026 or any release that supports .NET 10. (Visual Studio Code with the .NET MAUI extension also works.)
- **.NET 10 SDK**
- An **Android emulator** (set up through Visual Studio's Android Device Manager) or an Android phone with USB debugging enabled
- **A PostgreSQL database** the app can reach, and its connection string (see step 2)
- Git

> iOS and Mac Catalyst builds require a Mac with Xcode. Windows can be used as a build target for quick testing.

### 1. Clone the repository

```bash
git clone https://github.com/QuietHealer/Hearth-Remedy-.git
cd Hearth-Remedy-
```

### 2. Create the secrets file

The database connection string is kept out of source control, so the repo will **not build until you add it**. Create the file `RemedyLib/Secrets.cs`:

```csharp
namespace RemedyLib;

public static class Secrets
{
    public const string ConnectionString =
        "Host=YOUR_HOST;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require";
}
```

This file is listed in `.gitignore`. **Never commit it.**

> **Security note:** Keeping `Secrets.cs` out of Git only protects the password in the repository. The connection string is still compiled into the app, so anyone who gets a copy of the built app (for example, the APK) can decompile it and read the database password. This is acceptable for a class project with a throwaway database, but it is not safe for a production app. Use a low-privilege database account with a password you don't use anywhere else, and rotate it if the app is ever shared publicly. The proper fix is to put a small API between the app and the database so credentials stay on the server (see [Future Work](#future-work)).

Using a hosted database service (such as Supabase, Neon, or Azure Database for PostgreSQL)? Copy the connection details from its dashboard. For local testing, a PostgreSQL install on your own machine works too, but a physical phone or emulator will only reach it if the host address is reachable from the device.

### 3. Restore and build

```bash
cd HearthRemedies
dotnet restore
```

Or open `HearthRemedies/HearthRemedies.slnx` in Visual Studio.

### 4. Run the app

1. Set **HearthRemedies** as the startup project.
2. Choose an Android emulator or connected phone as the target.
3. Press **F5** (Run).

On first launch the app creates the database tables and loads the starter remedies automatically (`EnsureCreated`), so no manual database step is needed for a fresh, empty database.

> **Changing the data model?** The repo includes EF Core migrations in `RemedyLib/Migrations`. Add new ones from the `RemedyLib` folder with `dotnet ef migrations add <Name> --startup-project ../HearthRemedies`. Note that a database first created by `EnsureCreated` has no migration history, so use a fresh database if you switch to applying migrations with `dotnet ef database update`.

### Troubleshooting

| Problem | Fix |
|---|---|
| Build error: `The name 'Secrets' does not exist` | Create `RemedyLib/Secrets.cs` as shown in step 2. |
| App starts but the feed is empty or throws errors | Check the connection string and that the database is reachable from your device or emulator and that the device has internet. |
| Missing workload or SDK errors | Install the .NET MAUI workload and the .NET 10 SDK. |
| Emulator is slow or won't start | Confirm hardware acceleration is enabled and give the first boot a few minutes. |

## Using the App

1. **Open the app.** You land directly on the feed of remedies.
2. **Filter and sort** with the dropdowns at the top: remedy type, usage method, and Newest or Top voted. Use **Clear** to reset.
3. **Vote** with the ▲ and ▼ buttons on any post.
4. **Open a remedy** by tapping a post to see its full ingredients and steps.
5. **Share your own** with the **Create new Post** button: fill in the form and submit. If something is invalid, the form shows what to fix.

## Project Structure

```
Hearth-Remedy-/
├── README.md
├── HomeRemedyApp.pptx             # Final class presentation
├── HearthRemedies/                # .NET MAUI Blazor Hybrid app
│   ├── Components/
│   │   ├── Layout/                # MainLayout: filter and sort bar
│   │   └── Pages/                 # Home feed, post card, post detail, create post
│   ├── Controller/                # PostFilterState (shared filter/sort state)
│   ├── Model/                     # PostTransferService, enum helpers
│   ├── Platforms/                 # Android, iOS, Mac Catalyst, Windows
│   ├── PreferencesDeviceIdProvider.cs   # Random per-install device ID
│   ├── MauiProgram.cs             # Service registration and startup
│   └── wwwroot/                   # Global CSS and index.html
└── RemedyLib/                     # Shared data layer
    ├── Data/
    │   ├── Models.cs              # Post, Vote, Report, enums
    │   ├── AppDbContext.cs        # EF Core model, constraints, seed posts
    │   └── RemedyService.cs       # Feed, create, vote, and report logic
    ├── Migrations/                # EF Core migrations
    └── Secrets.cs                 # NOT in the repo: you create this file
```

## Testing

Testing was manual, on an Android emulator and a physical phone, throughout development. The project does not include an automated test suite.

Checked during testing:

- Feed loads and scrolls
- Sorting (newest, top voted) and filtering by type and usage method
- Upvote, downvote, switch vote, and remove vote
- Post detail page
- Post creation, including validation messages

## Known Limitations

- **Post reporting is not available in the app.** The data model and `RemedyService.ReportPostAsync` support reporting (a report flags the post and hides it after three reports), but the Report button in the UI is commented out.
- **Abuse protection is switched off.** The server-side blocked-word filter and the posts-per-hour limit are written but disabled, and the word list is still a placeholder.
- **No in-app "not medical advice" notice.** The disclaimer appears in this README but not yet inside the app.
- **No date-range filter in the UI.** The service supports "this week / this month / all time", but the app currently offers type, method, and sort controls only.
- **No favorites, accounts, or comments.**
- **The post creation screen is basic** and needs visual polish.
- **Database credentials ship inside the app.** The connection string is compiled into the build and can be recovered by decompiling it (see the security note in [Setup Instructions](#setup-instructions)).
- **Requires internet**, since all data lives in the remote database.
- **Anonymous device IDs** reset if the app is reinstalled, so votes cast before a reinstall can't be matched to the same user.

## Future Work

- Turn the Report button back on and build a moderator review view
- Enable the word filter and posting rate limit with a real word list
- Add a visible "not medical advice" notice on the detail and create screens
- Add the date-range filter to the top bar
- Accounts, favorites, and saved posts
- Comments on remedies
- Automated tests
- Move database access behind a small API so the connection string is never shipped inside the app

## Retrospective

**What went well**

- The app runs on mobile and looks good overall
- The database layer is unified and simple, with all access in one service
- Sorting and filtering work well
- Team attendance and communication were strong

**What did not go as planned**

- We had less development time than expected
- Post reporting and the abuse protections were built on the data side but didn't make it into the finished app
- Favorites and saved posts did not make it in
- We changed our approach mid-project: the original plan was an ASP.NET Core API with SQLite, and we ended up connecting the app to PostgreSQL directly through a shared library

**Lessons learned**

- Communication is key
- Object responsibilities need to be clearly defined to prevent different interpretations of the same thing

## Competitive Landscape

| App | Content model | Notes |
|---|---|---|
| Home Remedies: Natural Cure (Android) | Developer-written catalog | Large library with search and categories; no user contributions |
| Verda: Home Apothecary (iOS) | Expert-reviewed, curated | In-app purchases and paywall complaints; iPhone only |
| Reddit | Community posts and votes | General-purpose and unstructured, not remedy-specific |

**How Hearth Remedy differs:** remedies come from the community, votes surface the best ones, posts have a consistent structure (ingredients, steps, usage method), and there is no account or paywall.
