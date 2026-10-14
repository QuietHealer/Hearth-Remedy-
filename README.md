# Hearth Remedy

*Home remedies, shared by the families who use them.*

## Project Description

Hearth Remedy is a mobile app for sharing and discovering simple home remedies. When people or their kids get sick with a minor ailment, they often don't want to visit a doctor (although a doctor is still recommended for anything serious) and don't want to spend time searching through scattered articles. With Hearth Remedy, they can quickly find a simple remedy that other people have shared and ranked.

**Planned features (MVP):**

- Create a remedy post with a title, ingredients, steps, how to use it (skin, eat, or breathe), and a category
- Browse a scrollable feed of remedies and open a post for full details
- Sort by votes or date range (this week, this month, all time) and filter by category
- Upvote or downvote posts so the community decides which remedies rise to the top
- Anonymous use with no sign-up, plus a "not medical advice" notice and a report button for moderation

**Stretch goals:** accounts with favorites, comments on posts, and keyword search.

> Hearth Remedy is not a substitute for medical care. Remedies are shared by users and are not verified by medical professionals. See a doctor if symptoms are serious or persistent.

## Team Members

- [Morgan Davis]
- [Kaden Fox]
- [Aiden Davis]

## Technologies

| Area | Technology |
| --- | --- |
| Language | C# |
| Mobile app | .NET MAUI Blazor Hybrid (Android and iOS from one codebase) |
| Backend | ASP.NET Core Web API |
| Database | SQLite with Entity Framework Core |
| Front-end markup and styling | HTML/CSS in Razor components (JavaScript only for Blazor interop, if needed) |
| Development tools | Visual Studio 2022, Git and GitHub, Android Emulator and a physical phone |
| AI tools | Used for brainstorming, mockups, research, and editing |

## Setup and Installation

> The project is in early development. The folder names and commands below describe the planned layout and will be updated as the code is added.

### Prerequisites

- [Visual Studio 2026](https://visualstudio.microsoft.com/) with the **.NET Multi-platform App UI development** and **ASP.NET and web development** workloads
- The [.NET SDK](https://dotnet.microsoft.com/download) version required by the project
- Git
- An Android emulator (installed through Visual Studio) or an Android phone with USB debugging enabled
- iOS builds require a Mac. The team is targeting Android first.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/QuietHealer/Hearth-Remedy-.git
   cd Hearth-Remedy-
   ```

2. Open the solution file (`.sln`) in Visual Studio 2022.

3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Create the local SQLite database by applying migrations (once the backend is added):

   ```bash
   dotnet ef database update --project HearthRemedy.Api
   ```

5. Start the API. Set `HearthRemedy.Api` as the startup project in Visual Studio and run it, or use:

   ```bash
   dotnet run --project HearthRemedy.Api
   ```

6. Run the mobile app. Set `HearthRemedy.App` as the startup project, choose the Android Emulator or a connected phone as the target, and press **Run**.

## Project Structure (planned)

```
Hearth-Remedy-/
├── HearthRemedy.App/    # .NET MAUI Blazor Hybrid mobile app
├── HearthRemedy.Api/    # ASP.NET Core Web API and SQLite database
└── README.md
```

## Development Timeline

| Week | Focus |
| --- | --- |
| 1 | Environment setup and an app that runs on a phone with pre-made posts |
| 2 | Backend, database, real posts, and voting |
| 3 | Post creation, sorting, filtering, and reporting |
| 4 | Testing, polish, and stretch goals |
