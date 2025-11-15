# BComponentComparator

[![Release Version](https://img.shields.io/github/v/release/bwaynesu/BComponentComparator?include_prereleases)](https://github.com/bwaynesu/BComponentComparator/releases) 
[![Release Date](https://img.shields.io/github/release-date/bwaynesu/BComponentComparator.svg)](https://github.com/bwaynesu/BComponentComparator/releases)  
[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg?style=flat&logo=unity)](https://unity3d.com/get-unity/download) 
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) 
[![Twitter](https://img.shields.io/twitter/follow/bwaynesu.svg?label=Follow&style=social)](https://x.com/intent/follow?screen_name=bwaynesu)

**Stop switching between Inspector windows.**   
Compare and edit multiple Unity objects side-by-side, spot inconsistencies instantly, and maintain quality across your entire project — all in a single, synchronized view.

<img width="700" alt="Preview" src="https://github.com/user-attachments/assets/05294e56-962d-4598-9ddb-dbe10c4963f0" />

## Why BComponentComparator?

Ever found yourself:
- Clicking between dozens of GameObjects to check if their settings match?
- Copy-pasting Component values one by one across multiple objects?
- Missing subtle differences in Material properties that break visual consistency?
- Wishing you could see all your ScriptableObject data side-by-side?

**Unity's Inspector shows one object at a time. Your workflow shouldn't be limited by that.**

BComponentComparator gives you a parallel view of multiple Inspectors, letting you compare, edit, and synchronize objects effortlessly — saving hours of tedious clicking and ensuring nothing slips through the cracks.

## How It Works

**Simple drag-and-drop workflow:**
1. Drag any Component or asset to define what you want to compare
2. Drag your objects into the list
3. View and edit all Inspectors side-by-side with full Unity editing support

Native Inspector rendering means everything just works — Undo/Redo, Copy/Paste, Custom Inspectors, and all field types.

## Features

- **Side-by-Side Comparison**: View multiple Unity Inspectors in parallel columns for easy visual comparison
- **Flexible Object Selection**: Support for almost any Unity object type including Components, ScriptableObjects, Materials, Textures, Audio Clips, and more
- **Drag-and-Drop Interface**: Intuitive drag-and-drop to specify object type and add objects to compare
- **Native Inspector Support**: Uses Unity's native Inspector rendering, ensuring full editing capabilities including:
  - Undo/Redo
  - Copy/Paste Component Values
  - Custom Inspectors
  - All field types and widgets
- **Reorderable List**: Drag items in the list to reorder, with Inspector columns updating in real-time
- **Multi-Selection Sync**: Select multiple items in the list to highlight them in the Unity Editor
- **Adjustable Column Width**: Customize Inspector column width using a slider

## Real-World Use Cases

### Level Design Quality Control
Compare dozens of enemy spawners or interactive objects to ensure consistent difficulty curves and behavior settings across your game.

### Visual Consistency
Line up all your Materials side-by-side to verify shader parameters, texture assignments, and rendering settings match your art direction.

### Data-Driven Development
Review multiple ScriptableObject configurations simultaneously — perfect for balancing game items, character stats, or ability definitions.

### Bulk Configuration
Copy-paste Component values across multiple objects in seconds instead of minutes, then fine-tune individual differences with immediate visual feedback.

## Installation

1. **Unity Package Manager**
    - Open `Window` > `Package Manager`
    - Click `+` > `Install package from git URL...`
    - Enter: `https://github.com/bwaynesu/BComponentComparator.git?path=Project/Packages/BComponentComparator`

2. **Manual Installation**
    - Download the latest `.unitypackage` from the [Releases](https://github.com/bwaynesu/BComponentComparator/releases) page
    - In Unity, double-click the `.unitypackage` or use `Assets > Import Package > Custom Package...` to import

## Usage

### Basic Workflow

1. **Open the Window**: Go to `Window > BTools > BComponentComparator`
2. **Specify Object Type**: 
   - Drag a Component from the Scene/Hierarchy onto the "Drag Component type here..." field
   - Or drag a MonoScript (.cs file) from the Project window
   - Or drag any Unity asset (ScriptableObject, Material, Texture, Audio Clip, etc.)
3. **Add Objects to Compare**:
   - Drag GameObjects from the Hierarchy into the list (must have the specified Component)
   - Or drag any matching asset type from the Project window
   - Multiple objects can be dragged at once
4. **Compare and Edit**:
   - View Inspectors side-by-side in the right panel
   - Scroll horizontally to see more columns
   - All Inspectors scroll vertically in sync
   - Edit values directly in any Inspector
   - Use right-click menu to copy/paste Component values

### Advanced Features

#### Reordering Items
- Click and drag items in the list to reorder them
- Inspector columns update immediately to match the new order

#### Multi-Selection
- Hold Ctrl/Cmd to select multiple items
- Selected items are highlighted in both the list and Inspector columns
- Unity's Selection is synced with your list selection

#### Adjusting Column Width
- Use the "Inspector Width" slider to adjust all column widths simultaneously

#### Removing Items
- Hover over a list item to reveal the × button, click to remove
- Or hover over an Inspector column header to reveal the × button
- Or click the "Clear List" button to remove all items at once

## Supported Types

- **GameObject Components**: Any Component attached to GameObjects in the scene
- **ScriptableObjects**: Any ScriptableObject asset in the project
- **Materials**: Material assets in the project
- **Textures**: Texture2D, RenderTexture, and other texture assets
- **Audio**: AudioClip assets
- **Animations**: AnimationClip, AnimatorController, and related assets
- **Shaders**: Shader and ShaderGraph assets
- **And More**: Almost any Unity object type that can be displayed in the Inspector
- **Not Supported**: Prefab assets (drag Prefab instances from the scene instead)

## Requirements

- Unity 2022.3 LTS or later
- UI Toolkit support

## Known Limitations

- Prefab assets are not supported (use Prefab instances in the scene instead)
- Very large numbers of columns (50+) may impact performance

## License

This project is under the [MIT License](LICENSE).

## Author

- [bwaynesu](https://bwaynesu.github.io/portfolio/) 

## Links

- [Repository](https://github.com/bwaynesu/BComponentComparator)
- [Releases](https://github.com/bwaynesu/BComponentComparator/releases)
- [Issues](https://github.com/bwaynesu/BComponentComparator/issues)
- [Discussions](https://github.com/bwaynesu/BComponentComparator/discussions)

## See Also

- [GitHub](https://bwaynesu.github.io/portfolio/)
- [Asset Store](https://assetstore.unity.com/publishers/115148)
- [Medium](https://medium.com/@bwaynesu)
- [X](https://x.com/bwaynesu)
- [YouTube](https://www.youtube.com/@bwaynesu)

---

**BComponentComparator** — Spot inconsistencies, bulk adjust settings, and save hours!
