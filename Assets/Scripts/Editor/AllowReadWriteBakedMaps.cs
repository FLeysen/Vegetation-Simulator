using UnityEngine;
using UnityEditor;

class AllowReadWriteBakedMaps : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("Lightmap-"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.isReadable = true;
        }
    }
}
