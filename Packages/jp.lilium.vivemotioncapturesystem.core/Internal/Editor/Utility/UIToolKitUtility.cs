using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;


namespace Lilium.VMCStudio.Editor
{
    public static class UIToolKitUtility
    {
        public static BaseField<T> RegisterGetSetCallbacks<T> (this BaseField<T> field, Func<T> getter, Action<T> setter, int updateIntervalMs = 100)
        {
            field.RegisterValueChangedCallback (s => { setter (s.newValue); });
            field.RegisterCallback<BlurEvent> (evt => { field.value = getter (); }, TrickleDown.TrickleDown);

            var fieldValue = getter ();
            field.value = fieldValue;
            setter (fieldValue);

            field.schedule.Execute (() => {
                if (field.focusController.focusedElement != field) {
                    var val = getter ();
                    field.value = val;
                }
            }).Every (updateIntervalMs);
            return field;
        }
    }

}