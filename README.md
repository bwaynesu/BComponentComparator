# BComponentComparator

A Unity Editor tool for comparing multiple Components, ScriptableObjects, or Materials side-by-side in a single window.

## Features

- **Side-by-Side Comparison**: View multiple Unity Inspectors in parallel columns for easy visual comparison
- **Flexible Object Selection**: Support for GameObjects (Components), ScriptableObjects, and Materials
- **Drag-and-Drop Interface**: Intuitive drag-and-drop to specify component type and add objects to compare
- **Native Inspector Support**: Uses Unity's native Inspector rendering, ensuring full editing capabilities including:
  - Undo/Redo
  - Copy/Paste Component Values
  - Custom Inspectors
  - All field types and widgets
- **Reorderable List**: Drag items in the list to reorder, with Inspector columns updating in real-time
- **Multi-Selection Sync**: Select multiple items in the list to highlight them in the Unity Editor
- **Adjustable Column Width**: Customize Inspector column width using a slider (200-600 pixels)
- **Persistent After Docking**: Drag-and-drop functionality remains stable when moving or docking the window

## Installation

1. Copy the `BComponentComparator` folder into your project's `Assets` directory
2. The tool will be available under `Window > BTools > BComponentComparator`

## Usage

### Basic Workflow

1. **Open the Window**: Go to `Window > BTools > BComponentComparator`
2. **Specify Component Type**: 
   - Drag a Component from the Scene/Hierarchy onto the "Drag Component type here..." field
   - Or drag a MonoScript (.cs file) from the Project window
   - Or drag a ScriptableObject/Material asset type
3. **Add Objects to Compare**:
   - Drag GameObjects from the Hierarchy into the list (must have the specified Component)
   - Or drag ScriptableObject/Material assets from the Project window
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
- Range: 200-600 pixels

#### Removing Items
- Hover over a list item to reveal the × button, click to remove
- Or hover over an Inspector column header to reveal the × button
- Or click the "Clear List" button to remove all items at once

## Supported Types

- **GameObject Components**: Any Component attached to GameObjects in the scene
- **ScriptableObjects**: Any ScriptableObject asset in the project
- **Materials**: Material assets in the project
- **Not Supported**: Prefab assets (drag Prefab instances from the scene instead)

## Requirements

- Unity 2022.3 LTS or later
- UI Toolkit support

## Tips

- **Use for Bulk Editing**: Compare and adjust properties across multiple objects efficiently
- **Quality Control**: Quickly spot inconsistencies in Component settings
- **Data Migration**: Copy settings from one object to many others using native copy/paste
- **Custom Inspectors**: Fully supports custom Inspector drawers and property drawers

## Known Limitations

- Prefab assets are not supported (use Prefab instances in the scene instead)
- Very large numbers of columns (50+) may impact performance

## License

See LICENSE file for details.
