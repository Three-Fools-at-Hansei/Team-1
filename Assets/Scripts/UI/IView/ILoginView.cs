using UnityEngine;

public interface ILoginView
{
    void SetIdInputText(string text);
    void SetPasswordInputText(string text);
    void SetHelperText(string text);
    void SetSubmitButtonEnabled(bool enabled);
    void SetSubmitButtonColor(Color color);
    void SetTopIconSprite(Sprite sprite);
}
