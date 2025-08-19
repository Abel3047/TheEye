🌪️ The Eye Almanac
A Dynamic Storm Simulation Tool for Narrative Gameplay

The Eye Almanac is a C# project that simulates the movement and behavior of colossal, never-ending hurricanes in a post-apocalyptic world ravaged by runaway fusion reactors and climate collapse.

Designed for use alongside AboveVTT or other virtual tabletop tools, this middleware provides real-time data on storm behavior—turning environmental storytelling into a visceral, dynamic experience for players.

🎮 Gameplay Purpose

In the campaign world, the Eye of the Storm is both sanctuary and prison. Civilization survives only within its calm center, while the deadly Eye Wall outside makes escape nearly impossible. The Eye Almanac helps Dungeon Masters inject tension into sessions by:

-Simulating Eye movement across the world map (6–18 km per in-game day).
-Allowing unpredictable shifts in storm direction, forcing nomadic tribes to adapt.
-Tracking diameter changes—an Eye shrinking in size can create panic as once-safe havens collapse into chaos.
-Generating DM-facing data such as Eye speed, vector changes, and storm intensity.

Players can see this information via a lightweight HTML/JS dashboard that updates live, creating a feeling of urgency and inevitability.

🛠️ Technical Overview

Built with Clean Architecture principles, the code is separated into distinct layers:

-Domain Layer → Core storm models and simulation logic.
-Application Layer → Services for Eye behavior, direction randomness, and rules for contraction/expansion.
-Infrastructure Layer → Data persistence and middleware hooks.
-Presentation Layer → API endpoints for DM interaction.

Features:

-Written in C# (.NET 8) with extensibility in mind.
-REST endpoints for jump inputs (e.g., advance storm by X days).
-HTML/JS dashboard for visualization.
-Modular controllers for clarity and best practices.

📂 Project Structure
EyeAlmanac/
 ├── Domain/
 │   ├── Entities/
 │   └── Services/
 ├── Application/
 │   └── Interfaces/
 ├── Infrastructure/
 │   └── Persistence/
 ├── Presentation/
 │   ├── Controllers/
 │   └── Dashboard/ (HTML + JS)
 ├── Program.cs
 └── README.md

🌍 Narrative Context

The Eye Almanac is more than code—it’s a storytelling device. It embodies the constant dread of survival in a world where the very atmosphere is weaponized. By introducing unpredictability and scarcity through storm dynamics, DMs can reinforce the themes of:

-Fragility of civilization under overwhelming natural forces.
-Tension of scarcity, as food, water, and parts must be scavenged before the Eye moves again.
-The inevitability of change, forcing difficult moral and tactical decisions.

📺 Twitch & Public Engagement

This project is intended for dual use:

-Employers & Collaborators → Demonstrates ability to design clean, modular C# systems and integrate narrative mechanics into code.
-Twitch & Community → Adds a real-time "weather report" layer to roleplay, letting viewers track the Eye’s progression like an apocalyptic news broadcast.

Ultimately this project is a display of my love to code and my even greater love for story telling in Dungeons and Dragons as a Dungeon Master
