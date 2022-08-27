using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
namespace ReLogic.OS.FNA
{
    public class FNAIme : PlatformIme
    {
        private bool _disposedValue;
        public override uint CandidateCount => 0u;
        public override string CompositionString => string.Empty;
        public override bool IsCandidateListVisible => false;
        public override uint SelectedCandidate => 0u;

        public override string GetCandidate(uint index)
        {
            return string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (base.IsEnabled)
                {
                    Disable();
                }

                _disposedValue = true;
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
            if (base.IsEnabled)
            {
                OnKeyPress(c);
            }
        }

        ~FNAIme()
        {
            Dispose(disposing: false);
        }
    }
}
