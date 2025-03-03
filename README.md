# Summoner Tactics

A 2D game project built in Unity.

## Project Overview

Summoner Tactics is a 2D game that features:
- A top-down camera system with zoom functionality
- Core game management systems
- Map management

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Basic knowledge of Unity and C#

### Installation

1. Clone this repository
2. Open the project in Unity
3. Open the main scene from `Assets/_Project/Scenes`

## Project Structure

The project is organized into the following directories:

- **Scripts**: Contains all C# scripts organized by functionality
  - **Core**: Core game systems
    - **GameManager**: Central game management scripts
  - **Utils**: Utility scripts and helper functions
- **Prefabs**: Reusable game objects
  - **Map**: Map-related prefabs
- **Scenes**: Game scenes
- **Art**: Visual assets
  - **Sprites**: 2D sprite assets
- **ScriptableObjects**: Data containers

## Core Systems

### GameManager

The GameManager is responsible for:
- Managing the game state
- Handling scene transitions
- Coordinating other manager systems

### CameraController

The CameraController handles:
- Camera positioning and movement
- Zoom functionality
- Map boundary enforcement

## Controls

- **Mouse Wheel**: Zoom in/out
- **R Key**: Reset camera position and zoom

## Development Guidelines

- Follow Unity's component-based architecture
- Use descriptive variable and function names
- Keep scripts focused on a single responsibility
- Document public methods and properties
- Use ScriptableObjects for configuration data

## License

This project is licensed under the MIT License - see the LICENSE file for details. 