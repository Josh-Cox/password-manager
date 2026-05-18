using Microsoft.Maui.Controls;

namespace PasswordManager.UI.Helpers;

public static class VisualElementExtensions
{
    public static async Task FadeInAsync(
        this VisualElement view,
        uint duration = 150)
    {
        view.IsVisible = true;
        view.InputTransparent = false;
        await view.FadeToAsync(1, duration);
    }

    public static async Task FadeOutAsync(
        this VisualElement view,
        uint duration = 150)
    {
        await view.FadeToAsync(0, duration);
        view.InputTransparent = true;
        view.IsVisible = false;
    }

    public static Task ShowAsync(this VisualElement view)
    {
        view.IsVisible = true;
        view.Opacity = 1;
        view.InputTransparent = false;
        return Task.CompletedTask;
    }

    public static Task HideAsync(this VisualElement view)
    {
        view.IsVisible = false;
        view.InputTransparent = true;
        return Task.CompletedTask;
    }

    public static async Task SetVisibleAsync(
        this VisualElement view,
        bool visible,
        uint duration = 150)
    {
        if (visible)
            await view.FadeInAsync(duration);
        else
            await view.FadeOutAsync(duration);
    }
}