using UnityEngine;

public interface ILoginView
{
    void SetIdInputText(string text);
    void SetPasswordInputText(string text);
    void SetSignupNicknameText(string text);
    void SetSignupIdText(string text);
    void SetSignupPasswordText(string text);
    void SetSignupPasswordConfirmText(string text);
    void SetHelperText(string text);
    void SetSubmitButtonEnabled(bool enabled);
    void SetSubmitButtonColor(Color color);
    void SetTopIconSprite(Sprite sprite);
    void ShowLoginLayout();
    void ShowSignupLayout();
}
