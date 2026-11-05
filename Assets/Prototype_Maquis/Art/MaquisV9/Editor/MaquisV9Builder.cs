using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

namespace CorseVR.Editor
{
    public static class MaquisV9Builder
    {
        const string Art = "Assets/Prototype_Maquis/Art/MaquisV9";
        const string Previous = "Assets/Prototype_Maquis/Art/BotaniqueV8";
        static Transform maquis, forest, house, tower;
        static Maquis35Route route;
        static MeshCollider terrain;
        static TourZoneSeparation zones;
        static Material leaves, granite, ground, litter;
        static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        static readonly Dictionary<GameObject, Vector3[]> geometry = new Dictionary<GameObject, Vector3[]>();
        static readonly List<string> oldNames = new List<string>
        {
            "07_Sol_Details", "10_Refonte_Botanique_V2", "11_Densification",
            "12_Versant_Continu_Habillage", "13_Strate_Basse_Finition"
        };
        static readonly Dictionary<int, List<CombineInstance>> leafCells = new Dictionary<int, List<CombineInstance>>();
        static System.Random random;
        static int planted, stones, patches;

        static float Range(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
        static void Context()
        {
            if (EditorApplication.isPlaying) throw new Exception("Quitter le mode Play avant de construire.");
            maquis = GameObject.Find("Maquis_35m").transform;
            route = maquis.GetComponent<Maquis35Route>();
            terrain = maquis.Find("01_Terrain_Chemin").GetComponentInChildren<MeshCollider>();
            house = GameObject.Find("Maison_Accueil_Corse").transform;
            var extension = GameObject.Find("Extension_Tour_Littoral").transform;
            tower = extension.Find("01_Tour_Genoise");
            zones = extension.GetComponent<TourZoneSeparation>();
            forest = maquis.Find("14_Maquis_Illustre");
        }
        static Material CopyMaterial(string file, Material original, Shader shader, float mode)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/" + file + ".mat");
            if (!material)
            {
                material = new Material(original);
                AssetDatabase.CreateAsset(material, Art + "/Materials/" + file + ".mat");
            }
            material.shader = shader;
            material.SetFloat("_Mode", mode);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
        public static string Prepare()
        {
            Context();
            foreach (string folder in new[] { "Materials", "Prefabs", "Meshes" })
                if (!AssetDatabase.IsValidFolder(Art + "/" + folder)) AssetDatabase.CreateFolder(Art, folder);
            var shader = Shader.Find("CorseVR/Maquis Illustre");
            if (!shader || !shader.isSupported) throw new Exception("Shader du maquis indisponible.");
            var botanical = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/FinitionV7/Materials/Botanique_Fibres_Nervures_V7.mat");
            leaves = CopyMaterial("Feuillage_et_ecorce", botanical, shader, 0);
            leaves.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "/Textures/Feuille_Peinte.png"));
            leaves.SetTexture("_BarkMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "/Textures/Ecorce_Peinte.png"));
            leaves.SetFloat("_Roughness", .86f);
            leaves.SetColor("_BaseColor", new Color(1.03f, 1.02f, .96f));
            leaves.SetColor("_ShadowTint", new Color(.43f, .57f, .52f));
            granite = CopyMaterial("Granit_patine", botanical, shader, 2);
            granite.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/FinitionV7/Textures/Granit_Peint.png"));
            granite.SetColor("_BaseColor", new Color(1.10f, 1.06f, .98f));
            granite.SetFloat("_Roughness", .94f);
            granite.SetFloat("_TextureScale", .70f);
            granite.SetColor("_ShadowTint", new Color(.60f, .61f, .55f));
            ground = CopyMaterial("Terre_et_litiere", botanical, shader, 3);
            ground.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/FinitionV7/Textures/Terre_Peinte.png"));
            ground.SetTexture("_SlopeMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/FinitionV7/Textures/Granit_Peint.png"));
            ground.SetTexture("_LitterMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "/Textures/Litiere_Peinte.png"));
            ground.SetColor("_BaseColor", Color.white);
            ground.SetColor("_ShadowTint", new Color(.53f, .58f, .51f));
            litter = CopyMaterial("Feuilles_mortes", botanical, shader, 4);
            litter.SetFloat("_Roughness", .97f);
            litter.SetColor("_BaseColor", Color.white);
            litter.SetColor("_ShadowTint", new Color(.55f, .57f, .48f));
            string[] families = { "ArbousierCanopee", "Filaire", "BruyereBalais", "Fragon", "Nepita", "Brize",
                "CheneVert", "Arbousier", "Bruyere", "Ciste", "Lentisque", "Myrte", "Romarin", "Lavande", "Immortelle", "Herbes" };
            foreach (string species in families)
                for (int form = 0; form < 3; form++) MakePrefab(species, form);
            foreach (string kind in new[] { "Granit", "Litiere" })
                for (int form = 0; form < 3; form++) MakeSimplePrefab(kind, form);
            AssetDatabase.SaveAssets();
            return "48 végétaux et 6 éléments minéraux/litière préparés.";
        }
        static void MakePrefab(string species, int form)
        {
            string path = Art + "/Prefabs/" + species + "_" + form + ".prefab";
            var root = new GameObject(species + "_" + form);
            try
            {
                var levels = new LOD[3];
                for (int level = 0; level < 3; level++)
                {
                    string modelPath = Art + "/Models/" + species + "_" + form + "_Niveau" + level + ".fbx";
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (!asset) asset = AssetDatabase.LoadAssetAtPath<GameObject>(Previous + "/Models/" + species + "_" + form + "_Niveau" + level + ".fbx");
                    if (!asset) throw new Exception(modelPath);
                    var instance = Object.Instantiate(asset, root.transform);
                    instance.name = "Niveau_" + level;
                    var renderers = instance.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.sharedMaterial = leaves;
                        renderer.shadowCastingMode = level < 2 ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    }
                    levels[level] = new LOD(level == 0 ? .60f : level == 1 ? .20f : .003f, renderers);
                }
                var group = root.AddComponent<LODGroup>();
                group.SetLODs(levels);
                group.RecalculateBounds();
                levels[0].screenRelativeTransitionHeight = Mathf.Clamp(group.size / 4.7f, .16f, .95f);
                levels[1].screenRelativeTransitionHeight = Mathf.Clamp(group.size / 15.5f, .05f, levels[0].screenRelativeTransitionHeight * .68f);
                group.SetLODs(levels);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { Object.DestroyImmediate(root); }
        }
        static void MakeSimplePrefab(string kind, int form)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "/Models/" + kind + "_" + form + ".fbx");
            var root = Object.Instantiate(asset);
            root.name = kind + "_" + form;
            try
            {
                var renderer = root.GetComponentInChildren<MeshRenderer>();
                renderer.sharedMaterial = kind == "Granit" ? granite : litter;
                renderer.shadowCastingMode = kind == "Granit" ? ShadowCastingMode.On : ShadowCastingMode.Off;
                if (kind == "Granit")
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                }
                PrefabUtility.SaveAsPrefabAsset(root, Art + "/Prefabs/" + root.name + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static GameObject Prefab(string name)
        {
            GameObject prefab;
            if (!prefabs.TryGetValue(name, out prefab))
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "/Prefabs/" + name + ".prefab");
                if (!prefab) throw new Exception("Prefab absent " + name);
                prefabs[name] = prefab;
            }
            return prefab;
        }
        static Vector3[] Vertices(GameObject prefab)
        {
            Vector3[] points;
            if (geometry.TryGetValue(prefab, out points)) return points;
            var group = prefab.GetComponent<LODGroup>();
            var filters = group ? group.GetLODs()[0].renderers.Where(r => r).Select(r => r.GetComponent<MeshFilter>()).Where(f => f) : prefab.GetComponentsInChildren<MeshFilter>();
            var vertices = new List<Vector3>();
            foreach (var filter in filters)
                using (var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh))
                using (var buffer = new Unity.Collections.NativeArray<Vector3>(data[0].vertexCount, Unity.Collections.Allocator.Temp))
                {
                    data[0].GetVertices(buffer);
                    vertices.AddRange(buffer.ToArray().Select(v => filter.transform.localToWorldMatrix.MultiplyPoint3x4(v)));
                }
            points = vertices.ToArray();
            geometry[prefab] = points;
            return points;
        }
        static bool Ground(Vector3 position, out RaycastHit hit)
        {
            return terrain.Raycast(new Ray(position + Vector3.up * 30, Vector3.down), out hit, 65);
        }
        static bool BuildingsClear(Bounds bounds)
        {
            var p = house.InverseTransformPoint(bounds.center);
            float ex = bounds.extents.x * 1.08f, ez = bounds.extents.z * 1.08f;
            if (p.x + ex > -5.62f && p.x - ex < 1.92f && p.z + ez > -5.72f && p.z - ez < .80f && bounds.min.y < house.position.y + 4.80f) return false;
            var q = tower.InverseTransformPoint(bounds.center);
            return new Vector2(q.x, q.z).magnitude > 4.45f + Mathf.Min(ex, ez);
        }
        static bool Place(string species, int form, float station, int side, float offset, float height, bool background)
        {
            var prefab = Prefab(species + "_" + form);
            var points = Vertices(prefab);
            float originalHeight = points.Max(v => v.y) - points.Min(v => v.y);
            float scale = height / originalHeight;
            var tangent = route.Direction(station); tangent.y = 0; tangent.Normalize();
            var outward = Vector3.Cross(Vector3.up, tangent).normalized;
            float lowRadius = points.Where(p => p.y * scale < 2.15f).Select(p => Mathf.Abs(p.x) * scale).DefaultIfEmpty(.2f).Max();
            offset = Mathf.Max(offset, lowRadius + .87f);
            var position = route.WorldAt(station) + outward * side * offset + tangent * Range(-.35f, .35f);
            RaycastHit hit;
            if (!Ground(position, out hit)) return false;
            position = hit.point - Vector3.up * .035f;
            float yaw = species == "ArbousierCanopee" ? (side < 0 ? 180 : 0) + Range(-16, 16) : Range(-180, 180);
            var rotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(0, yaw, 0);
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (var p in points)
            {
                var world = position + rotation * p * scale;
                min = Vector3.Min(min, world); max = Vector3.Max(max, world);
            }
            if (!BuildingsClear(new Bounds((min + max) * .5f, max - min))) return false;
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, forest);
            obj.name = species + "_" + form + "_S" + station.ToString("F1");
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.localScale = Vector3.one * scale;
            if (background)
            {
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Object.DestroyImmediate(obj.GetComponent<LODGroup>());
                Object.DestroyImmediate(obj.transform.Find("Niveau_0").gameObject);
                Object.DestroyImmediate(obj.transform.Find("Niveau_1").gameObject);
            }
            foreach (var component in obj.GetComponentsInChildren<Component>()) if (component) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            planted++;
            return true;
        }
        static void Rock(float station, int side)
        {
            var prefab = Prefab("Granit_" + random.Next(3));
            float size = Range(.38f, 1.14f);
            var tangent = route.Direction(station);
            var normal = Vector3.Cross(Vector3.up, tangent).normalized;
            var point = route.WorldAt(station) + normal * side * (1.40f + size);
            RaycastHit hit;
            if (!Ground(point, out hit)) return;
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, forest);
            obj.transform.SetPositionAndRotation(hit.point - Vector3.up * size * .24f, Quaternion.Euler(Range(-9, 8), Range(0, 360), Range(-8, 8)));
            obj.transform.localScale *= size;
            if (!BuildingsClear(obj.GetComponentInChildren<Renderer>().bounds)) { Object.DestroyImmediate(obj); return; }
            stones++;
        }
        static void LeafPatch(float station, int side, int row)
        {
            var prefab = Prefab("Litiere_" + random.Next(3));
            var tangent = route.Direction(station);
            var normal = Vector3.Cross(Vector3.up, tangent).normalized;
            var p = route.WorldAt(station) + normal * side * (Range(.62f, .92f) + row * Range(.23f, .36f)) + tangent * Range(-.17f, .17f);
            RaycastHit hit;
            if (!Ground(p, out hit)) return;
            var filter = prefab.GetComponentInChildren<MeshFilter>();
            var matrix = Matrix4x4.TRS(hit.point + Vector3.up * .002f, Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, Range(0, 360), 0), Vector3.one * Range(.76f, 1.18f));
            int cell = (int)(station / 5);
            if (!leafCells.ContainsKey(cell)) leafCells[cell] = new List<CombineInstance>();
            leafCells[cell].Add(new CombineInstance { mesh = filter.sharedMesh, transform = forest.worldToLocalMatrix * matrix * filter.transform.localToWorldMatrix });
            patches++;
        }
        static void BuildSection(float from, float to)
        {
            for (int step = 0; step < 50; step++)
            {
                float station = .35f + step * .70f;
                if (station < from || station >= to) continue;
                random = new System.Random(73031 + step * 433);
                bool opening = station > 8 && station < 11 || station > 22 && station < 25.5f;
                foreach (int side in new[] { -1, 1 })
                {
                    string[] border = opening ? new[] { "Ciste", "Herbes", "Lavande", "Romarin", "Nepita" } : new[] { "Fragon", "Herbes", "Filaire", "Nepita" };
                    string species = border[random.Next(border.Length)];
                    Place(species, random.Next(3), station, side, 1.12f, Range(.26f, species == "Filaire" ? .72f : .60f), false);
                    if (step % 2 == 0)
                    {
                        string middle = opening ? (side > 0 ? "Lentisque" : "Arbousier") : new[] { "Arbousier", "Bruyere", "Filaire", "BruyereBalais" }[random.Next(4)];
                        Place(middle, random.Next(3), station, side, Range(1.7f, 2.6f), Range(1.7f, 3.3f), false);
                    }
                    if (step % 3 == 0)
                        Place(opening ? "Arbousier" : "ArbousierCanopee", random.Next(3), station, side, Range(2.25f, 3.05f), Range(3.55f, 4.9f), false);
                    if (step % 4 == 0 && !opening)
                        Place("CheneVert", random.Next(3), station, side, Range(3.0f, 4.6f), Range(4.8f, 6.7f), false);
                    if (step % 2 == 0)
                        for (int row = 0; row < 2; row++)
                            Place(new[] { "Arbousier", "Bruyere", "Filaire" }[random.Next(3)], random.Next(3), station, side, Range(4.7f, 6.4f) + row * 2.2f, Range(2.6f, 4.1f), true);
                    if (opening && step % 3 == 0)
                    {
                        Place(side > 0 ? "Immortelle" : "Brize", random.Next(3), station, side, Range(1.0f, 1.7f), Range(.28f, .51f), false);
                    }
                    if (step % 5 == 0) Rock(station, side);
                    for (int row = 0; row < 3; row++) LeafPatch(station, side, row);
                }
            }
        }
        static void LeafMeshes()
        {
            foreach (var entry in leafCells)
            {
                var mesh = new Mesh { name = "Litiere_Secteur_" + entry.Key, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(entry.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                string path = Art + "/Meshes/Litiere_Secteur_" + entry.Key + ".asset";
                var previous = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (previous) { EditorUtility.CopySerialized(mesh, previous); Object.DestroyImmediate(mesh); mesh = previous; EditorUtility.SetDirty(mesh); }
                else AssetDatabase.CreateAsset(mesh, path);
                var old = forest.Find("Litiere_" + entry.Key);
                if (old) Object.DestroyImmediate(old.gameObject);
                var obj = new GameObject("Litiere_" + entry.Key);
                obj.transform.SetParent(forest, false);
                obj.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = obj.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = litter;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                var group = obj.AddComponent<LODGroup>();
                group.SetLODs(new[] { new LOD(.31f, new Renderer[] { renderer }) });
                group.RecalculateBounds();
            }
        }
        static void Register()
        {
            var rs = forest.GetComponentsInChildren<Renderer>();
            var cs = forest.GetComponentsInChildren<Collider>();
            var oldR = zones.maquisRenderers; var oldC = zones.maquisColliders;
            var ri = Enumerable.Range(0, oldR.Length).Where(i => oldR[i] && !oldR[i].transform.IsChildOf(forest)).ToArray();
            var ci = Enumerable.Range(0, oldC.Length).Where(i => oldC[i] && !oldC[i].transform.IsChildOf(forest)).ToArray();
            zones.maquisInitial = ri.Select(i => zones.maquisInitial[i]).Concat(rs.Select(r => true)).ToArray();
            zones.maquisRenderers = ri.Select(i => oldR[i]).Concat(rs).ToArray();
            zones.maquisColliderInitial = ci.Select(i => zones.maquisColliderInitial[i]).Concat(cs.Select(c => true)).ToArray();
            zones.maquisColliders = ci.Select(i => oldC[i]).Concat(cs).ToArray();
            zones.ApplySide(TourZoneSeparation.Side.Maquis);
            EditorUtility.SetDirty(zones);
        }
        static void Materials()
        {
            leaves = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/Feuillage_et_ecorce.mat");
            granite = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/Granit_patine.mat");
            ground = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/Terre_et_litiere.mat");
            litter = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/Feuilles_mortes.mat");
        }
        public static string Prototype()
        {
            Context(); Materials();
            if (forest) throw new Exception("Le secteur V9 existe déjà.");
            forest = new GameObject("14_Maquis_Illustre").transform;
            forest.SetParent(maquis, false);
            foreach (string name in oldNames) { var old = maquis.Find(name); if (old) old.gameObject.SetActive(false); }
            var terrainRenderer = terrain.GetComponent<MeshRenderer>();
            terrainRenderer.sharedMaterials = terrainRenderer.sharedMaterials.Select(m => m.GetFloat("_Mode") > 2.5f ? ground : granite).ToArray();
            EditorUtility.SetDirty(terrainRenderer);
            BuildSection(0, 5);
            LeafMeshes(); Register(); Physics.SyncTransforms();
            return planted + " végétaux sur les cinq premiers mètres.";
        }
        public static string Complete()
        {
            Context(); Materials();
            if (!forest) throw new Exception("Construire d'abord le prototype.");
            if (forest.Find("Parcours_35m_termine")) throw new Exception("Secteur déjà terminé.");
            BuildSection(5, 35.1f);
            LeafMeshes();
            foreach (string name in oldNames) { var old = maquis.Find(name); if (old) Object.DestroyImmediate(old.gameObject); }
            var marker = new GameObject("Parcours_35m_termine"); marker.transform.SetParent(forest, false);
            Register(); Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(maquis.gameObject.scene);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(maquis.gameObject.scene);
            return planted + " végétaux, " + stones + " groupes rocheux et " + patches + " nappes de litière.";
        }
        public static string Enrich()
        {
            Context(); Materials();
            if (!forest || forest.Find("Sous_bois_et_lisieres_termines")) throw new Exception("Etat de composition inattendu.");
            int before = forest.childCount;
            for (int step = 0; step < 49; step++)
            {
                float station = .50f + step * .70f;
                random = new System.Random(93117 + step * 317);
                bool sunny = station > 8 && station < 11 || station > 22 && station < 25.5f;
                foreach (int side in new[] {-1, 1})
                {
                    for (int i = 0; i < 2; i++)
                        Place(sunny ? new[] {"Ciste", "Romarin", "Lavande", "Nepita"}[random.Next(4)] : i == 0 ? "Fragon" : new[] {"Fragon", "Herbes", "Nepita"}[random.Next(3)],
                            random.Next(3), station + Range(-.22f,.22f), side, Range(1.05f, 1.70f), Range(.35f, .70f), false);
                    if (step % 2 == 0)
                    {
                        Place(sunny ? "Lentisque" : "Filaire", random.Next(3), station + Range(-.25f,.25f), side,
                            Range(2.5f, 3.8f), Range(1.2f, 1.9f), false);
                        Place(new[] {"Arbousier", "Filaire", "Bruyere"}[random.Next(3)], random.Next(3), station, side,
                            Range(4.1f, 5.2f), Range(1.6f, 2.5f), false);
                    }
                    if (step % 3 == 0)
                        Place(new[] {"Arbousier", "Filaire", "Lentisque"}[random.Next(3)], random.Next(3), station, side,
                            Range(6.0f, 7.8f), Range(1.9f, 3.0f), true);
                }
            }
            var marker = new GameObject("Sous_bois_et_lisieres_termines");marker.transform.SetParent(forest,false);
            Register();Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(maquis.gameObject.scene);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(maquis.gameObject.scene);
            return (forest.childCount - before - 1) + " groupes supplémentaires en bordure et sur les versants.";
        }
    }
}

