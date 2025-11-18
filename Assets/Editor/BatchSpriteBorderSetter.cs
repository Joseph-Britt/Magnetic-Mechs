using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchSpriteBorderSetter : EditorWindow
{
    [MenuItem("Tools/Auto Slice Selected Sprites")]
    static void AutoSliceSelected()
    {
        Object[] selectedObjects = Selection.objects;
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select sprite textures first!", "OK");
            return;
        }

        int processedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            
            if (string.IsNullOrEmpty(path))
                continue;
                
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                // Change to Multiple mode so we can slice
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.isReadable = true;
                
                // Import first to apply the mode change
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                
                // Load the texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null)
                    continue;

                // Perform automatic slicing (trim transparent pixels)
                Rect trimmedRect = FindTrimmedBounds(texture);
                
                // Create sprite metadata
                SpriteMetaData spriteData = new SpriteMetaData();
                spriteData.rect = trimmedRect;
                spriteData.alignment = (int)SpriteAlignment.Center;
                spriteData.pivot = new Vector2(0.5f, 0.5f);
                spriteData.name = System.IO.Path.GetFileNameWithoutExtension(path);

                // Apply the slice
                importer.spritesheet = new SpriteMetaData[] { spriteData };
                
                EditorUtility.SetDirty(importer);
                AssetDatabase.WriteImportSettingsIfDirty(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                processedCount++;
                Debug.Log($"Auto-sliced: {spriteData.name} | Rect: {trimmedRect}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if (processedCount > 0)
        {
            Debug.Log($"<color=green>Successfully auto-sliced {processedCount} sprites!</color>");
            EditorUtility.DisplayDialog("Success", $"Auto-sliced {processedCount} sprites!\n\nThey are now in Multiple sprite mode.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Sprites Found", "No valid sprite textures were selected.", "OK");
        }
    }

    static Rect FindTrimmedBounds(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        int minX = width;
        int minY = height;
        int maxX = 0;
        int maxY = 0;

        // Find the bounds of non-transparent pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                
                // Check if pixel is not fully transparent
                if (pixel.a > 0.01f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // If no visible pixels found, return full texture
        if (minX > maxX || minY > maxY)
        {
            return new Rect(0, 0, width, height);
        }

        // Return the trimmed rectangle
        int rectWidth = maxX - minX + 1;
        int rectHeight = maxY - minY + 1;
        
        return new Rect(minX, minY, rectWidth, rectHeight);
    }
}