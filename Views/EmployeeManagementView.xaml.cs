using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NPOBalance.ViewModels;

namespace NPOBalance.Views
{
    public partial class EmployeeManagementView : UserControl
    {
        public EmployeeManagementViewModel? ViewModel => DataContext as EmployeeManagementViewModel;

        public EmployeeManagementView()
        {
            InitializeComponent();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 숫자만 입력 허용
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void EstimatedSalaryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            // 이벤트 핸들러가 재귀적으로 호출되는 것을 방지하기 위한 플래그
            if (textBox.GetValue(IsFormattingProperty) is bool isFormatting && isFormatting)
            {
                return;
            }

            // 커서 위치 저장
            int cursorPosition = textBox.SelectionStart;
            string originalText = textBox.Text;

            // 콤마 제거한 원본 숫자
            string cleanText = originalText.Replace(",", "");

            // 숫자로 파싱 가능한 경우에만 처리
            if (decimal.TryParse(cleanText, out var value))
            {
                // ViewModel 업데이트
                if (ViewModel?.SelectedEmployee != null)
                {
                    ViewModel.SelectedEmployee.EstimatedTotalSalary = value;
                }

                // 천단위 콤마 적용된 텍스트
                string formattedText = value.ToString("N0");

                // 텍스트가 변경된 경우에만 업데이트 (무한 루프 방지)
                if (originalText != formattedText)
                {
                    // 콤마 개수 차이 계산하여 커서 위치 조정
                    int digitsBeforeCursor = originalText.Take(cursorPosition).Count(c => char.IsDigit(c));

                    // 포맷팅 플래그 설정
                    textBox.SetValue(IsFormattingProperty, true);

                    // 텍스트 업데이트
                    textBox.Text = formattedText;

                    // 새로운 커서 위치 계산
                    int newCursorPosition = 0;
                    int digitCount = 0;

                    for (int i = 0; i < formattedText.Length; i++)
                    {
                        if (char.IsDigit(formattedText[i]))
                        {
                            digitCount++;
                            if (digitCount >= digitsBeforeCursor)
                            {
                                newCursorPosition = i + 1;
                                break;
                            }
                        }
                    }

                    // 커서 위치가 유효한 범위 내에 있는지 확인
                    textBox.SelectionStart = System.Math.Min(newCursorPosition, formattedText.Length);

                    // 포맷팅 플래그 해제
                    textBox.SetValue(IsFormattingProperty, false);
                }
            }
            else if (string.IsNullOrWhiteSpace(cleanText))
            {
                // 빈 문자열인 경우 null로 설정
                if (ViewModel?.SelectedEmployee != null)
                {
                    ViewModel.SelectedEmployee.EstimatedTotalSalary = null;
                }
            }
        }

        private static bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        // 포맷팅 중임을 나타내는 Attached Property
        private static readonly DependencyProperty IsFormattingProperty =
            DependencyProperty.RegisterAttached(
                "IsFormatting",
                typeof(bool),
                typeof(EmployeeManagementView),
                new PropertyMetadata(false));
    }
}