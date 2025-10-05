using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImagePreprocessEditor : EditorWindow
{
    private ImagePreprocessData currentData;
    private CharacterMark editingMark;
    private bool isCreatingNewMark = false;

    private Vector2 mainScrollPosition;
    private Vector2 listScrollPosition;

    // Image display
    private Rect imageRect;
    private float imageZoom = 1f;
    private Vector2 imageOffset = Vector2.zero;

    // Selected mark for editing
    private int selectedMarkIndex = -1;

    // Color settings
    private bool showColorSettings = false;
    private Color pivotColor = Color.red;
    private Color editingBoxColor = Color.cyan;
    private Color normalBoxColor = Color.green;
    private Color selectedBoxColor = Color.yellow;
    private Color noteColor = Color.black;

    [MenuItem("LD58/Image Preprocess Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<ImagePreprocessEditor>("Image Preprocess");
        window.minSize = new Vector2(800, 600);
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (currentData == null)
        {
            EditorGUILayout.HelpBox("Load or create an ImagePreprocessData asset to start editing.", MessageType.Info);
            return;
        }

        // Main scrollable content area
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);

        // Upper section: Image + Edit panel
        EditorGUILayout.BeginHorizontal();

        // Left: Image display
        DrawImagePanel();

        // Right: Edit panel (only when creating/editing)
        DrawEditPanel();

        EditorGUILayout.EndHorizontal();

        // Lower section: Marks list
        DrawMarksList();

        EditorGUILayout.Space(40f);

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CreateNewData();
        }

        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            LoadData();
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            SaveData();
        }

        GUILayout.FlexibleSpace();

        if (currentData != null)
        {
            EditorGUILayout.LabelField($"Editing: {currentData.name}", GUILayout.Width(200));
        }

        EditorGUILayout.EndHorizontal();

        // Data fields
        if (currentData != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            currentData.ID = EditorGUILayout.IntField("ID", currentData.ID);
            currentData.title = EditorGUILayout.TextField("Title", currentData.title);

            EditorGUI.BeginChangeCheck();
            currentData.image = (Sprite)EditorGUILayout.ObjectField("Image", currentData.image, typeof(Sprite), false);
            if (EditorGUI.EndChangeCheck())
            {
                // Reset zoom when image changes
                imageZoom = 1f;
                imageOffset = Vector2.zero;
            }

            // Color settings foldout
            showColorSettings = EditorGUILayout.Foldout(showColorSettings, "Display Colors");
            if (showColorSettings)
            {
                EditorGUI.indentLevel++;
                pivotColor = EditorGUILayout.ColorField("Pivot Color", pivotColor);
                editingBoxColor = EditorGUILayout.ColorField("Editing Box Color", editingBoxColor);
                normalBoxColor = EditorGUILayout.ColorField("Normal Box Color", normalBoxColor);
                selectedBoxColor = EditorGUILayout.ColorField("Selected Box Color", selectedBoxColor);
                noteColor = EditorGUILayout.ColorField("Note Text Color", noteColor);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawImagePanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));

        if (currentData != null && currentData.image != null)
        {
            // Zoom controls (fixed size, not affected by zoom)
            EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width * 0.65f - 20));
            EditorGUILayout.LabelField("Zoom:", GUILayout.Width(50));
            imageZoom = EditorGUILayout.Slider(imageZoom, 0.1f, 3f, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                imageZoom = 1f;
                imageOffset = Vector2.zero;
            }
            EditorGUILayout.EndHorizontal();

            // Get sprite's texture rect (important for sprite sheets)
            Rect spriteRect = currentData.image.textureRect;

            // Image display area
            Rect imageAreaRect = GUILayoutUtility.GetRect(
                spriteRect.width * imageZoom,
                spriteRect.height * imageZoom,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            imageRect = new Rect(
                imageAreaRect.x + imageOffset.x,
                imageAreaRect.y + imageOffset.y,
                spriteRect.width * imageZoom,
                spriteRect.height * imageZoom
            );

            // Draw image (only the sprite's rect, not entire texture)
            GUI.DrawTextureWithTexCoords(imageRect, currentData.image.texture,
                new Rect(
                    spriteRect.x / currentData.image.texture.width,
                    spriteRect.y / currentData.image.texture.height,
                    spriteRect.width / currentData.image.texture.width,
                    spriteRect.height / currentData.image.texture.height
                ));

            // Draw existing marks
            DrawMarks();

            // Handle mouse input
            HandleImageInput(imageAreaRect);
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a sprite to start editing marks.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMarks()
    {
        if (currentData.characters == null) return;

        Rect spriteRect = currentData.image.textureRect;

        for (int i = 0; i < currentData.characters.Count; i++)
        {
            var mark = currentData.characters[i];

            // Convert texture coords to screen coords (with Y flip)
            // Texture coords: bottom-left origin, GUI coords: top-left origin
            float minScreenX = imageRect.x + (mark.min.x - spriteRect.x) * imageZoom;
            float minScreenY = imageRect.y + (spriteRect.height - (mark.min.y - spriteRect.y)) * imageZoom;
            float maxScreenX = imageRect.x + (mark.max.x - spriteRect.x) * imageZoom;
            float maxScreenY = imageRect.y + (spriteRect.height - (mark.max.y - spriteRect.y)) * imageZoom;

            // Note: after Y flip, min and max Y are swapped
            Rect boundingBox = new Rect(
                minScreenX,
                Mathf.Min(minScreenY, maxScreenY),
                maxScreenX - minScreenX,
                Mathf.Abs(maxScreenY - minScreenY)
            );

            // Draw bounding box
            Color boxColor = (i == selectedMarkIndex) ? selectedBoxColor : normalBoxColor;
            DrawRect(boundingBox, boxColor, 2f);

            // Draw pivot
            Vector2 pivotScreen = new Vector2(
                imageRect.x + (mark.pivot.x - spriteRect.x) * imageZoom,
                imageRect.y + (spriteRect.height - (mark.pivot.y - spriteRect.y)) * imageZoom
            );
            DrawCircle(pivotScreen, 2f, pivotColor);

            // Draw label
            string displayLabel = string.IsNullOrEmpty(mark.editorNote) ? mark.text : mark.editorNote;
            Color originalColor = GUI.color;
            GUI.color = noteColor;
            GUI.Label(new Rect(boundingBox.x, boundingBox.y - 20, 100, 20), displayLabel);
            GUI.color = originalColor;
        }

        // Draw editing mark
        if (isCreatingNewMark && editingMark != null)
        {
            float minScreenX = imageRect.x + (editingMark.min.x - spriteRect.x) * imageZoom;
            float minScreenY = imageRect.y + (spriteRect.height - (editingMark.min.y - spriteRect.y)) * imageZoom;
            float maxScreenX = imageRect.x + (editingMark.max.x - spriteRect.x) * imageZoom;
            float maxScreenY = imageRect.y + (spriteRect.height - (editingMark.max.y - spriteRect.y)) * imageZoom;

            Rect boundingBox = new Rect(
                minScreenX,
                Mathf.Min(minScreenY, maxScreenY),
                maxScreenX - minScreenX,
                Mathf.Abs(maxScreenY - minScreenY)
            );
            DrawRect(boundingBox, editingBoxColor, 5f);

            Vector2 pivotScreen = new Vector2(
                imageRect.x + (editingMark.pivot.x - spriteRect.x) * imageZoom,
                imageRect.y + (spriteRect.height - (editingMark.pivot.y - spriteRect.y)) * imageZoom
            );
            DrawCircle(pivotScreen, 6f, pivotColor);
        }
    }

    private void HandleImageInput(Rect imageAreaRect)
    {
        Event e = Event.current;

        if (imageAreaRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Click to create new mark
                Vector2 localPos = ScreenToImageLocal(e.mousePosition);
                CreateNewMark(localPos);
                e.Use();
            }
        }
    }

    private Vector2 ScreenToImageLocal(Vector2 screenPos)
    {
        Rect spriteRect = currentData.image.textureRect;

        // Convert editor screen pos to sprite-relative position (GUI coords: top-left origin)
        float localX = (screenPos.x - imageRect.x) / imageZoom;
        float localY = (screenPos.y - imageRect.y) / imageZoom;

        // Convert to texture coordinates (Texture coords: bottom-left origin, Y flipped)
        float texX = spriteRect.x + localX;
        float texY = spriteRect.y + (spriteRect.height - localY);

        return new Vector2(texX, texY);
    }

    private void DrawEditPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.35f - 10));

        // New mark editing area
        if (isCreatingNewMark && editingMark != null)
        {
            DrawMarkEditor(editingMark, true);
        }
        // Selected mark editing
        else if (selectedMarkIndex >= 0 && selectedMarkIndex < currentData.characters.Count)
        {
            DrawMarkEditor(currentData.characters[selectedMarkIndex], false);
        }
        else
        {
            EditorGUILayout.HelpBox("点击图片新建字形预设", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMarkEditor(CharacterMark mark, bool isNew)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField(isNew ? "New Character Mark" : "Edit Character Mark", EditorStyles.boldLabel);

        mark.text = EditorGUILayout.TextField("Text", mark.text);
        mark.editorNote = EditorGUILayout.TextField("Editor Note", mark.editorNote);

        mark.pivot = EditorGUILayout.Vector2Field("Pivot", mark.pivot);

        EditorGUI.BeginChangeCheck();
        mark.min = EditorGUILayout.Vector2Field("Min", mark.min);
        mark.max = EditorGUILayout.Vector2Field("Max", mark.max);
        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }

        EditorGUILayout.BeginHorizontal();

        if (isNew)
        {
            if (GUILayout.Button("Confirm", GUILayout.Height(30)))
            {
                ConfirmNewMark();
            }
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                CancelNewMark();
            }
        }
        else
        {
            if (GUILayout.Button("Done", GUILayout.Height(30)))
            {
                selectedMarkIndex = -1;
                Repaint();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
    }

    private void DrawMarksList()
    {
        if (currentData.characters == null)
        {
            currentData.characters = new List<CharacterMark>();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Character Marks", EditorStyles.boldLabel);

        for (int i = 0; i < currentData.characters.Count; i++)
        {
            var mark = currentData.characters[i];

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            string displayText = string.IsNullOrEmpty(mark.editorNote)
                ? (string.IsNullOrEmpty(mark.text) ? $"Mark {i}" : mark.text)
                : mark.editorNote;

            // Highlight selected mark
            Color originalBgColor = GUI.backgroundColor;
            if (selectedMarkIndex == i)
            {
                GUI.backgroundColor = Color.yellow;
            }

            if (GUILayout.Button(displayText, GUILayout.Height(25), GUILayout.Width(200)))
            {
                selectedMarkIndex = i;
                isCreatingNewMark = false;
                GUI.FocusControl(null); // Clear focus to refresh text fields
                Repaint();
            }

            GUI.backgroundColor = originalBgColor;

            // Red X delete button
            Color originalColor = GUI.color;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Mark", $"Delete mark '{displayText}'?", "Yes", "No"))
                {
                    currentData.characters.RemoveAt(i);
                    if (selectedMarkIndex == i)
                    {
                        selectedMarkIndex = -1;
                    }
                    EditorUtility.SetDirty(currentData);
                    Repaint();
                }
            }
            GUI.backgroundColor = originalBgColor;
            GUI.color = originalColor;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewMark(Vector2 localPos)
    {
        if (isCreatingNewMark)
        {
            // Already creating, ignore
            return;
        }

        editingMark = new CharacterMark
        {
            pivot = localPos,
            min = localPos - new Vector2(20, 20),
            max = localPos + new Vector2(20, 20),
            text = ""
        };

        isCreatingNewMark = true;
        selectedMarkIndex = -1;
        Repaint();
    }

    private void ConfirmNewMark()
    {
        if (editingMark != null)
        {
            if (currentData.characters == null)
            {
                currentData.characters = new List<CharacterMark>();
            }

            currentData.characters.Add(editingMark);
            EditorUtility.SetDirty(currentData);

            editingMark = null;
            isCreatingNewMark = false;
            Repaint();
        }
    }

    private void CancelNewMark()
    {
        editingMark = null;
        isCreatingNewMark = false;
        Repaint();
    }

    private void CreateNewData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create ImagePreprocessData",
            "NewImageData",
            "asset",
            "Create a new ImagePreprocessData asset"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var data = CreateInstance<ImagePreprocessData>();
            data.characters = new List<CharacterMark>();
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();

            currentData = data;
            selectedMarkIndex = -1;
            isCreatingNewMark = false;
        }
    }

    private void LoadData()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load ImagePreprocessData",
            "Assets",
            "asset"
        );

        if (!string.IsNullOrEmpty(path))
        {
            // Convert absolute path to relative
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var data = AssetDatabase.LoadAssetAtPath<ImagePreprocessData>(path);
            if (data != null)
            {
                currentData = data;
                selectedMarkIndex = -1;
                isCreatingNewMark = false;

                if (currentData.characters == null)
                {
                    currentData.characters = new List<CharacterMark>();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to load ImagePreprocessData", "OK");
            }
        }
    }

    private void SaveData()
    {
        if (currentData != null)
        {
            EditorUtility.SetDirty(currentData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", "ImagePreprocessData saved successfully!", "OK");
        }
    }

    // Helper drawing methods
    private void DrawRect(Rect rect, Color color, float thickness)
    {
        Handles.BeginGUI();
        Handles.color = color;

        Vector3[] points = new Vector3[]
        {
            new Vector3(rect.xMin, rect.yMin, 0),
            new Vector3(rect.xMax, rect.yMin, 0),
            new Vector3(rect.xMax, rect.yMax, 0),
            new Vector3(rect.xMin, rect.yMax, 0),
            new Vector3(rect.xMin, rect.yMin, 0)
        };

        Handles.DrawAAPolyLine(thickness, points);
        Handles.EndGUI();
    }

    private void DrawCircle(Vector2 center, float radius, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;
        Handles.DrawSolidDisc(center, Vector3.forward, radius);
        Handles.EndGUI();
    }
}
