using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Relevo Art Pass 2.0 — destaque Torre / Mina / Pedreira.</summary>
    public static class CityReliefBuilder
    {
        public static void Build(Transform terrain)
        {
            if (terrain == null)
            {
                return;
            }

            // Montanhas / encostas no horizonte norte (atrás da Torre).
            Cliff(terrain, "CliffDragonL", new Vector3(-6.5f, 2.4f, 22.5f), new Vector3(5.5f, 4.8f, 3.2f));
            Cliff(terrain, "CliffDragonC", new Vector3(0f, 3.1f, 24.2f), new Vector3(7.5f, 6.2f, 3.8f));
            Cliff(terrain, "CliffDragonR", new Vector3(6.5f, 2.4f, 22.5f), new Vector3(5.5f, 4.8f, 3.2f));
            Boulder(terrain, "GiantRockN1", new Vector3(-3.2f, 1.6f, 20.2f), 2.4f);
            Boulder(terrain, "GiantRockN2", new Vector3(3.8f, 1.4f, 20.6f), 2.0f);

            // Platô / falésia da Torre.
            Dec(terrain, "DragonShelf", new Vector3(0f, 1.55f, 18.2f), new Vector3(9.5f, 0.55f, 6.5f),
                CityArtMaterialCategory.StoneDark);
            Dec(terrain, "DragonCliffFace", new Vector3(0f, 0.9f, 20.8f), new Vector3(10f, 1.8f, 1.6f),
                CityArtMaterialCategory.StoneDark);

            // Mina (NO) — encosta escura.
            Cliff(terrain, "MineSlope", new Vector3(-17.5f, 1.8f, 13.5f), new Vector3(4.5f, 3.2f, 4.0f));
            Boulder(terrain, "MineBoulder", new Vector3(-16.2f, 1.2f, 12.2f), 1.6f);
            Dec(terrain, "MineTalus", new Vector3(-15.5f, 0.55f, 11.5f), new Vector3(3.2f, 0.7f, 2.4f),
                CityArtMaterialCategory.Dirt);

            // Pedreira (NE) — platô claro quebrado.
            Cliff(terrain, "QuarryFace", new Vector3(17.8f, 1.7f, 13.8f), new Vector3(4.8f, 3.0f, 3.6f), light: true);
            Boulder(terrain, "QuarryBlockA", new Vector3(16.4f, 1.1f, 12.0f), 1.5f, light: true);
            Boulder(terrain, "QuarryBlockB", new Vector3(15.2f, 0.9f, 13.5f), 1.2f, light: true);
            Dec(terrain, "QuarryShelf", new Vector3(15.8f, 0.7f, 12.8f), new Vector3(4.5f, 0.45f, 3.5f),
                CityArtMaterialCategory.StoneLight);
        }

        private static void Cliff(Transform root, string name, Vector3 pos, Vector3 scale, bool light = false)
        {
            Dec(root, name, pos, scale, light ? CityArtMaterialCategory.StoneLight : CityArtMaterialCategory.StoneDark);
        }

        private static void Boulder(Transform root, string name, Vector3 pos, float size, bool light = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size, size * 0.65f, size * 0.9f);
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            Ignore(go);
            CityArtMaterialLibrary.Apply(go.GetComponent<Renderer>(),
                light ? CityArtMaterialCategory.StoneLight : CityArtMaterialCategory.StoneDark);
        }

        private static void Dec(Transform root, string name, Vector3 pos, Vector3 scale, CityArtMaterialCategory cat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localRotation = Quaternion.Euler(0f, pos.x * 7f, 0f);
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            Ignore(go);
            CityArtMaterialLibrary.Apply(go.GetComponent<Renderer>(), cat);
        }

        private static void Ignore(GameObject go)
        {
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }
        }
    }
}
