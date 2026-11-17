using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Ferramentas para criar elementos de interface (UGUI) por código.
public static class UiBuilder
{
    private static Font font;

    public static void SetFont(Font newFont)
    {
        font = newFont;
    }

    // Fonte padrão. Tenta a fonte embutida do Unity e, se não achar, uma fonte do sistema.
    public static Font GetFont()
    {
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        return font;
    }

    public static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    // Posiciona um elemento por frações da tela/pai (0 a 1) e uma margem interna (pad) em pixels.
    public static void SetAnchors(RectTransform rect, float xMin, float yMin, float xMax, float yMax, float pad)
    {
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = new Vector2(pad, pad);
        rect.offsetMax = new Vector2(-pad, -pad);
    }

    public static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = GetFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false; // o texto não bloqueia cliques
        return text;
    }

    // Faz o texto diminuir sozinho para caber na caixa.
    public static void EnableBestFit(Text text, int minSize, int maxSize)
    {
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

    public static Button CreateButton(string name, Transform parent, string label, int fontSize, Color color, UnityAction onClick)
    {
        RectTransform rect = CreatePanel(name, parent, color);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();

        Text text = CreateText("Texto", rect, label, fontSize, TextAnchor.MiddleCenter, Color.white);
        SetAnchors(text.rectTransform, 0f, 0f, 1f, 1f, 4f);

        button.onClick.AddListener(onClick);
        return button;
    }
}