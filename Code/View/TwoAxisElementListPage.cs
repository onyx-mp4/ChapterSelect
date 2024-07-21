using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChapterSelect.Code;

using Math = System.Math;

/*
 * ElementListPage is designed to be a vertical list of elements that can be navigated with up/down
 *
 * We are taking it and turning it into a grid of elements that can be navigated with up/down/left/right
 */
public class TwoAxisElementListPage : ElementListPage
{
    private RewiredAxisAction _uiHorizontal;
    private RewiredAxisAction _uiVertical;
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private int _lastInput;
    private float _holdTime;
#pragma warning restore CS0414 // I don't re-implement the functionality for these here. But I could! (and should) 
    private Coroutine _scrollCoroutine;
    
    protected ScrollRect OurScrollRect;
    
    public int columnCount = 1;

    enum LastInputDirection
    {
        Left = -2,
        Down = -1,
        None = 0,
        Up = 1,
        Right = 2
    }
    
    protected override void Awake()
    {
        _uiVertical = new RewiredAxisAction { action = "UIVertical", threshold = 0.8f };
        _uiHorizontal = new RewiredAxisAction() { action = "UIHorizontal", threshold = 0.8f };
        base.Awake();
    }

    public override void Initialize()
    {
        base.Initialize();
        InitializePage();
    }

    private IEnumerator SmoothScrollAnimate(float target)
    {
        float time = 0;
        var start = OurScrollRect.verticalNormalizedPosition;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            OurScrollRect.verticalNormalizedPosition = Mathf.SmoothStep(start, target, time / 0.2f);
            yield return null;
        }
    }

    private void CenterOnScrollRect()
    {
        var element = elementController.GetElement(elementController.selectedElementIndex);
        // The significantly cheaper call is private, but we don't follow the rules here, sunglasses emoji
        var scrollRectTransform = (RectTransform)OurScrollRect.CallPrivate("get_rectTransform");

        // Calculate position
        var contentHeight = scrollRectTransform.rect.height;
        var elementHeight = element.rectTransform.rect.height;
        var elementCenter = element.rectTransform.anchoredPosition.y + (elementHeight / 2);
        var normalizedPosition = Math.Clamp(Math.Abs(elementCenter / contentHeight), 0, 1);

        // Ensure element is visible
        if (_scrollCoroutine != null)
            StopCoroutine(_scrollCoroutine);
        _scrollCoroutine = StartCoroutine(SmoothScrollAnimate(1-normalizedPosition));
    }

    public override void Update()
    {
        // i dont think anyone ever intended for this class to be subclassed like this. alas i thrive.
        // this implements the same functionality as ElementListPage, but with the addition of horizontal navigation.
        if (!isActive || !IsInteractable())
        {
            return;
        }
        player ??= Scr_InputMaster.Instance.Player;
        var upPressed = player.GetButtonDown(_uiVertical, RewiredAxisAction.Dir.POSITIVE);
        var downPressed = player.GetButtonDown(_uiVertical, RewiredAxisAction.Dir.NEGATIVE);
        var leftPressed = player.GetButtonDown(_uiHorizontal, RewiredAxisAction.Dir.NEGATIVE);
        var rightPressed = player.GetButtonDown(_uiHorizontal, RewiredAxisAction.Dir.POSITIVE);

        if (upPressed)
        {
            _lastInput = (int)LastInputDirection.Up;
            Plugin.LOG.LogInfo("Up pressed");
            _holdTime = 0f;
        }
        else if (downPressed)
        {
            _lastInput = (int)LastInputDirection.Down;
            Plugin.LOG.LogInfo("Down pressed");
            _holdTime = 0f;
        }
        else if (leftPressed)
        {
            _lastInput = (int)LastInputDirection.Left;
            Plugin.LOG.LogInfo("Left pressed");
            _holdTime = 0f;
        }
        else if (rightPressed)
        {
            _lastInput = (int)LastInputDirection.Right;
            Plugin.LOG.LogInfo("Right pressed");
            _holdTime = 0f;
        }

        if (upPressed)
        {
            var targetIndex = Math.Max(0, elementController.selectedElementIndex - columnCount);
            elementController.SelectElement(targetIndex);
            CenterOnScrollRect();
        }
        else if (downPressed)
        {
            var targetIndex = Math.Min(elementController.elements.Count - 1, elementController.selectedElementIndex + columnCount);
            elementController.SelectElement(targetIndex);
            CenterOnScrollRect();
        }
        else if (leftPressed)
        {
            if (elementController.SelectPreviousElement(true))
            {
                CenterOnScrollRect();
            }
        }
        else if (rightPressed)
        {
            if (elementController.SelectNextElement(true))
            {
                CenterOnScrollRect();
            }
        }
        
        if (player.GetButtonDown(0))
        {
            elementController.GetSelectedElement()?.OnSubmit();
        }

        this.CallPrivate("UpdateScrollAxis");
    }
}