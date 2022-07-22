using System.Numerics;

internal struct Material
{
    public string Name;
    public Vector3 Ambient;
    public Vector3 Diffuse;
    public Vector3 Specular;
    public float Shininess;

    public Material(Vector3 ambient, Vector3 diffuse, Vector3 specular, float shininess)
        : this("", ambient, diffuse, specular, shininess) { }

    public Material(
        string name,
        Vector3 ambient,
        Vector3 diffuse,
        Vector3 specular,
        float shininess
    )
    {
        Name = name;
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
    }

    public static readonly Material Emerald =
        new(
            "Emerald",
            new(0.0215f, 0.1745f, 0.0215f),
            new(0.07568f, 0.61424f, 0.07568f),
            new(0.633f, 0.727811f, 0.633f),
            0.6f * 128f
        );

    public static readonly Material Jade =
        new(
            "Jade",
            new(0.135f, 0.2225f, 0.1575f),
            new(0.54f, 0.89f, 0.63f),
            new(0.316228f, 0.316228f, 0.316228f),
            0.1f * 128f
        );

    public static readonly Material Obsidian =
        new(
            "Obsidian",
            new(0.05375f, 0.05f, 0.06625f),
            new(0.18275f, 0.17f, 0.22525f),
            new(0.332741f, 0.328634f, 0.346435f),
            0.3f * 128f
        );

    public static readonly Material Pearl =
        new(
            "Pearl",
            new(0.25f, 0.20725f, 0.20725f),
            new(1f, 0.829f, 0.829f),
            new(0.296648f, 0.296648f, 0.296648f),
            0.088f * 128f
        );

    public static readonly Material Ruby =
        new(
            "Ruby",
            new(0.1745f, 0.01175f, 0.01175f),
            new(0.61424f, 0.04136f, 0.04136f),
            new(0.727811f, 0.626959f, 0.626959f),
            0.6f * 128f
        );

    public static readonly Material Turquoise =
        new(
            "Turquoise",
            new(0.1f, 0.18725f, 0.1745f),
            new(0.396f, 0.74151f, 0.69102f),
            new(0.297254f, 0.30829f, 0.306678f),
            0.1f * 128f
        );

    public static readonly Material Brass =
        new(
            "Brass",
            new(0.329412f, 0.223529f, 0.027451f),
            new(0.780392f, 0.568627f, 0.113725f),
            new(0.992157f, 0.941176f, 0.807843f),
            0.21794872f * 128f
        );

    public static readonly Material Bronze =
        new(
            "Bronze",
            new(0.2125f, 0.1275f, 0.054f),
            new(0.714f, 0.4284f, 0.18144f),
            new(0.393548f, 0.271906f, 0.166721f),
            0.2f * 128f
        );

    public static readonly Material Chrome =
        new(
            "Chrome",
            new(0.25f, 0.25f, 0.25f),
            new(0.4f, 0.4f, 0.4f),
            new(0.774597f, 0.774597f, 0.774597f),
            0.6f * 128f
        );

    public static readonly Material Copper =
        new(
            "Copper",
            new(0.19125f, 0.0735f, 0.0225f),
            new(0.7038f, 0.27048f, 0.0828f),
            new(0.256777f, 0.137622f, 0.086014f),
            0.1f * 128f
        );

    public static readonly Material Gold =
        new(
            "Gold",
            new(0.24725f, 0.1995f, 0.0745f),
            new(0.75164f, 0.60648f, 0.22648f),
            new(0.628281f, 0.555802f, 0.366065f),
            0.4f * 128f
        );

    public static readonly Material Silver =
        new(
            "Silver",
            new(0.19225f, 0.19225f, 0.19225f),
            new(0.50754f, 0.50754f, 0.50754f),
            new(0.508273f, 0.508273f, 0.508273f),
            0.4f * 128f
        );

    public static readonly Material BlackPlastic =
        new(
            "BlackPlastic",
            new(0.0f, 0.0f, 0.0f),
            new(0.01f, 0.01f, 0.01f),
            new(0.50f, 0.50f, 0.50f),
            0.25f * 128f
        );

    public static readonly Material CyanPlastic =
        new(
            "CyanPlastic",
            new(0.0f, 0.1f, 0.06f),
            new(0.0f, 0.50980392f, 0.50980392f),
            new(0.50196078f, 0.50196078f, 0.50196078f),
            0.25f * 128f
        );

    public static readonly Material GreenPlastic =
        new(
            "GreenPlastic",
            new(0.0f, 0.0f, 0.0f),
            new(0.1f, 0.35f, 0.1f),
            new(0.45f, 0.55f, 0.45f),
            0.25f * 128f
        );

    public static readonly Material RedPlastic =
        new(
            "RedPlastic",
            new(0.0f, 0.0f, 0.0f),
            new(0.5f, 0.0f, 0.0f),
            new(0.7f, 0.6f, 0.6f),
            0.25f * 128f
        );

    public static readonly Material WhitePlastic =
        new(
            "WhitePlastic",
            new(0.0f, 0.0f, 0.0f),
            new(0.55f, 0.55f, 0.55f),
            new(0.70f, 0.70f, 0.70f),
            0.25f * 128f
        );

    public static readonly Material YellowPlastic =
        new(
            "YellowPlastic",
            new(0.0f, 0.0f, 0.0f),
            new(0.5f, 0.5f, 0.0f),
            new(0.60f, 0.60f, 0.50f),
            0.25f * 128f
        );

    public static readonly Material BlackRubber =
        new(
            "BlackRubber",
            new(0.02f, 0.02f, 0.02f),
            new(0.01f, 0.01f, 0.01f),
            new(0.4f, 0.4f, 0.4f),
            0.078125f * 128f
        );

    public static readonly Material CyanRubber =
        new(
            "CyanRubber",
            new(0.0f, 0.05f, 0.05f),
            new(0.4f, 0.5f, 0.5f),
            new(0.04f, 0.7f, 0.7f),
            0.078125f * 128f
        );

    public static readonly Material GreenRubber =
        new(
            "GreenRubber",
            new(0.0f, 0.05f, 0.0f),
            new(0.4f, 0.5f, 0.4f),
            new(0.04f, 0.7f, 0.04f),
            0.078125f * 128f
        );

    public static readonly Material RedRubber =
        new(
            "RedRubber",
            new(0.05f, 0.0f, 0.0f),
            new(0.5f, 0.4f, 0.4f),
            new(0.7f, 0.04f, 0.04f),
            0.078125f * 128f
        );

    public static readonly Material WhiteRubber =
        new(
            "WhiteRubber",
            new(0.05f, 0.05f, 0.05f),
            new(0.5f, 0.5f, 0.5f),
            new(0.7f, 0.7f, 0.7f),
            0.078125f * 128f
        );

    public static readonly Material YellowRubber =
        new(
            "YellowRubber",
            new(0.05f, 0.05f, 0.0f),
            new(0.5f, 0.5f, 0.4f),
            new(0.7f, 0.7f, 0.04f),
            0.078125f * 128f
        );

    public static readonly Material[] All =
    {
        Emerald,
        Jade,
        Obsidian,
        Pearl,
        Ruby,
        Turquoise,
        Brass,
        Bronze,
        Chrome,
        Copper,
        Gold,
        Silver,
        BlackPlastic,
        CyanPlastic,
        GreenPlastic,
        RedPlastic,
        WhitePlastic,
        YellowPlastic,
        BlackRubber,
        CyanRubber,
        GreenRubber,
        RedRubber,
        WhiteRubber,
        YellowRubber
    };
}
