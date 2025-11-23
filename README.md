# BComponentComparator

[![Release Version](https://img.shields.io/github/v/release/bwaynesu/BComponentComparator?include_prereleases)](https://github.com/bwaynesu/BComponentComparator/releases) 
[![Release Date](https://img.shields.io/github/release-date/bwaynesu/BComponentComparator.svg)](https://github.com/bwaynesu/BComponentComparator/releases)  
[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg?style=flat&logo=unity)](https://unity3d.com/get-unity/download) 
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) 
[![Twitter](https://img.shields.io/twitter/follow/bwaynesu.svg?label=Follow&style=social)](https://x.com/intent/follow?screen_name=bwaynesu)

**Stop switching between Inspector windows.**   
Compare and edit multiple Unity objects side-by-side, spot inconsistencies instantly, and maintain quality across your entire project — all in a single, synchronized view.

<img width="900" alt="Preview" src="https://github.com/user-attachments/assets/e676f25c-7474-4806-891d-edee3ab1cf85" />

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

<!-- https://github.com/user-attachments/assets/caa12751-699b-432d-bc69-6eb8385326be -->

## Features

- **Drag-and-Drop Interface**: Intuitive drag-and-drop to specify object type and add objects to compare
  ![DragDrop-optimize](https://github.com/user-attachments/assets/b3675555-d084-4272-8293-e5a851be801b)

- **Side-by-Side Comparison**: View multiple Unity Inspectors in parallel columns for easy visual comparison
- **Flexible Object Selection**: Support for almost any Unity object type including Components, ScriptableObjects, Materials, Textures, Audio Clips, and more
  ![AssetTest-optimize](https://github.com/user-attachments/assets/f9f70881-519e-4d40-a23a-0d6aa14df8f0)

- **Native Inspector Support**: Uses Unity's native Inspector rendering, ensuring full editing capabilities including:
  - Undo/Redo
  - Copy/Paste Component Values
  - Custom Inspectors (including third-party tools like Odin Inspector)
  - All field types and widgets

  ![CopyPaste-optimize](https://github.com/user-attachments/assets/64c72be7-d135-426c-b637-7e85786abe4c)
  
- **Inheritance Selector**: Drag a component and use the dropdown to select a base type (e.g., drag `BoxCollider`, select `Collider`, then compare with `SphereCollider`)
  ![Inherit-optimize](https://github.com/user-attachments/assets/4af000d3-b758-486e-9589-256ac91d9d6e)

- **Reorderable List**: Drag items in the list to reorder, with Inspector columns updating in real-time
- **Multi-Selection Sync**: Select multiple items in the list to highlight them in the Unity Editor
- **Context Menu Integration**: Right-click any Component or asset in the Inspector and select "Add to Comparator" to quickly add it to the comparison list
  ![ContextMenu-optimize](https://github.com/user-attachments/assets/d70b77d9-2568-4fcf-8765-8ddbc0fb9e03)

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
   - **(Optional)** Use the "Use Type" dropdown to select a base class (e.g., `Collider`) for polymorphic comparison
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

#### Using Context Menu
- Right-click any Component or asset in the Inspector
- Select "Add to Comparator" from the context menu
- The Comparator window will open (or focus if already open)
- The object's type will be automatically set, and the object will be added to the list
- Perfect for quickly comparing multiple instances of the same type

#### Customizing Display Mode

If an Inspector displays incorrectly or appears empty compared to Unity's default Inspector:

1. **Select the object type** by dragging it to the top field
2. **Try different display modes** using the "Display Mode" dropdown:
   - **Element**: Default rendering (fastest, but may miss some asset types)
   - **Editor**: Full IMGUI rendering (best compatibility for Materials, Fonts, Textures)
   - **EditorThenElement**: Hybrid approach (useful for Shaders and complex assets)
3. **Settings persist automatically** — once configured, the mode is remembered for that object type
4. **Manage settings** in `Edit > Project Settings > BComponentComparator`

**Pre-configured types**:
| Type                      | Display Mode         |
|---------------------------|----------------------|
| `DefaultAsset`            | `Editor`             |
| `Texture2D`               | `Editor`             |
| `Font`                    | `Editor`             |
| `AssemblyDefinitionAsset` | `Editor`             |
| `InputActionAsset`        | `Editor`             |
| `Shader`                  | `EditorThenElement`  |

#### Removing Items
- Hover over a list item to reveal the × button, click to remove
- Or hover over an Inspector column header to reveal the × button
- Or click the "Clear List" button to remove all items at once

## Supported Types

- Almost any Unity object type that can be displayed in the Inspector

## Requirements

- Unity 2022.3 LTS or later
- UI Toolkit support

## Known Limitations

- Some asset types may require Display Mode adjustment for proper rendering (see Display Mode Customization above)

## Under Evaluation

Features being considered for future releases:

- **Session Persistence**: Save and load previous comparison sessions (type and list items)

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
