namespace BTools.BComponentComparator.Editor
{
    /// <summary>
    /// Defines how an Inspector should be rendered for a specific object type
    /// </summary>
    public enum InspectorDisplayMode
    {
        /// <summary>
        /// Default rendering using InspectorElement
        /// </summary>
        Element = 0,

        /// <summary>
        /// Use IMGUIContainer and Editor.CreateEditor for rendering
        /// </summary>
        Editor,

        /// <summary>
        /// First render using Editor.CreateEditor, then render the remaining properties using InspectorElement
        /// </summary>
        EditorThenElement,
    }
}