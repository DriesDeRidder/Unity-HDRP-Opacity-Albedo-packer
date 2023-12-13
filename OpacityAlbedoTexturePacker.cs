#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class OpacityAlbedoTexturePacker : EditorWindow
{
 private Texture2D albedoTexture2D;
    private Texture2D opacityTexture2D;

    [MenuItem("Tools/Opacity Albedo Packer")]
    private static void ShowWindow()
    {
        var window = GetWindow<OpacityAlbedoTexturePacker>();
        window.titleContent = new GUIContent("Opacity Albedo Combiner");
        window.minSize = new Vector2(350, 250);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine Albedo and Opacity Textures", EditorStyles.boldLabel);
        albedoTexture2D = (Texture2D)EditorGUILayout.ObjectField("Albedo Texture", albedoTexture2D, typeof(Texture2D), false);
        opacityTexture2D = (Texture2D)EditorGUILayout.ObjectField("Alpha Texture", opacityTexture2D, typeof(Texture2D), false);

        if (GUILayout.Button("Combine Textures") && albedoTexture2D && opacityTexture2D)
        {
            CombineTextures(albedoTexture2D, opacityTexture2D);
        }
    }

    private void CombineTextures(Texture2D albedoTexture, Texture2D OpacityTexture)
    {
        if (albedoTexture.width != OpacityTexture.width || albedoTexture.height != OpacityTexture.height)
        {
            EditorUtility.DisplayDialog("Error", "Albedo and Alpha texture dimensions must match", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(albedoTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            EditorGUIUtility.PingObject( AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D))as Texture2D);
            EditorUtility.DisplayDialog("Error", "Albedo texture must be set to readable in import settings", "OK");
            return;
        }

        path = AssetDatabase.GetAssetPath(OpacityTexture);
        importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            EditorGUIUtility.PingObject( AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D))as Texture2D);

            EditorUtility.DisplayDialog("Error", "Alpha texture must be set to readable in import settings", "OK");

            return;
        }

        Color[] albedoPixels = albedoTexture.GetPixels();
        Color[] alphaPixels = OpacityTexture.GetPixels();

        for (int i = 0; i < albedoPixels.Length; i++)
        {
            albedoPixels[i].a = alphaPixels[i].a;
        }

        Texture2D combinedTexture = new Texture2D(albedoTexture.width, albedoTexture.height, TextureFormat.RGBA32, false);
        combinedTexture.SetPixels(albedoPixels);
        combinedTexture.Apply();

        byte[] pngData = combinedTexture.EncodeToPNG();
        if (pngData != null)
        {
            string combinedTexturePath = AssetDatabase.GenerateUniqueAssetPath(path.Replace(".png", "_combined.png"));
            System.IO.File.WriteAllBytes(combinedTexturePath, pngData);
            AssetDatabase.Refresh();
            TextureImporter importerForCombined = TextureImporter.GetAtPath(combinedTexturePath) as TextureImporter;
            importerForCombined.sRGBTexture = importer.sRGBTexture;
            AssetDatabase.ImportAsset(combinedTexturePath, ImportAssetOptions.ForceUpdate);
        }

        DestroyImmediate(combinedTexture);
    }
}
#endif