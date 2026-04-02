# Fleshgolem

Fleshgolem is a C# based game engine/prototype focused on the creation and management of entities composed of modular body parts. The game allows for the construction of unique creatures (like a Homunculus) by attaching limbs and body parts with distinct attributes.

## Features

- **Modular Entity System**: Create entities by attaching or detaching specific body parts (Head, Chest, Abdomen, Arms, Legs, and Tail).
- **Attribute System**: Each body part contributes to the entity's overall stats, including Strength, Size, and Dexterity.
- **Dynamic Stat Calculation**: Entities dynamically calculate their base attributes based on the parts currently attached.
- **Data-Driven Parts**: Body parts are populated from a data-driven structure, allowing for different creature types (e.g., Manticore, Human).
- **Ruleset Integration**: Includes a basic ruleset with common RPG attributes and dice definitions.

## Ruleset

The game ruleset is loosely based on [Mythras Imperative RPG](https://www.drivethrurpg.com/product/185335/Mythras-Imperative), providing a foundation for character attributes and dice mechanics.

## Project Structure

- `Program.cs`: The entry point of the application, demonstrating entity creation and limb management.
- `Entity/`: Contains the core logic for `Entity`, `Limb`, and `BodyPart` classes.
- `Data/`: Handles the population of game data (e.g., `Bodyparts.cs`).
- `Ruleset/`: Defines the core mechanics, such as attributes (`BaseAttribute.cs`) and dice (`Dice.cs`).

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (Version 6.0 or later recommended)

### Running the Project

1. Clone the repository.
2. Navigate to the project directory:
   ```bash
   cd Fleshgolem
   ```
3. Build and run the project:
   ```bash
   dotnet run
   ```

## Future Plans

- Implementation of a combat system using the `Dice` ruleset.
- Expanded creature and body part library.
- Saving/Loading entity configurations.
