# BComponentComparator

Compare multiple Unity objects side-by-side with native Inspector editing.

## Quick Start

1. **Open Window**: `Window > BTools > BComponentComparator`
2. **Set Type**: Drag any Component, asset, or MonoScript to the top field
3. **Add Objects**: Drag matching objects into the list
4. **Compare**: View and edit all Inspectors side-by-side

## Features

- Native Inspector rendering (full editing support)
- Inheritance support (drag derived classes)
- Drag-and-drop workflow
- Reorderable list with synchronized columns
- Multi-selection support

## Supported Types

- Almost any Inspector-compatible Unity object

## Display Mode Customization

If an Inspector displays incorrectly or appears empty, you can customize how it renders:

1. **Change Display Mode**: Use the "Display Mode" dropdown in the left panel
2. **Available Modes**:
   - **Element**: Default rendering using InspectorElement (fastest)
   - **Editor**: Full IMGUI rendering (best compatibility)
   - **EditorThenElement**: Hybrid mode combining both approaches
3. **Auto-Save**: Settings are saved per object type and persist across sessions
4. **Project Settings**: View and manage all custom modes in `Edit > Project Settings > BComponentComparator`

## Tips

- **Bulk Editing**: Edit multiple objects simultaneously
- **Copy/Paste**: Right-click Inspector to copy/paste values between objects
- **Reorder**: Drag list items to change column order
- **Multi-Select**: Hold Ctrl/Cmd to select multiple items

## Requirements

- Unity 2022.3 LTS or later

## Links

- [GitHub Repository](https://github.com/bwaynesu/BComponentComparator)
- [Full Documentation](https://github.com/bwaynesu/BComponentComparator/blob/main/README.md)
- [Issue Tracker](https://github.com/bwaynesu/BComponentComparator/issues)

## License

MIT License - See [LICENSE](https://github.com/bwaynesu/BComponentComparator/blob/main/LICENSE)
