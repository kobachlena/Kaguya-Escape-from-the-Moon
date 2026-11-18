using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

internal sealed class PointerUiHitTest
{
    private readonly List<RaycastResult> results = new List<RaycastResult>();
    private PointerEventData pointerEvent;
    private EventSystem cachedEventSystem;

    public bool IsOverUi(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        if (cachedEventSystem != eventSystem)
        {
            cachedEventSystem = eventSystem;
            pointerEvent = new PointerEventData(eventSystem);
        }

        pointerEvent.Reset();
        pointerEvent.position = position;
        results.Clear();
        eventSystem.RaycastAll(pointerEvent, results);

        bool hitUi = false;
        foreach (RaycastResult result in results)
        {
            if (result.module is GraphicRaycaster)
            {
                hitUi = true;
                break;
            }
        }

        results.Clear();
        return hitUi;
    }

    public static TouchControl FindTouch(InputAction pressAction)
    {
        for (InputControl control = pressAction.activeControl; control != null; control = control.parent)
        {
            if (control is TouchControl touch)
                return touch;
        }

        return null;
    }

    public static bool TryGetPosition(InputAction pressAction, out Vector2 position)
    {
        TouchControl touch = FindTouch(pressAction);
        if (touch != null)
        {
            position = touch.position.ReadValue();
            return true;
        }

        if (pressAction.activeControl?.device is Pointer pointer)
        {
            position = pointer.position.ReadValue();
            return true;
        }

        if (pressAction.activeControl == null && Pointer.current != null)
        {
            position = Pointer.current.position.ReadValue();
            return true;
        }

        position = Vector2.zero;
        return false;
    }
}
