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

namespace RandomNumberGame
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random _random = new Random();
        private int _randomNumber;
        private int _maxTryCount;
        private int _tryCount;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TbxMinValue.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载异常：" + ex.Message);
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
                    MessageBox.Show("请输入正确的最小值");
                    TbxMinValue.Focus();
                    return;
                }

                var strMax = TbxMaxValue.Text;
                int max;
                if (string.IsNullOrEmpty((strMax)) || !int.TryParse(strMax, out max))
                {
                    MessageBox.Show("请输入正确的最大值");
                    TbxMinValue.Focus();
                    return;
                }

                var strMaxTryCount = TbxMaxTryCount.Text;
                int maxTryCount;
                if (string.IsNullOrEmpty((strMaxTryCount)) || !int.TryParse(strMaxTryCount, out maxTryCount))
                {
                    MessageBox.Show("请输入正确的最大次数");
                    TbxMaxTryCount.Focus();
                    return;
                }

                GenerateRandomNumber(min, max);
                _maxTryCount = maxTryCount;
                _tryCount = 0;
                PanelInput.IsEnabled = true;
                TbxInput.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作异常：" + ex);
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
                    MessageBox.Show("请输入正确的数值");
                    TbxInput.Focus();
                    return;
                }

                CheckInputValue(inputValue);
                TbxInput.Clear();
                TbxInput.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作异常：" + ex);
            }
        }

        private void CheckInputValue(int inputValue)
        {
            var resultText = inputValue + "：";
            if (inputValue > _randomNumber)
            {
                TbkResult.Text = resultText + Properties.Resources.TextTooBig;
            }
            else if (inputValue < _randomNumber)
            {
                TbkResult.Text = resultText + Properties.Resources.TextTooSmall;
            }
            else
            {
                TbkResult.Text = resultText + Properties.Resources.TextIsRight;
                PanelInput.IsEnabled = false;
            }
        }
    }
}
