using Gtec.Chain.Common.Nodes.Utilities.CVEPCCA;
using Gtec.Chain.Common.Nodes.Utilities.LDA;
using Gtec.Chain.Common.SignalProcessingPipelines;
using Gtec.Chain.Common.Templates.Utilities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Gtec.Chain.Common.SignalProcessingPipelines.CVEPPipeline;
using static Gtec.Chain.Common.Templates.DataAcquisitionUnit.DataAcquisitionUnit;
using static Gtec.UnityInterface.CVEPBCIManager;
using Random = UnityEngine.Random;

namespace Gtec.UnityInterface
{
    public class BCIManager2D : MonoBehaviour
    {
        private CVEPFlashController2D _flashController;
        private Dictionary<int, SpriteRenderer> _selectedObjects;
        public Sprite[] sprites;
        private Canvas _cvTraining;
        private Canvas _cvConnectionDialog;
        private Canvas _cvTrainingCompletedDialog;
        private ConnectionDialog _connectionDialog;
        private TrainingDialog _trainingDialog;
        private TrainingCompletedDialog _trainingCompletedDialog;
        private States _currentState;
        private CVEPPipeline.Mode _currentMode;
        private GameObject Camera;
        public GameObject player, Zombie;
        private bool _connectionStateChanged;
        private bool _modeChanged;
        private bool _classifierCalculated;
        private bool _calculatingClassifier;
        private bool _classifierCalculationFailed;
        private bool _startFlashing;
        private System.Diagnostics.Stopwatch _sw;
        private int _flashingDelayMs = 1000;
        void Awake()
        {
            Camera = GameObject.Find("Main Camera");
        }

        void Start()
        {
            _sw = new System.Diagnostics.Stopwatch();

            //get bci dialogs
            Canvas[] dialogs = gameObject.GetComponentsInChildren<Canvas>();
            foreach (Canvas dialog in dialogs)
            {
                if (dialog.name.Equals("dlgConnection"))
                    _cvConnectionDialog = dialog;
                else if (dialog.name.Equals("dlgTraining"))
                    _cvTraining = dialog;
                else if (dialog.name.Equals("dlgTrainingCompleted"))
                    _cvTrainingCompletedDialog = dialog;
            }

            //connection dialog
            _connectionDialog = gameObject.GetComponentInChildren<ConnectionDialog>();
            _connectionDialog.BtnConnect_Click += OnBtnConnectClick;
            _connectionStateChanged = false;
            _modeChanged = false;
            _startFlashing = false;

            //training dialog
            _trainingDialog = gameObject.GetComponentInChildren<TrainingDialog>();
            _trainingDialog.BtnStartStopFlashing_Click += OnBtnStartStopFlashing_Click;

            //training completed dialog
            _trainingCompletedDialog = gameObject.GetComponentInChildren<TrainingCompletedDialog>();
            _trainingCompletedDialog.BtnContinueFlashing_Click += OnBtnContinue_Click;
            _trainingCompletedDialog.BtnRetrain_Click += OnBtnRetrain_Click;

            //set dialog visibility
            _cvConnectionDialog.gameObject.SetActive(true);
            _cvTraining.gameObject.SetActive(false);
            _cvTrainingCompletedDialog.gameObject.SetActive(false);

            //flash controller
            _flashController = gameObject.GetComponent<CVEPFlashController2D>();
            _flashController.FlashingStarted += OnFlashingStarted;
            _flashController.FlashingStopped += OnFlashingStopped;
            _flashController.Trigger += OnTrigger;

            BCIManager.Instance.Initialize();

            //bci manager
            _currentMode = CVEPPipeline.Mode.Training;

            //apply bci config
            CVEPBCIManager.Instance.RuntimeExceptionOccured += OnRuntimeExceptionOccured;
            CVEPBCIManager.Instance.ModeChanged += OnModeChanged;
            CVEPBCIManager.Instance.ClassifierCalculated += OnClassifierAvailable;
            CVEPBCIManager.Instance.ClassifierCalculationFailed += OnClassifierCalculationFailed;
            CVEPBCIManager.Instance.StateChanged += OnBCIStateChanged;

            _classifierCalculated = false;
            _calculatingClassifier = false;
        }

        private void OnTrigger(object sender, EventArgs e)
        {
            if (CVEPBCIManager.Instance.Initialized)
                CVEPBCIManager.Instance.SetTrigger();
        }

        private void OnClassifierAvailable(object sender, EventArgs e)
        {
            _calculatingClassifier = false;
            _classifierCalculated = true;
            CCAAccuracy accuracy = CVEPBCIManager.Instance.Accuracy();
            UnityEngine.Debug.Log(string.Format("Classifier calculated.\nAccuracy: {0}", accuracy.Result.ToString())); ;
        }

