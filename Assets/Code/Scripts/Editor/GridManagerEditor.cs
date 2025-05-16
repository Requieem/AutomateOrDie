// using Code.Scripts.Runtime;
// using Code.Scripts.Runtime.Grid;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace Code.Scripts.Editor
// {
//     using UnityEditor;
//     [CustomEditor(typeof(GridManager))]
//     public class GridManagerEditor : Editor
//     {
//         private Vector2Int m_selectingCell;
//
//         public override VisualElement CreateInspectorGUI()
//         {
//             var root = base.CreateInspectorGUI() ?? new VisualElement();
//             var centerProp = serializedObject.FindProperty("m_center");
//             var cellSizeProp = serializedObject.FindProperty("m_cellSize");
//             var boundsProp = serializedObject.FindProperty("m_bounds");
//             var structures = serializedObject.FindProperty("m_buildings");
//             var floorProp = serializedObject.FindProperty("m_floor");
//             var wallsProp = serializedObject.FindProperty("m_walls");
//             var charactersProp = serializedObject.FindProperty("m_characters");
//             var structuresTilemapProp = serializedObject.FindProperty("m_buildingsTilemap");
//
//             var centerField = new Vector2Field("Center");
//             centerField.BindProperty(centerProp);
//             var cellSizeField = new Vector2Field("Cell Size");
//             cellSizeField.BindProperty(cellSizeProp);
//             var boundsField = new Vector2Field("Bounds");
//             boundsField.BindProperty(boundsProp);
//             var structuresField = new PropertyField(structures);
//             structuresField.BindProperty(structures);
//             var floorField = new PropertyField(floorProp);
//             floorField.BindProperty(floorProp);
//             var wallsField = new PropertyField(wallsProp);
//             wallsField.BindProperty(wallsProp);
//             var charactersField = new PropertyField(charactersProp);
//             charactersField.BindProperty(charactersProp);
//             root.Add(centerField);
//             root.Add(cellSizeField);
//             root.Add(boundsField);
//             root.Add(structuresField);
//             root.Add(floorField);
//             root.Add(wallsField);
//             root.Add(charactersField);
//
//             var gridManager = (GridManager)target;
//             var selectingCellField = new Vector2IntField("Selecting Cell")
//             {
//                 value = m_selectingCell
//             };
//
//             selectingCellField.RegisterValueChangedCallback(evt =>
//             {
//                 m_selectingCell = evt.newValue;
//             });
//
//             var setSelectedCellButton = new Button(() =>
//             {
//                 gridManager.SelectedCell = m_selectingCell;
//                 EditorUtility.SetDirty(gridManager);
//             })
//             {
//                 text = "Set Selected Cell"
//             };
//
//             var clearSelectedCellButton = new Button(() =>
//             {
//                 gridManager.SelectedCell = null;
//                 EditorUtility.SetDirty(gridManager);
//             })
//             {
//                 text = "Clear Selected Cell"
//             };
//
//             root.Add(selectingCellField);
//             root.Add(setSelectedCellButton);
//             root.Add(clearSelectedCellButton);
//             return root;
//         }
//     }
// }