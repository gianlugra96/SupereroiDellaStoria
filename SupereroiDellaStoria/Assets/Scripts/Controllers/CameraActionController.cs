using System.Collections;
using UnityEngine;

public class CameraActionController : MonoBehaviour
{
    public RectTransform backgroundRect;
    private Coroutine m_moveCoroutine;
    private Coroutine m_zoomCoroutine;
    private Coroutine m_shakeCoroutine;

    public void Zoom(StoryScene.Sentence.CameraAction.Zoom zoomPercentage, float speed, bool isAnimated = true)
    {
        float scale;
        switch (zoomPercentage)
        {
            case StoryScene.Sentence.CameraAction.Zoom.x1: scale = 1f;
                break;
            case StoryScene.Sentence.CameraAction.Zoom.x2:
                scale = 1.3f;
                break;
            case StoryScene.Sentence.CameraAction.Zoom.x4:
                scale = 1.7f;
                break;
            case StoryScene.Sentence.CameraAction.Zoom.x8:
                scale = 2.5f;
                break;
            case StoryScene.Sentence.CameraAction.Zoom.x16:
                scale = 3.5f;
                break;
            default: scale = 1f;
                break;
        }

        var scaleVector = new Vector3(scale, scale, scale); 

        if(isAnimated && speed > 0)
        {
            StartZoom(scaleVector,speed);
        }
        else
        {
            backgroundRect.localScale = scaleVector;
        }
    }

    public void Move(Vector2 coords, float speed, bool isAnimated = true)
    {
        if (isAnimated && speed > 0)
        {
            StartMove(coords, speed);
        }
        else
        {
            backgroundRect.localPosition = coords;
        }
    }
    public void Shake(float duration, float magnitude, bool isAnimated = true)
    {
        if (isAnimated && duration > 0)
        {
            StartShake(duration, magnitude);
        }
    }

    private void StartZoom(Vector3 scaleVector, float speed)
    {
        if(m_zoomCoroutine != null)
        {
            StopCoroutine(m_zoomCoroutine);
        }
        m_zoomCoroutine = StartCoroutine(ZoomCoroutine(scaleVector, speed));
    }

    private void StartMove(Vector2 coords, float speed)
    {
        if (m_moveCoroutine != null)
        {
            StopCoroutine(m_moveCoroutine);
        }
        m_moveCoroutine = StartCoroutine(MoveCoroutine(coords, speed));
    }

    private void StartShake(float duration, float magnitude)
    {
        if (m_shakeCoroutine != null)
        {
            StopCoroutine(m_shakeCoroutine);
        }
        m_shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ZoomCoroutine(Vector3 scaleVector, float speed)
    {
        while (Vector3.Distance(backgroundRect.localScale, scaleVector) > 0.1f)
        {
            backgroundRect.localScale = Vector3.Lerp(backgroundRect.localScale, scaleVector, Time.deltaTime * speed);
            yield return null;
        }
    }

    private IEnumerator MoveCoroutine(Vector2 coords, float speed)
    {
        while (backgroundRect.localPosition.x != coords.x || backgroundRect.localPosition.y != coords.y)
        {
            backgroundRect.localPosition = Vector2.MoveTowards(backgroundRect.localPosition, coords,
                Time.deltaTime * 100f * speed);
            yield return null;
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector2 originalPosition = backgroundRect.localPosition;
        float elapsed = 0;
        while (elapsed < duration) 
        { 
            float x = Random.Range(-1f,1f) * magnitude;
            float y = Random.Range(-1f,1f) * magnitude;
            backgroundRect.localPosition = new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        backgroundRect.localPosition = originalPosition;
    }

    public void ResetZoom()
    {
        backgroundRect.transform.localScale = Vector2.one;
    }

    public void ResetMove()
    {
        backgroundRect.localPosition = Vector2.zero;
    }

    //public void Setup(Sprite sprite)
    //{
    //    switcher.SetImage(sprite);
    //}

    //public void Show(Vector2 coords, bool isAnimated = true)
    //{
    //    if (isAnimated)
    //    {
    //        animator.enabled = true;
    //        animator.SetTrigger("Show");
    //    }
    //    else
    //    {
    //        animator.enabled = false;
    //        canvasGroup.alpha = 1;
    //    }
    //    rect.localPosition = coords;
    //}

    //public void Hide(bool isAnimated = true)
    //{
    //    if (isAnimated)
    //    {
    //        animator.enabled = true;
    //        switcher.SyncImages();
    //        animator.SetTrigger("Hide");
    //    }
    //    else
    //    {
    //        animator.enabled = false;
    //        canvasGroup.alpha = 0;
    //    }
    //}


    //public void SwitchSprite(Sprite sprite, bool isAnimated = true)
    //{
    //    if (switcher.GetImage() != sprite)
    //    {
    //        if (isAnimated)
    //        {
    //            switcher.SwitchImage(sprite);
    //        }
    //        else
    //        {
    //            switcher.SetImage(sprite);
    //        }
    //    }
    //}
}