        private void OnClassifierCalculationFailed(object sender, EventArgs e)
        {
            _calculatingClassifier = false;
            _classifierCalculated = false;
            _classifierCalculationFailed = true;

            Debug.Log("Could not calculate classifier.");
        }

        private void OnFlashingStarted(object sender, EventArgs e)
        {
            Debug.Log("Flashing started");
        }

        private void OnFlashingStopped(object sender, EventArgs e)
        {
            Debug.Log("Flashing stopped");
            if (_currentMode == CVEPPipeline.Mode.Training)
            {
                _calculatingClassifier = true;
                _classifierCalculated = false;
                CVEPBCIManager.Instance.Train();
            }
        }

        private void OnModeChanged(object sender, ModeChangedEventArgs e)
        {
            _currentMode = e.Mode;
            _modeChanged = true;
            Debug.Log(String.Format("Mode Changed to '{0}'", _currentMode.ToString()));
        }

        private void OnBCIStateChanged(object sender, StateChangedEventArgs e)
        {
            _currentState = e.State;
            _connectionStateChanged = true;
            Debug.Log(String.Format("Device state changed to '{0}'", e.State));
        }

        private void OnRuntimeExceptionOccured(object sender, RuntimeExceptionEventArgs e)
        {
            Debug.Log(String.Format("A runtime exception occured.\n{0}\n{1}", e.Exception.Message, e.Exception.StackTrace));
        }

