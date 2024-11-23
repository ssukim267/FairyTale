namespace NOT_Lonely.Weatherade
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

    [ExecuteInEditMode]
#endif
    public class SRS_TerrainUtility : MonoBehaviour
    {
        public Vector2Int basemapResolution = new Vector2Int(512, 512);
        [Range(0, 1)] public float tilingMultiplier = 0.2f;
        [SerializeField] private Camera cam;
        public Texture2D albedoLOD;
        public Texture2D normalLOD;
        [Range(1, 2)] public float boundsMultiplier = 1f;
        private RenderTexture primaryRT;
        private RenderTexture secondaryRT;
        private RenderTexture packedRT;
        [SerializeField] private Terrain terrain;
        [SerializeField] private Material initialMtl;
        [SerializeField] private Material bakeMtl;
        private Vector3 initialTerPos;
        private Material packTexMtl;

        private void OnEnable()
        {
            terrain = GetComponent<Terrain>();
            UpdateBoundsMultiplier(boundsMultiplier);
#if UNITY_EDITOR
            Lightmapping.bakeStarted += OnLightimapBakeStart;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            Lightmapping.bakeStarted -= OnLightimapBakeStart;
#endif
        }

        public void UpdateBoundsMultiplier(float multiplier)
        {
            if (terrain == null) terrain = GetComponent<Terrain>();

            if (terrain != null)
                terrain.patchBoundsMultiplier = Vector3.one * multiplier;
        }

#if UNITY_EDITOR

        void OnLightimapBakeStart()
        {
            BakeMaps();
        }


        public void ValicateValues()
        {
            basemapResolution.x = Mathf.ClosestPowerOfTwo(Mathf.Clamp(basemapResolution.x, 16, 4096));
            basemapResolution.y = Mathf.ClosestPowerOfTwo(Mathf.Clamp(basemapResolution.y, 16, 4096));
        }

        public void ReAssignTexture()
        {
            Shader.SetGlobalTexture("_SRS_BaseMapCustom", albedoLOD);
            if (initialMtl == null) initialMtl = GetComponent<Terrain>().materialTemplate;
            initialMtl.SetTexture("_AlbedoLOD", albedoLOD);
            initialMtl.SetTexture("_NormalLOD", normalLOD);
            terrain.terrainData.SetBaseMapDirty();
        }

        public void BakeMaps()
        {
            
            if (terrain == null) terrain = GetComponent<Terrain>();
            initialMtl = terrain.materialTemplate;

            if (initialMtl.shader != Shader.Find("NOT_Lonely/Weatherade/Snow Coverage (Terrain)")
                && initialMtl.shader != Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)")
                && initialMtl.shader != Shader.Find("NOT_Lonely/Weatherade/Rain Coverage (Terrain)"))
            {
                Debug.LogWarning("Terrain base map baking is only allowed with Weatherade shaders. Current terrain shader = " + initialMtl.shader);
                return;
            }
            if (initialMtl.shader == Shader.Find("NOT_Lonely/Weatherade/Snow Coverage (Terrain)") || Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)"))
                bakeMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Bake)"));
            if (initialMtl.shader == Shader.Find("NOT_Lonely/Weatherade/Rain Coverage (Terrain)"))
                bakeMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/Rain Coverage (Terrain-Bake)"));

            EditorUtility.DisplayProgressBar("Hold On", $"Baking LOD texture for '{terrain.name}'", 0.5f);

            bool initialDetailsDraw = terrain.drawTreesAndFoliage;
            terrain.drawTreesAndFoliage = false;
            float initialPixelError = terrain.heightmapPixelError;
            terrain.heightmapPixelError = 1;


            //Debug.Log("Initital mtl shader = " + initialMtl.shader);
            bakeMtl.CopyPropertiesFromMaterial(initialMtl);

            packTexMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/NL_TexturePacking"));

            cam = CreateCam();
            initialTerPos = terrain.transform.position;

            albedoLOD = Bake(albedoLOD, "AlbedoLOD", 0, true, 2);
            normalLOD = Bake(normalLOD, "NormalLOD", 1, false);

            ReAssignTexture();

            RenderTexture.active = null;
            cam.targetTexture = null;
            terrain.heightmapPixelError = initialPixelError;
            terrain.drawTreesAndFoliage = initialDetailsDraw;

            primaryRT.Release();
            secondaryRT.Release();
            packedRT.Release();

            DestroyImmediate(primaryRT, true);
            DestroyImmediate(secondaryRT, true);
            DestroyImmediate(packedRT, true);
            DestroyImmediate(packTexMtl, true);
            DestroyImmediate(bakeMtl, true);
            DestroyImmediate(cam.gameObject, true);
            EditorUtility.ClearProgressBar();
        }

        private Texture2D Bake(Texture2D texMap, string mapNamePostfix, int map_rgb_id, bool sRGB, int map_a_id = -1)
        {
            primaryRT = new RenderTexture(basemapResolution.x, basemapResolution.y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            terrain.materialTemplate = bakeMtl;
            terrain.transform.position = initialTerPos + Vector3.one * -100000;

            bakeMtl.SetFloat("_TilingMultiplier", tilingMultiplier);
            bakeMtl.SetInt("_MapID", map_rgb_id);
            cam.targetTexture = primaryRT;
            cam.Render();

            if (map_a_id != -1)
            {
                secondaryRT = new RenderTexture(basemapResolution.x, basemapResolution.y, 16, RenderTextureFormat.ARGB32);
                packedRT = new RenderTexture(basemapResolution.x, basemapResolution.y, 16, RenderTextureFormat.ARGB32);

                bakeMtl.SetFloat("_MapID", map_a_id);
                cam.targetTexture = secondaryRT;
                cam.Render();
                packTexMtl.SetTexture("_rgb", primaryRT);
                packTexMtl.SetTexture("_a", secondaryRT);
                Graphics.Blit(secondaryRT, packedRT, packTexMtl, 2);
                primaryRT = packedRT;
            }

            terrain.transform.position = initialTerPos;
            terrain.materialTemplate = initialMtl;


            texMap = SaveTex(primaryRT, "_" + mapNamePostfix, sRGB);
            //initialMtl.SetTexture("_" + mapNamePostfix, texMap);

            return texMap;
        }

        private Camera CreateCam()
        {
            cam = new GameObject("SRS_TerrainBaseMapCam").AddComponent<Camera>();
            cam.gameObject.hideFlags = HideFlags.HideAndDontSave;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.farClipPlane = terrain.terrainData.bounds.size.y + 3;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.forceIntoRenderTexture = true;
            cam.orthographicSize = Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z) / 2;
            cam.aspect = Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z) / Mathf.Min(terrain.terrainData.size.x, terrain.terrainData.size.z);
            cam.transform.parent = terrain.transform;
            cam.transform.localPosition = new Vector3(terrain.terrainData.size.x / 2, terrain.terrainData.bounds.size.y + 1, terrain.terrainData.size.z / 2);
            cam.transform.localEulerAngles = new Vector3(90, 0, 0);
            return cam;
        }

        private Texture2D SaveTex(RenderTexture rTex, string mapNamePostfix, bool sRGB)
        {
            string splatMapPath = AssetDatabase.GetAssetPath(initialMtl);
            splatMapPath = splatMapPath.Remove(splatMapPath.Length - 4) + mapNamePostfix + ".png";

            string absolutePath = Application.dataPath.Remove(Application.dataPath.Length - 7) + "/" + splatMapPath;

            Texture2D tex = new Texture2D(basemapResolution.x, basemapResolution.y, TextureFormat.ARGB32, false);

            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, basemapResolution.x, basemapResolution.y), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(absolutePath, bytes);

            AssetDatabase.Refresh();
            SetTextureImportSettings(splatMapPath, sRGB);

            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(splatMapPath, typeof(Texture2D));

            return tex;
        }

        private void SetTextureImportSettings(string texPath, bool sRGB)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texPath);
            TextureImporterSettings settings = new TextureImporterSettings();
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
            importer.ReadTextureSettings(settings);
            settings.sRGBTexture = sRGB;
            platformSettings.maxTextureSize = 8192;
            importer.SetTextureSettings(settings);
            importer.SetPlatformTextureSettings(platformSettings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

#endif

    }
}
