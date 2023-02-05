interface IMainWindowOpenAndCloser
{
    void HideWindow();
    // This function updates the state but does not send a message to close the window.
    void WindowClosed();
    void ShowWindow();
}