        private void OnBtnConnectClick(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    _currentState = States.Connecting;
                    _connectionStateChanged = true;

                    CVEPConfiguration config = CVEPBCIManager.Instance.Configuration;
                    config.NumberOfClasses = (int)_flashController.NumberOfClasses;
                    config.PhaseShift = _flashController.PhaseShift;
                    CVEPBCIManager.Instance.Configuration = config;
                    CVEPBCIManager.Instance.Initialize(_connectionDialog.Serial);
                }
                catch (Exception ex)
                {
                    try
                    {
                        CVEPBCIManager.Instance.Uninitialize();
                    }
                    catch
                    {
                    //DO NOTHING 
                    }

                    _currentState = States.Disconnected;
                    _connectionStateChanged = true;
                    Debug.Log(String.Format("Device initialization failed.\n{0}\n{1}", ex.Message, ex.StackTrace));
                }
            }).Start();
        }

        private void OnBtnStartStopFlashing_Click(object sender, EventArgs e)
        {
            if (_trainingDialog.IsFlashing)
            {
                _startFlashing = true;
            }
            else
            {
                _flashController.StopFlashing();
            }
        }

        private void OnBtnRetrain_Click(object sender, EventArgs e)
        {
            CVEPBCIManager.Instance.Configure(CVEPPipeline.Mode.Training);
            _trainingDialog.Reset();
        }

        private void OnBtnContinue_Click(object sender, EventArgs e)
        {
            CVEPBCIManager.Instance.Configure(CVEPPipeline.Mode.Application);
            Camera.transform.position = new Vector3(0, 35, -10);

            
            GameObject clone1 = Instantiate(Zombie, new Vector3(Random.Range(-20.5f, 20.5f), Random.Range(27, 43), 0), Quaternion.identity);
            GameObject clone2 = Instantiate(Zombie, new Vector3(Random.Range(-20.5f, 20.5f), Random.Range(27, 43), 0), Quaternion.identity);
            GameObject clone3 = Instantiate(Zombie, new Vector3(Random.Range(-20.5f, 20.5f), Random.Range(27, 43), 0), Quaternion.identity);
            GameObject clone4 = Instantiate(Zombie, new Vector3(Random.Range(-20.5f, 20.5f), Random.Range(27, 43), 0), Quaternion.identity);
            CVEPFlashObject2D clone1_smk, clone2_smk, clone3_smk, clone4_smk;
            clone1_smk.ClassId = 3;
            clone1_smk.Rotate = true;
            clone1_smk.GameObject = clone1;
            clone1_smk.DarkSprite = sprites[0];
            clone1_smk.FlashSprite = sprites[1];
            clone1_smk.Rotate = false;
            _flashController.ApplicationObjects.Add(clone1_smk);
            clone2_smk.ClassId = 4;
            clone2_smk.Rotate = true;
            clone2_smk.GameObject = clone2;
            clone2_smk.DarkSprite = sprites[0];
            clone2_smk.FlashSprite = sprites[1];
            clone2_smk.Rotate = false;
            _flashController.ApplicationObjects.Add(clone2_smk);
            clone3_smk.ClassId = 5;
            clone3_smk.Rotate = true;
            clone3_smk.GameObject = clone3;
            clone3_smk.DarkSprite = sprites[0];
            clone3_smk.FlashSprite = sprites[1];
            clone3_smk.Rotate = false;
            _flashController.ApplicationObjects.Add(clone3_smk);
            clone4_smk.ClassId = 6;
            clone4_smk.Rotate = true;
            clone4_smk.GameObject = clone4;
            clone4_smk.DarkSprite = sprites[0];
            clone4_smk.FlashSprite = sprites[1];
            clone4_smk.Rotate = false;
            _flashController.ApplicationObjects.Add(clone4_smk);
            _selectedObjects = new Dictionary<int, SpriteRenderer>();
            List<CVEPFlashObject2D> applicationObjects = _flashController.ApplicationObjects;
            foreach (CVEPFlashObject2D applicationObject in applicationObjects)
            {
                SpriteRenderer[] spriteRenderers = applicationObject.GameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    if (spriteRenderer.name.Contains("Selected"))
                    {
                        _selectedObjects.Add(applicationObject.ClassId, spriteRenderer);
                    }
                }
            }
            player.SetActive(true);
            clone1.SetActive(true);
            clone2.SetActive(true);
            clone3.SetActive(true);
            clone4.SetActive(true);
            Camera.transform.parent = player.transform;
            _startFlashing = true;
            

        }

        void Update()
        {
            //show/hide connection dialog
            if (_connectionStateChanged || _modeChanged)
            {
                if (_currentState == States.Disconnected)
                {
                    _cvConnectionDialog.gameObject.SetActive(true);
                    _cvTraining.gameObject.SetActive(false);
                    _cvTrainingCompletedDialog.gameObject.SetActive(false);
                }
                else if (_currentState == States.Connecting)
                {
                    _cvConnectionDialog.gameObject.SetActive(false);
                    _cvTraining.gameObject.SetActive(false);
                    _cvTrainingCompletedDialog.gameObject.SetActive(false);
                }
                else
                {
                    if (_currentMode == CVEPPipeline.Mode.Training)
                    {
                        _cvConnectionDialog.gameObject.SetActive(false);
                        _cvTraining.gameObject.SetActive(true);
                        _cvTrainingCompletedDialog.gameObject.SetActive(false);
                    }
                    else
                    {
                        _cvConnectionDialog.gameObject.SetActive(false);
                        _cvTraining.gameObject.SetActive(false);
                        _cvTrainingCompletedDialog.gameObject.SetActive(false);
                    }
                }
                _connectionStateChanged = false;
                _modeChanged = false;
            }

            if (_startFlashing && _currentState == States.Acquiring)
            {
                if (!_sw.IsRunning)
                {
                    _sw.Reset();
                    _sw.Start();
                }
                if (_sw.IsRunning && _sw.ElapsedMilliseconds > _flashingDelayMs)
                {
                    if (_currentMode == CVEPPipeline.Mode.Training)
                        _flashController.StartFlashing(CVEPFlashController2D.Mode.Training);
                    else
                        _flashController.StartFlashing(CVEPFlashController2D.Mode.Application);
                    _startFlashing = false;
                    _sw.Stop();
                }
            }

            if (_calculatingClassifier)
            {
                _cvConnectionDialog.gameObject.SetActive(false);
                _cvTraining.gameObject.SetActive(false);
                _cvTrainingCompletedDialog.gameObject.SetActive(false);
                _calculatingClassifier = false;
            }

            if (_classifierCalculated)
            {
                _cvConnectionDialog.gameObject.SetActive(false);
                _cvTraining.gameObject.SetActive(true);
                _cvTrainingCompletedDialog.gameObject.SetActive(true);
                _classifierCalculated = false;
            }

            if (_classifierCalculationFailed)
            {
                CVEPBCIManager.Instance.Configure(CVEPPipeline.Mode.Training);
                _trainingDialog.Reset();
                _classifierCalculationFailed = false;
            }
        }

        private void OnApplicationQuit()
        {
            _connectionDialog.BtnConnect_Click -= OnBtnConnectClick;

            _trainingDialog.BtnStartStopFlashing_Click -= OnBtnStartStopFlashing_Click;

            _trainingCompletedDialog.BtnContinueFlashing_Click -= OnBtnContinue_Click;
            _trainingCompletedDialog.BtnRetrain_Click -= OnBtnRetrain_Click;

            _flashController.FlashingStarted -= OnFlashingStarted;
            _flashController.FlashingStopped -= OnFlashingStopped;
            _flashController.Trigger -= OnTrigger;

            BCIManager.Instance.Uninitialize();

            CVEPBCIManager.Instance.RuntimeExceptionOccured -= OnRuntimeExceptionOccured;
            CVEPBCIManager.Instance.ModeChanged -= OnModeChanged;
            CVEPBCIManager.Instance.ClassifierCalculated -= OnClassifierAvailable;
            CVEPBCIManager.Instance.ClassifierCalculationFailed -= OnClassifierCalculationFailed;
            CVEPBCIManager.Instance.StateChanged -= OnBCIStateChanged;

            CVEPBCIManager.Instance.Uninitialize();
        }
    }
}