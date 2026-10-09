using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Construye en tiempo de ejecución el nivel "Ruinas del Bosque Olvidado":
/// un bosque nocturno con ruinas de piedra, runas ancestrales y un espíritu guardián.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }
    public static readonly Vector3 PlayerStart = new Vector3(0f, 1.1f, -18f);

    public Player3D Player { get; private set; }
    public GameManagerPA4 Manager { get; private set; }
    public AudioSource MusicSource { get; private set; }
    public AudioClip PickupSound { get; private set; }
    public AudioClip HitSound { get; private set; }
    public AudioClip WinSound { get; private set; }
    public AudioClip AlarmSound { get; private set; }
    public ImpactPool ImpactPool { get; private set; }

    // Materiales del bosque y las ruinas
    Material mossMat, wallMat, stoneMat, woodMat, leafMat, dirtMat;
    Material runeMat, amberMat, spiritMat, eyeMat, mushroomMat, stemMat, portalMat, fireflyMat;
    Material tunicMat, hoodMat, skinMat, packMat;

    Transform worldRoot;
    readonly System.Random rng = new System.Random(7);

    // Luciérnagas decorativas (se animan en Update, sin físicas)
    struct Firefly { public Transform t; public Vector3 basePos; public float phase; public float speed; }
    readonly List<Firefly> fireflies = new List<Firefly>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Application.targetFrameRate = 60;
        BuildGame();
    }

    void Update()
    {
        float t = Time.time;
        for (int i = 0; i < fireflies.Count; i++)
        {
            Firefly f = fireflies[i];
            if (f.t == null) continue;
            float a = t * f.speed + f.phase;
            f.t.position = f.basePos + new Vector3(Mathf.Sin(a) * 0.7f, Mathf.Sin(a * 1.7f) * 0.4f, Mathf.Cos(a * 0.8f) * 0.7f);
        }
    }

    float Rand(float min, float max) { return min + (float)rng.NextDouble() * (max - min); }

    void BuildGame()
    {
        // Ambientación: noche de bosque con niebla verdosa.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.17f, 0.15f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.04f, 0.07f, 0.06f);
        RenderSettings.fogDensity = 0.018f;

        worldRoot = new GameObject("BOSQUE_RUINAS_3D").transform;
        CreateMaterials();
        CreateLighting();
        CreateEnvironment();
        CreateForest();
        CreateFireflies();
        CreatePlayer();
        CreateEnemy();
        CreateCollectibles();
        CreateExit();
        CreateUI();
        CreateAudio();
        CreatePool();
        StartCoroutine(IntroRoutine());
    }

    // ------------------------------------------------------------------ MATERIALES

    void CreateMaterials()
    {
        mossMat = MakeMat("PiedraMusgo", new Color(0.22f, 0.30f, 0.17f));
        wallMat = MakeMat("MuroRuinas", new Color(0.17f, 0.20f, 0.16f));
        stoneMat = MakeMat("PiedraAntigua", new Color(0.45f, 0.46f, 0.42f));
        woodMat = MakeMat("Madera", new Color(0.22f, 0.13f, 0.07f));
        leafMat = MakeMat("Follaje", new Color(0.08f, 0.30f, 0.10f));
        dirtMat = MakeMat("TierraBosque", new Color(0.07f, 0.09f, 0.06f));

        runeMat = MakeMat("RunaBrillante", new Color(0.15f, 0.95f, 0.75f), true);
        amberMat = MakeMat("RunaAmbar", new Color(1f, 0.68f, 0.15f), true);
        spiritMat = MakeMat("EspirituEtereo", new Color(0.55f, 0.95f, 0.85f), true, 1.2f);
        eyeMat = MakeMat("OjosEspiritu", new Color(1f, 0.5f, 0.08f), true, 3f);
        mushroomMat = MakeMat("HongoVenenoso", new Color(0.75f, 0.12f, 0.55f), true, 1.6f);
        stemMat = MakeMat("TalloHongo", new Color(0.80f, 0.78f, 0.70f));
        portalMat = MakeMat("PortalAncestral", new Color(0.3f, 1f, 0.55f), true);
        fireflyMat = MakeMat("Luciernaga", new Color(0.85f, 1f, 0.35f), true, 3f);

        tunicMat = MakeMat("TunicaExploradora", new Color(0.20f, 0.45f, 0.28f));
        hoodMat = MakeMat("CapuchaExploradora", new Color(0.55f, 0.30f, 0.12f));
        skinMat = MakeMat("Piel", new Color(0.85f, 0.65f, 0.50f));
        packMat = MakeMat("Mochila", new Color(0.35f, 0.22f, 0.12f));
    }

    Material MakeMat(string name, Color color, bool emission = false, float emissionMult = 2.2f)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = color };
        if (emission)
        {
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * emissionMult);
        }
        return m;
    }

    // ------------------------------------------------------------------ ILUMINACIÓN

    void CreateLighting()
    {
        // Luz de luna
        var moon = new GameObject("Luz_Luna");
        moon.transform.SetParent(worldRoot);
        moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var dl = moon.AddComponent<Light>();
        dl.type = LightType.Directional; dl.color = new Color(0.62f, 0.72f, 1f); dl.intensity = 0.55f; dl.shadows = LightShadows.Soft;

        // Luces suaves de bosque a lo largo de la ruta
        Vector3[] lightPos =
        {
            new Vector3(0f, 4f, -16f), new Vector3(0f, 4f, 0f), new Vector3(20f, 6f, 0f),
            new Vector3(20f, 6f, 14f), new Vector3(20f, 8.5f, 26f)
        };
        for (int i = 0; i < lightPos.Length; i++)
        {
            var g = new GameObject("Luz_Bosque_" + i);
            g.transform.SetParent(worldRoot);
            g.transform.position = lightPos[i];
            var l = g.AddComponent<Light>();
            l.type = LightType.Point; l.color = new Color(0.45f, 1f, 0.65f); l.intensity = 2.2f; l.range = 9f;
        }

        // Resplandor del santuario
        var key = new GameObject("Luz_Santuario");
        key.transform.SetParent(worldRoot); key.transform.position = new Vector3(20f, 8f, 30f);
        var kl = key.AddComponent<Light>(); kl.type = LightType.Point; kl.color = new Color(0.2f, 1f, 0.8f); kl.intensity = 6f; kl.range = 10f;
    }

    // ------------------------------------------------------------------ ESCENARIO

    void CreateEnvironment()
    {
        // Suelo lejano del bosque (decorativo; si el jugador cae, pierde una vida y reaparece).
        Cube("Suelo_Bosque", new Vector3(0, -4.5f, 5), new Vector3(200, 1, 160), dirtMat);

        // Ruta (avanza hacia el NORTE): claro -> sendero -> plaza -> torre este -> puente con salto -> santuario -> portal.
        Cube("Claro_Inicio", new Vector3(0, -0.5f, -18), new Vector3(14, 1, 12), mossMat);          // top 0
        Cube("Sendero_Piedra", new Vector3(0, -0.5f, -9), new Vector3(4, 1, 6), mossMat);           // top 0
        Cube("Plaza_Columnas", new Vector3(0, -0.5f, 0), new Vector3(18, 1, 12), mossMat);          // top 0
        Cube("Anexo_Oeste", new Vector3(-13.5f, -0.5f, 0), new Vector3(9, 1, 9), mossMat);          // top 0 (rincón secundario)
        Ramp("Rampa_Torre", new Vector3(9, 0, 0), new Vector3(15, 2.4f, 0), 4f, woodMat);
        Cube("Torre_Este", new Vector3(20, 1.9f, 0), new Vector3(10, 1, 10), mossMat);              // top 2.4
        Cube("Puente_Raices_A", new Vector3(20, 1.9f, 7.75f), new Vector3(3.5f, 1, 5.5f), mossMat); // top 2.4
        // Hueco de 2.5 m entre z=10.5 y z=13 : hay que SALTAR para cruzar.
        Cube("Puente_Raices_B", new Vector3(20, 1.9f, 15f), new Vector3(3.5f, 1, 4f), mossMat);     // top 2.4
        Ramp("Rampa_Santuario", new Vector3(20, 2.4f, 17), new Vector3(20, 4.4f, 25), 4f, woodMat);
        Cube("Santuario", new Vector3(20, 3.9f, 31), new Vector3(16, 1, 12), mossMat);              // top 4.4

        // Muros de ruinas que delimitan el nivel.
        Cube("Muro_Ruinas_Sur", new Vector3(0, 3.5f, -27), new Vector3(60, 9, 0.6f), wallMat);
        Cube("Muro_Ruinas_Norte", new Vector3(0, 3.5f, 38), new Vector3(60, 9, 0.6f), wallMat);
        Cube("Muro_Ruinas_Oeste", new Vector3(-30, 3.5f, 5.5f), new Vector3(0.6f, 9, 65), wallMat);
        Cube("Muro_Ruinas_Este", new Vector3(30, 3.5f, 5.5f), new Vector3(0.6f, 9, 65), wallMat);

        // Elementos de ruinas
        CreateFountain(new Vector3(0, 0, 0));
        CreateAltar(new Vector3(20, 4.4f, 30));
        CreateStela(new Vector3(-4, 1.3f, -13.5f), -1f);
        CreateStela(new Vector3(4, 1.3f, -13.5f), -1f);
        CreateStoneCircle(new Vector3(-13.5f, 0, 0));
        CreateBrokenArch(new Vector3(0, 0, 5.2f));
        CreateRuinPillars();

        // Hongos luminosos que guían el camino: x, z, altura del suelo.
        float[,] guide =
        {
            { 3f, -14f, 0f }, { -3f, -20f, 0f }, { 1.6f, -9f, 0f }, { 6f, -4f, 0f }, { -7f, 4f, 0f },
            { 17f, -3.5f, 2.4f }, { 23f, 3f, 2.4f }, { 21.3f, 8f, 2.4f }, { 18.7f, 15f, 2.4f },
            { 14f, 28f, 4.4f }, { 26f, 28f, 4.4f }, { 14f, 34f, 4.4f }
        };
        for (int i = 0; i < guide.GetLength(0); i++)
            CreateMushroom(new Vector3(guide[i, 0], guide[i, 2], guide[i, 1]), 1.1f, runeMat, new Color(0.3f, 1f, 0.8f), 1.6f);

        // Hongos venenosos: advertencia visual de zonas peligrosas.
        float[,] poison =
        {
            { -3f, -16f, 0f }, { 5f, 3f, 0f }, { -4f, -3f, 0f }, { -14f, 2f, 0f },
            { 24f, -3.5f, 2.4f }, { 18.5f, 28.5f, 4.4f }
        };
        for (int i = 0; i < poison.GetLength(0); i++)
            CreateMushroom(new Vector3(poison[i, 0], poison[i, 2], poison[i, 1]), 1.5f, mushroomMat, new Color(0.9f, 0.2f, 0.8f), 1.5f);
    }

    /// <summary>Rampa entre dos puntos de su superficie superior (funciona en cualquier dirección).</summary>
    GameObject Ramp(string name, Vector3 startTop, Vector3 endTop, float width, Material mat)
    {
        const float thickness = 0.6f;
        Vector3 dir = endTop - startTop;
        Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        Vector3 center = (startTop + endTop) * 0.5f - rot * Vector3.up * (thickness * 0.5f);
        var g = Cube(name, center, new Vector3(width, thickness, dir.magnitude), mat);
        g.transform.rotation = rot;
        return g;
    }

    /// <summary>Fuente seca con estanque luminoso en el centro de la plaza.</summary>
    void CreateFountain(Vector3 floorPos)
    {
        Shape(PrimitiveType.Cylinder, "Fuente_Borde", worldRoot, floorPos + new Vector3(0, 0.5f, 0), new Vector3(4.4f, 0.5f, 4.4f), stoneMat);
        var water = Shape(PrimitiveType.Cylinder, "Fuente_Agua", worldRoot, floorPos + new Vector3(0, 1.01f, 0), new Vector3(3.8f, 0.03f, 3.8f), runeMat, false);
        var l = water.AddComponent<Light>(); l.type = LightType.Point; l.color = new Color(0.2f, 1f, 0.8f); l.intensity = 3f; l.range = 8f;
        Cube("Fuente_Aguja", floorPos + new Vector3(0, 2.1f, 0), new Vector3(0.6f, 2.2f, 0.6f), stoneMat, false);
    }

    /// <summary>Círculo de piedras erguidas con un sello luminoso en el suelo.</summary>
    void CreateStoneCircle(Vector3 floorPos)
    {
        for (int i = 0; i < 6; i++)
        {
            float ang = i * Mathf.PI * 2f / 6f;
            float h = 1.2f + Rand(0f, 1.4f);
            Cube("Piedra_Circulo_" + i, floorPos + new Vector3(Mathf.Cos(ang) * 3f, h * 0.5f, Mathf.Sin(ang) * 3f), new Vector3(0.8f, h, 0.8f), stoneMat);
        }
        Shape(PrimitiveType.Cylinder, "Sello_Rúnico", worldRoot, floorPos + new Vector3(0, 0.05f, 0), new Vector3(2.6f, 0.03f, 2.6f), runeMat, false);
    }

    /// <summary>Arco roto que enmarca el borde norte de la plaza.</summary>
    void CreateBrokenArch(Vector3 basePos)
    {
        Shape(PrimitiveType.Cylinder, "Arco_Pilar_Izq", worldRoot, basePos + new Vector3(-3f, 2.5f, 0), new Vector3(1.1f, 2.5f, 1.1f), stoneMat);
        Shape(PrimitiveType.Cylinder, "Arco_Pilar_Der", worldRoot, basePos + new Vector3(3f, 1.5f, 0), new Vector3(1.1f, 1.5f, 1.1f), stoneMat);
        var lintel = Cube("Arco_Dintel_Roto", basePos + new Vector3(-1.4f, 5.25f, 0), new Vector3(3.4f, 0.6f, 1f), stoneMat, false);
        lintel.transform.rotation = Quaternion.Euler(0, 0, -5f);
    }

    /// <summary>Columnas rotas que emergen del abismo junto a las plataformas.</summary>
    void CreateRuinPillars()
    {
        // x, z, altura del suelo
        float[,] spots =
        {
            { -6f, -22f, 0f }, { 6f, -22f, 0f }, { -6f, -13f, 0f }, { 6f, -13f, 0f },
            { -8.5f, -5.5f, 0f }, { 8.5f, -5.5f, 0f }, { -8.5f, 5.5f, 0f }, { 8.5f, 5.5f, 0f },
            { -17f, -4f, 0f }, { -17f, 4f, 0f },
            { 24f, -4.2f, 2.4f }, { 16f, -4.2f, 2.4f }, { 24f, 4.2f, 2.4f }, { 16f, 4.2f, 2.4f },
            { 13f, 26f, 4.4f }, { 27f, 26f, 4.4f }, { 13f, 36f, 4.4f }, { 27f, 36f, 4.4f }
        };
        for (int i = 0; i < spots.GetLength(0); i++)
        {
            float x = spots[i, 0], z = spots[i, 1], floor = spots[i, 2];
            float top = floor + Rand(0.6f, 3.2f);
            float bottom = -4f;
            float h = top - bottom;
            var col = Shape(PrimitiveType.Cylinder, "Columna_Rota", worldRoot,
                new Vector3(x, bottom + h * 0.5f, z), new Vector3(1.2f, h * 0.5f, 1.2f), stoneMat);
            col.transform.rotation = Quaternion.Euler(0f, Rand(0f, 90f), 0f);
            // Capitel desgastado en las columnas más altas
            if (top - floor > 2f)
                Cube("Capitel", new Vector3(x, top + 0.15f, z), new Vector3(1.7f, 0.3f, 1.7f), stoneMat, false);
        }
    }

    /// <summary>Altar sagrado con monolito luminoso.</summary>
    void CreateAltar(Vector3 floorPos)
    {
        Cube("Altar_Base", floorPos + new Vector3(0, 0.5f, 0), new Vector3(3.5f, 1f, 3.5f), stoneMat);
        // El monolito queda detrás de la base para dejar libre la runa sobre el altar.
        var mono = Cube("Altar_Monolito", floorPos + new Vector3(0, 2.6f, 1.2f), new Vector3(1f, 3.2f, 0.9f), runeMat, false);
        var l = mono.AddComponent<Light>(); l.type = LightType.Point; l.color = new Color(0.2f, 1f, 0.8f); l.intensity = 5f; l.range = 8f;
        // Piedras alrededor del altar
        for (int i = 0; i < 4; i++)
        {
            float ang = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
            Cube("Altar_Piedra_" + i, floorPos + new Vector3(Mathf.Cos(ang) * 3.2f, 0.45f, Mathf.Sin(ang) * 3.2f), new Vector3(0.7f, 0.9f, 0.7f), stoneMat, false);
        }
    }

    /// <summary>Estela rúnica de piedra. faceDir indica hacia dónde mira la runa en Z.</summary>
    void CreateStela(Vector3 p, float faceDir)
    {
        Cube("Estela", p, new Vector3(1.4f, 2.6f, 0.5f), stoneMat);
        Cube("Estela_Runa", p + new Vector3(0, 0.3f, 0.28f * faceDir), new Vector3(0.7f, 1.1f, 0.05f), runeMat, false);
    }

    void CreateMushroom(Vector3 floorPos, float size, Material capMat, Color lightColor, float lightIntensity)
    {
        var root = new GameObject("Hongo").transform;
        root.SetParent(worldRoot, false);
        root.localPosition = floorPos;
        Shape(PrimitiveType.Cylinder, "Tallo", root, new Vector3(0, 0.25f * size, 0), new Vector3(0.14f * size, 0.25f * size, 0.14f * size), stemMat, false);
        Shape(PrimitiveType.Sphere, "Sombrero", root, new Vector3(0, 0.55f * size, 0), new Vector3(0.7f * size, 0.32f * size, 0.7f * size), capMat, false);
        var lg = new GameObject("Luz");
        lg.transform.SetParent(root, false);
        lg.transform.localPosition = new Vector3(0, 0.8f * size, 0);
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point; l.color = lightColor; l.intensity = lightIntensity; l.range = 4.5f;
    }

    // ------------------------------------------------------------------ BOSQUE

    void CreateForest()
    {
        // Anillo de árboles fuera de los muros (laterales, sur y norte).
        for (float z = -34f; z <= 45f; z += 3.6f)
        {
            for (int side = -1; side <= 1; side += 2)
                CreateTree(new Vector3(side * (33f + Rand(0f, 9f)), -4f, z + Rand(-1.2f, 1.2f)), Rand(11f, 17f), Rand(0.9f, 1.5f));
        }
        for (float x = -28f; x <= 28f; x += 3.6f)
        {
            CreateTree(new Vector3(x + Rand(-1.2f, 1.2f), -4f, -31f - Rand(0f, 8f)), Rand(11f, 17f), Rand(0.9f, 1.5f));
            CreateTree(new Vector3(x + Rand(-1.2f, 1.2f), -4f, 41f + Rand(0f, 8f)), Rand(11f, 17f), Rand(0.9f, 1.5f));
        }

        // Árboles gigantes que crecen desde el abismo entre las plataformas.
        float[,] inner =
        {
            { -16f, -12f }, { 16f, -14f }, { -20f, 10f }, { -12f, 16f },
            { 8f, 15f }, { 8f, 27f }, { -8f, 24f }, { -18f, 30f }
        };
        for (int i = 0; i < inner.GetLength(0); i++)
            CreateTree(new Vector3(inner[i, 0], -4f, inner[i, 1]), Rand(10f, 14f), Rand(1.1f, 1.6f));
    }

    void CreateTree(Vector3 basePos, float height, float girth)
    {
        var root = new GameObject("Arbol").transform;
        root.SetParent(worldRoot, false);
        root.localPosition = basePos;
        Shape(PrimitiveType.Cylinder, "Tronco", root, new Vector3(0, height * 0.5f, 0), new Vector3(girth, height * 0.5f, girth), woodMat, false);
        float s = Rand(0.85f, 1.2f);
        Shape(PrimitiveType.Sphere, "Copa_Baja", root, new Vector3(0, height, 0), new Vector3(5f * s, 3.4f * s, 5f * s), leafMat, false);
        Shape(PrimitiveType.Sphere, "Copa_Alta", root, new Vector3(0.4f, height + 2.2f * s, 0.2f), new Vector3(3.4f * s, 2.8f * s, 3.4f * s), leafMat, false);
    }

    void CreateFireflies()
    {
        // Zonas: xMin, xMax, zMin, zMax, altura del suelo
        float[,] zones =
        {
            { -6f, 6f, -24f, -12f, 0f }, { -9f, 9f, -6f, 6f, 0f }, { -18f, -9f, -4f, 4f, 0f },
            { 15f, 25f, -5f, 5f, 2.4f }, { 18.5f, 21.5f, 5f, 17f, 2.4f }, { 12f, 28f, 25f, 37f, 4.4f }
        };
        for (int i = 0; i < 45; i++)
        {
            int z = rng.Next(zones.GetLength(0));
            var p = new Vector3(Rand(zones[z, 0], zones[z, 1]), zones[z, 4] + Rand(0.8f, 4.2f), Rand(zones[z, 2], zones[z, 3]));
            var g = Shape(PrimitiveType.Sphere, "Luciernaga_" + i, worldRoot, p, Vector3.one * 0.13f, fireflyMat, false);
            fireflies.Add(new Firefly { t = g.transform, basePos = p, phase = Rand(0f, 6.28f), speed = Rand(0.5f, 1.3f) });
        }
    }

    // ------------------------------------------------------------------ PERSONAJES

    void CreatePlayer()
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        g.name = "Exploradora"; g.transform.SetParent(worldRoot); g.transform.position = PlayerStart;
        g.GetComponent<Renderer>().sharedMaterial = tunicMat;
        var col = g.GetComponent<CapsuleCollider>(); col.center = Vector3.zero; col.height = 2f;
        var cc = g.AddComponent<CharacterController>(); cc.height = 2f; cc.radius = 0.42f; cc.center = Vector3.zero; cc.stepOffset = 0.35f; cc.slopeLimit = 45f;
        Destroy(col);

        // Aspecto: capucha, rostro, mochila y farol de aceite.
        Shape(PrimitiveType.Sphere, "Capucha", g.transform, new Vector3(0, 0.72f, 0), Vector3.one * 0.62f, hoodMat, false);
        Shape(PrimitiveType.Sphere, "Rostro", g.transform, new Vector3(0, 0.68f, 0.22f), new Vector3(0.36f, 0.34f, 0.30f), skinMat, false);
        Shape(PrimitiveType.Cube, "Mochila", g.transform, new Vector3(0, 0.05f, -0.45f), new Vector3(0.6f, 0.8f, 0.32f), packMat, false);
        var lamp = Shape(PrimitiveType.Sphere, "Farol", g.transform, new Vector3(0.5f, 0f, 0.3f), Vector3.one * 0.24f, amberMat, false);
        var ll = lamp.AddComponent<Light>(); ll.type = LightType.Point; ll.color = new Color(1f, 0.75f, 0.35f); ll.intensity = 2.5f; ll.range = 7f;

        Player = g.AddComponent<Player3D>();
        Player.CameraOffset = new Vector3(0, 3.4f, -6.2f);

        var cam = new GameObject("Main Camera"); cam.tag = "MainCamera";
        cam.transform.position = g.transform.position + Player.CameraOffset;
        cam.transform.rotation = Quaternion.LookRotation(g.transform.position + Vector3.up - cam.transform.position);
        var camComp = cam.AddComponent<Camera>();
        camComp.clearFlags = CameraClearFlags.SolidColor;
        camComp.backgroundColor = RenderSettings.fogColor;
        camComp.farClipPlane = 150f;
        cam.AddComponent<AudioListener>();
        cam.AddComponent<FollowCamera3D>().target = g.transform;
    }

    void CreateEnemy()
    {
        // Espíritu del bosque: una criatura etérea con astas de ciervo y ojos ámbar.
        var e = new GameObject("Espiritu_Guardian");
        e.transform.SetParent(worldRoot);
        e.transform.position = new Vector3(-6f, 1.5f, -3.5f);

        Shape(PrimitiveType.Capsule, "Cuerpo", e.transform, Vector3.zero, new Vector3(0.9f, 1.2f, 0.9f), spiritMat, false);
        Shape(PrimitiveType.Sphere, "Ojo_Izq", e.transform, new Vector3(-0.2f, 0.65f, 0.36f), Vector3.one * 0.15f, eyeMat, false);
        Shape(PrimitiveType.Sphere, "Ojo_Der", e.transform, new Vector3(0.2f, 0.65f, 0.36f), Vector3.one * 0.15f, eyeMat, false);

        var antlerL = Shape(PrimitiveType.Cylinder, "Asta_Izq", e.transform, new Vector3(-0.28f, 1.35f, 0f), new Vector3(0.07f, 0.38f, 0.07f), woodMat, false);
        antlerL.transform.localRotation = Quaternion.Euler(0, 0, 20f);
        var antlerR = Shape(PrimitiveType.Cylinder, "Asta_Der", e.transform, new Vector3(0.28f, 1.35f, 0f), new Vector3(0.07f, 0.38f, 0.07f), woodMat, false);
        antlerR.transform.localRotation = Quaternion.Euler(0, 0, -20f);
        var tineL = Shape(PrimitiveType.Cylinder, "Punta_Izq", e.transform, new Vector3(-0.5f, 1.7f, 0f), new Vector3(0.05f, 0.22f, 0.05f), woodMat, false);
        tineL.transform.localRotation = Quaternion.Euler(0, 0, 50f);
        var tineR = Shape(PrimitiveType.Cylinder, "Punta_Der", e.transform, new Vector3(0.5f, 1.7f, 0f), new Vector3(0.05f, 0.22f, 0.05f), woodMat, false);
        tineR.transform.localRotation = Quaternion.Euler(0, 0, -50f);

        var aura = e.AddComponent<Light>();
        aura.type = LightType.Point; aura.color = new Color(0.5f, 1f, 0.8f); aura.intensity = 3f; aura.range = 6f;

        var ai = e.AddComponent<EnemyAI>();
        ai.player = Player.transform;
        ai.waypoints = new Transform[4];
        Vector3[] pts =
        {
            new Vector3(-6f, 1.5f, -3.5f), new Vector3(6f, 1.5f, -3.5f),
            new Vector3(6f, 1.5f, 4f), new Vector3(-6f, 1.5f, 4f)
        };
        for (int i = 0; i < pts.Length; i++)
        {
            var w = new GameObject("Waypoint_" + i);
            w.transform.SetParent(worldRoot);
            w.transform.position = pts[i];
            ai.waypoints[i] = w.transform;
        }
    }

    void CreateCollectibles()
    {
        // Runas ancestrales con forma de cristal tallado.
        Vector3[] positions = { new Vector3(-6, 0.9f, 3f), new Vector3(21, 3.2f, -2f), new Vector3(20, 6.4f, 30f) };
        for (int i = 0; i < positions.Length; i++)
        {
            var cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.name = "Runa_Ancestral_" + (i + 1); cell.transform.SetParent(worldRoot); cell.transform.position = positions[i];
            cell.transform.localScale = Vector3.one * 0.55f; cell.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            cell.GetComponent<Renderer>().sharedMaterial = amberMat;
            DestroyCollider(cell);
            var trigger = cell.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 1.4f;
            cell.AddComponent<CollectibleCell>();
            var glow = cell.AddComponent<Light>(); glow.type = LightType.Point; glow.color = new Color(1f, 0.7f, 0.2f); glow.intensity = 1.8f; glow.range = 4f;
            var ps = cell.AddComponent<ParticleSystem>(); var main = ps.main; main.startLifetime = 0.7f; main.startSpeed = 0.7f; main.startSize = 0.08f; main.maxParticles = 20; var em = ps.emission; em.rateOverTime = 8; var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.35f;
        }
    }

    void CreateExit()
    {
        // Portal ancestral al fondo del santuario, flanqueado por dos pilares y un dintel de piedra.
        var gate = Cube("Portal_Ancestral", new Vector3(20f, 6.7f, 36f), new Vector3(4f, 4f, 0.7f), portalMat, false);
        var trigger = gate.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(1.3f, 1.3f, 4f);
        gate.AddComponent<ExitGate>();
        var pl = gate.AddComponent<Light>(); pl.type = LightType.Point; pl.color = new Color(0.3f, 1f, 0.55f); pl.intensity = 5f; pl.range = 9f;

        Cube("Portal_Pilar_Izq", new Vector3(17.2f, 6.65f, 36f), new Vector3(0.9f, 4.5f, 0.9f), stoneMat, false);
        Cube("Portal_Pilar_Der", new Vector3(22.8f, 6.65f, 36f), new Vector3(0.9f, 4.5f, 0.9f), stoneMat, false);
        Cube("Portal_Dintel", new Vector3(20f, 9.25f, 36f), new Vector3(6.6f, 0.7f, 1f), stoneMat, false);
    }

    // ------------------------------------------------------------------ UI

    void CreateUI()
    {
        var canvasGO = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 50;
        var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
        var managerGO = new GameObject("GameManager"); Manager = managerGO.AddComponent<GameManagerPA4>(); Manager.canvas = canvas;
        Manager.infoText = MakeText(canvas.transform, "INFO", new Vector2(24, -20), new Vector2(800, 90), 24, TextAnchor.UpperLeft);
        Manager.messageText = MakeText(canvas.transform, "MESSAGE", new Vector2(0, -55), new Vector2(1000, 140), 34, TextAnchor.UpperCenter);
        Manager.messageText.text = "RUINAS DEL BOSQUE OLVIDADO";
        Manager.messageText.color = new Color(0.75f, 1f, 0.8f);
        Manager.helpText = MakeText(canvas.transform, "HELP", new Vector2(-24, 20), new Vector2(800, 60), 18, TextAnchor.LowerRight, true);
        Manager.helpText.text = "WASD / Flechas: mover   ·   Espacio: saltar   ·   ESC: pausa   ·   R: reiniciar al terminar";
    }

    Text MakeText(Transform parent, string name, Vector2 anchored, Vector2 size, int fontSize, TextAnchor anchor, bool bottomRight = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var rt = go.GetComponent<RectTransform>();
        if (bottomRight) { rt.anchorMin = new Vector2(1, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(1, 0); rt.anchoredPosition = anchored; }
        else { rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = anchored; }
        rt.sizeDelta = size; var t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize = fontSize; t.alignment = anchor; t.color = Color.white; t.horizontalOverflow = HorizontalWrapMode.Wrap; return t;
    }

    // ------------------------------------------------------------------ AUDIO

    void CreateAudio()
    {
        MusicSource = gameObject.AddComponent<AudioSource>();
        MusicSource.playOnAwake = false;
        MusicSource.loop = true;
        MusicSource.volume = 0.2f;
        MusicSource.spatialBlend = 0f;
        MusicSource.clip = MakeMusic();
        MusicSource.Play();
        PickupSound = MakeTone("Pickup", 0.2f, 660, 0.2f);
        HitSound = MakeTone("Hit", 0.3f, 90, 0.28f);
        WinSound = MakeTone("Win", 0.8f, 523, 0.2f);
        AlarmSound = MakeTone("Alarm", 0.22f, 330, 0.15f);
    }

    AudioClip MakeTone(string name, float seconds, float freq, float volume)
    {
        int rate = 44100, samples = Mathf.Max(1, Mathf.RoundToInt(rate * seconds)); var clip = AudioClip.Create(name, samples, 1, rate, false); var data = new float[samples];
        for (int i = 0; i < samples; i++) { float fade = 1f - i / (float)samples; data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * volume * fade; }
        clip.SetData(data, 0); return clip;
    }

    /// <summary>Música ambiental: notas suaves de pad sobre una capa de viento de bosque.</summary>
    AudioClip MakeMusic()
    {
        int rate = 22050, seconds = 8, n = rate * seconds;
        var clip = AudioClip.Create("Ambient_Forest", n, 1, rate, true);
        var data = new float[n];
        float[] notes = { 196f, 246.9f, 293.7f, 246.9f, 220f, 261.6f, 329.6f, 261.6f };
        var noiseRng = new System.Random(3);
        float lowpass = 0f;
        for (int i = 0; i < n; i++)
        {
            int beat = (i / rate) % notes.Length;
            float f = notes[beat];
            float phase = (i % rate) / (float)rate;
            float env = Mathf.Sin(Mathf.PI * phase); env *= env;
            float pad = Mathf.Sin(2f * Mathf.PI * f * i / rate) * 0.06f
                      + Mathf.Sin(2f * Mathf.PI * f * 1.5f * i / rate) * 0.02f
                      + Mathf.Sin(2f * Mathf.PI * f * 0.5f * i / rate) * 0.02f;

            float noise = (float)(noiseRng.NextDouble() * 2.0 - 1.0);
            lowpass += (noise - lowpass) * 0.03f;
            float swell = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 2f * i / n);
            float wind = lowpass * swell * 0.25f;

            data[i] = pad * env + wind;
        }
        clip.SetData(data, 0); return clip;
    }

    void CreatePool() { ImpactPool = gameObject.AddComponent<ImpactPool>(); ImpactPool.Initialize(8); }

    IEnumerator IntroRoutine()
    {
        // El jugador se habilita de inmediato para que la escena sea jugable al pulsar Play.
        Player.enabled = true;
        Manager.ShowObjective();
        yield return null;
    }

    // ------------------------------------------------------------------ EVENTOS DE JUEGO

    public void Pickup(Transform item)
    {
        Manager.CollectCell();
        if (ImpactPool != null) ImpactPool.Play(item.position, amberMat.color);
        PlayOneShot(PickupSound, item.position, 0.5f);
    }

    public void PlayerHit()
    {
        PlayOneShot(HitSound, Player.transform.position, 0.7f); Manager.LoseLife();
    }

    public void PlayerFell()
    {
        PlayOneShot(HitSound, Player.transform.position, 0.7f);
        Manager.LoseLife("Caíste entre las ruinas.");
    }

    public void PlayOneShot(AudioClip clip, Vector3 pos, float volume)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, pos, volume);
    }

    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    // ------------------------------------------------------------------ UTILIDADES

    void DestroyCollider(GameObject g) { var c = g.GetComponent<Collider>(); if (c != null) DestroyImmediate(c); }

    public void RegisterEnvironmentObject(GameObject g) { g.transform.SetParent(worldRoot); }

    GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, bool collider = true)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = localScale;
        g.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collider) DestroyCollider(g);
        return g;
    }

    GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat, bool collider = true)
    {
        return Shape(PrimitiveType.Cube, name, worldRoot, pos, scale, mat, collider);
    }
}
