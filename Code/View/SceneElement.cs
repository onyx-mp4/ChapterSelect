using System.Collections;
using Koop;
using UnityEngine;
using UnityEngine.UI;

using Math = System.Math;

namespace ChapterSelect.Code;

public class SceneElement : SelectableElement
{
    public int index;
    public ChaptersPage parentPage;
    
    public override void OnSelected()
    {
        base.OnSelected();
        StartCoroutine(AnimateAlpha(transform.GetChild(1).GetComponent<RawImage>(), Direction.Down));
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        StartCoroutine(AnimateAlpha(transform.GetChild(1).GetComponent<RawImage>(), Direction.Up));
    }

    public override void OnSubmit()
    {
        base.OnSubmit();
        Plugin.LOG.LogInfo("Scene clicked: " + name);
        Mgr_ChapterSelect.Instance.StartThrowbackForScene(name);
    }

    private IEnumerator AnimateAlpha(RawImage image, Direction direction)
    {
        const float duration = 0.2f; // Animation duration in seconds
        var startTime = Time.time;
        float startAlpha;
        float endAlpha;
        if (direction == Direction.Down)
        {
            startAlpha = Math.Min(0.75f, image.color.a);
            endAlpha = 0.25f;
        }
        else
        {
            startAlpha = Math.Max(0.25f, image.color.a);
            endAlpha = 0.75f;
        }
        
        var startColor = new Color(image.color.r, image.color.g, image.color.b, startAlpha);
        var endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha); // 25% alpha

        while (Time.time < startTime + duration)
        {
            var t = (Time.time - startTime) / duration;
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        image.color = endColor;
    }
}