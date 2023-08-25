using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;

namespace ReLogic.OS.FNA;

internal class FNAIme : PlatformIme
{
    public override uint CandidateCount => 0u;

    public override string CompositionString => string.Empty;

    public override bool IsCandidateListVisible => false;

    public override uint SelectedCandidate => 0u;

    private bool Disposed;

    public override string GetCandidate(uint index)
    {
        return string.Empty;
    }

    protected override void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (IsEnabled)
            {
                Disable();
            }

            Disposed = true;
        }
    }

    protected override void OnEnable()
    {
        TextInputEXT.TextInput += KeyPressCallback;
    }

    protected override void OnDisable()
    {
        TextInputEXT.TextInput -= KeyPressCallback;
    }

    private void KeyPressCallback(char c)
    {
        if (IsEnabled)
        {
            OnKeyPress(c);
        }
    }

    ~FNAIme()
    {
        Dispose(disposing: false);
    }
}
