using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
namespace TerraAngel.Input
{
    public class RealFNAIme : PlatformIme
    {
        public static bool blocking = false;
        private bool _disposedValue;
        public override uint CandidateCount => 0u;
        public override string CompositionString => string.Empty;
        public override bool IsCandidateListVisible => false;
        public override uint SelectedCandidate => 0u;
        private void OnCharCallback(char key)
        {
            if (base.IsEnabled)
            {
                OnKeyPress(key);
            }
        }

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
            if (!blocking)
                OnKeyPress(c);
        }

        ~RealFNAIme()
        {
            Dispose(disposing: false);
        }
    }
}
