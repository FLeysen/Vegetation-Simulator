using UnityEditor;
using System.Text.RegularExpressions;

class AllowReadWriteBakedShadows : AssetPostprocessor
{
    private Regex _pathMatcher = new Regex("Lightmap-\\d+_comp_shadowmask.png");

    private void OnPreprocessTexture()
    {
        if (!_pathMatcher.IsMatch(assetPath)) return;

        TextureImporter textureImporter = assetImporter as TextureImporter;
        if (textureImporter)
            textureImporter.isReadable = true;
    }
}
