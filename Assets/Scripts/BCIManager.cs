using Gtec.Chain.Common.Nodes.Utilities.MatrixLib;
using System;
using static Gtec.Chain.Common.Nodes.InputNodes.ToWorkspace;

namespace Gtec.UnityInterface
{
    public sealed class BCIManager
    {
        public event EventHandler ClassSelectionAvailable;

        public class ClassSelectionAvailableEventArgs : EventArgs
        {
            public uint Class { get; set; }
        };

        private static BCIManager _instance = null;
        private bool _initialized;

        public static BCIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BCIManager();
                }
                return _instance;
            }
        }

        private BCIManager()
        {
            _initialized = false;
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                CVEPBCIManager.Instance.SelectedClassAvailable += OnSelectedClassAvailable;
                _initialized = true;
            }
        }

        public void Uninitialize()
        {
            if (_initialized)
            {
                CVEPBCIManager.Instance.SelectedClassAvailable -= OnSelectedClassAvailable;
                _initialized = false;
            }
        }

        private void OnSelectedClassAvailable(object sender, int e)
        {
            ClassSelectionAvailableEventArgs c = new ClassSelectionAvailableEventArgs();
            c.Class = (uint)e;
            ClassSelectionAvailable?.Invoke(this, c);
        }
    }
}