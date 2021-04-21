using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using NLog;

namespace RandomNumberGame
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static Properties.Settings mSettings = Properties.Settings.Default;
        private readonly Random _random = new Random();
        private int _randomNumber;
        private int _maxTryCount;
        private int _tryCount;
        private DateTime _startTime;
        private SpeechRecognitionEngine _recognizer;

        public MainWindow()
        {
            InitializeComponent();
            var bounds = mSettings.MainWindowBounds;
            this.Width = bounds.Width;
            this.Height = bounds.Height;
            this.WindowState = mSettings.MainWindowState;

            TbxMinValue.Text = mSettings.MinValue;
            TbxMaxValue.Text = mSettings.MaxValue;
            TbxMaxTryCount.Text = mSettings.MaxTryCount;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            TbkResult.SizeChanged += TbkResult_SizeChanged;
            ContentPanel.SizeChanged += TbkResult_SizeChanged;
        }

        private void TbkResult_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TbkResult.Text))
                {
                    return;
                }

                var change = Math.Min(ContentPanel.ActualHeight / TbkResult.ActualHeight, ContentPanel.ActualWidth / TbkResult.ActualWidth) * 0.9;
                TbkResult.FontSize = (int)(change * TbkResult.FontSize);
            }
            catch (Exception)
            {
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TbxMinValue.Focus();

                _recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("zh-CN"));
                _recognizer.LoadGrammar(new DictationGrammar());
                _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                _recognizer.SetInputToDefaultAudioDevice();
            }
            catch (Exception ex)
            {
                ShowMessage("加载异常：" + ex.Message);
            }
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                var text = e.Result.Text;
                TbxInput.Text = text;
                int value = 0;
                if (int.TryParse(text, out value))
                {
                    CheckInputValue(value);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("加载异常：" + ex.Message);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                mSettings.MainWindowBounds = this.RestoreBounds;
                mSettings.MainWindowState = (this.WindowState == WindowState.Minimized ? WindowState.Normal : this.WindowState);

                mSettings.MinValue = TbxMinValue.Text;
                mSettings.MaxValue = TbxMaxValue.Text;
                mSettings.MaxTryCount = TbxMaxTryCount.Text;
                mSettings.Save();

                if (_recognizer != null)
                {
                    _recognizer.Dispose();
                    _recognizer = null;
                }
            }
            catch (Exception ex)
            {
                ShowMessage("操作异常：" + ex);
            }
        }

        private void BtnStartPlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var strMin = TbxMinValue.Text;
                int min;
                if (string.IsNullOrEmpty((strMin)) || !int.TryParse(strMin, out min))
                {
                    ShowMessage("请输入正确的最小值");
                    TbxMinValue.Focus();
                    return;
                }

                var strMax = TbxMaxValue.Text;
                int max;
                if (string.IsNullOrEmpty((strMax)) || !int.TryParse(strMax, out max))
                {
                    ShowMessage("请输入正确的最大值");
                    TbxMinValue.Focus();
                    return;
                }

                var strMaxTryCount = TbxMaxTryCount.Text;
                int maxTryCount = 0;

                if (!string.IsNullOrEmpty((strMaxTryCount)) && !int.TryParse(strMaxTryCount, out maxTryCount))
                {
                    ShowMessage("请输入正确的最大次数");
                    TbxMaxTryCount.Focus();
                    return;
                }

                GenerateRandomNumber(min, max);
                _maxTryCount = maxTryCount;
                _tryCount = 0;
                PanelInput.IsEnabled = true;
                TbxInput.Focus();
                PanelStartPlay.IsEnabled = false;
                _startTime = DateTime.Now;
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception ex)
            {
                ShowMessage("操作异常：" + ex);
            }
        }

        private void GenerateRandomNumber(int min, int max)
        {
            _randomNumber = _random.Next(min, max);
        }

        private void BtnSubmit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var tbxInputText = TbxInput.Text;
                int inputValue;
                if (string.IsNullOrEmpty((tbxInputText)) || !int.TryParse(tbxInputText, out inputValue))
                {
                    ShowMessage("请输入正确的数值");
                    TbxInput.Focus();
                    return;
                }

                CheckInputValue(inputValue);
                TbxInput.Clear();
                TbxInput.Focus();
            }
            catch (Exception ex)
            {
                ShowMessage("操作异常：" + ex);
            }
        }

        private void CheckInputValue(int inputValue)
        {
            var resultText = inputValue + "：";
            _tryCount++;
            tbkTryCount.Text = string.Format("已猜{0}次", _tryCount);
            if (inputValue == _randomNumber)
            {
                var ts = DateTime.Now.Subtract(_startTime).TotalSeconds;
                var message = string.Format("您猜对了！正确答案是{0}，共耗时{1}秒。", _randomNumber, ts);
                ShowMessage(message);
                //TbkResult.Text = resultText + Properties.Resources.TextIsRight;
                GameEnd();
            }
            else
            {
                if (inputValue > _randomNumber)
                {
                    TbkResult.Text = resultText + Properties.Resources.TextTooBig;
                }
                else if (inputValue < _randomNumber)
                {
                    TbkResult.Text = resultText + Properties.Resources.TextTooSmall;
                }

                if (_maxTryCount > 0 && _tryCount >= _maxTryCount)
                {
                    var message = string.Format("很遗憾，游戏结束。正确答案是 {0}", _randomNumber);
                    ShowMessage(message);
                    GameEnd();
                }
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(this, message);
        }

        private void GameEnd()
        {
            PanelInput.IsEnabled = false;
            PanelStartPlay.IsEnabled = true;
            TbkResult.Text = "";
            tbkTryCount.Text = "";
            _recognizer?.RecognizeAsyncCancel();
        }
    }
}
