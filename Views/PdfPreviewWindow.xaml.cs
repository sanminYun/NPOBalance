using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace NPOBalance.Views;

public partial class PdfPreviewWindow : Window
{
    private readonly byte[] _pdfBytes;
    private readonly string _defaultFileName;
    private string? _tempPdfPath;

    public PdfPreviewWindow(byte[] pdfBytes, string defaultFileName)
    {
        InitializeComponent();
        _pdfBytes = pdfBytes;
        _defaultFileName = defaultFileName;

        Loaded += PdfPreviewWindow_Loaded;
        Closed += PdfPreviewWindow_Closed;
    }

    private async void PdfPreviewWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await InitializeWebView();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"PDF 미리보기 초기화 중 오류가 발생했습니다:\n{ex.Message}",
                "오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
        }
    }

    private async Task InitializeWebView()
    {
        // WebView2 초기화
        await PdfWebView.EnsureCoreWebView2Async();

        // 임시 PDF 파일 생성
        _tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(_tempPdfPath, _pdfBytes);

        // PDF 파일을 WebView2로 로드
        PdfWebView.Source = new Uri(_tempPdfPath);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog
        {
            FileName = _defaultFileName,
            DefaultExt = ".pdf",
            Filter = "PDF 파일 (*.pdf)|*.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllBytes(saveDialog.FileName, _pdfBytes);

                var result = MessageBox.Show(
                    $"PDF 파일이 저장되었습니다.\n\n{saveDialog.FileName}\n\n파일을 여시겠습니까?",
                    "저장 완료",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"파일 저장 중 오류가 발생했습니다:\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void PdfPreviewWindow_Closed(object? sender, EventArgs e)
    {
        // 임시 파일 정리
        if (!string.IsNullOrEmpty(_tempPdfPath) && File.Exists(_tempPdfPath))
        {
            try
            {
                File.Delete(_tempPdfPath);
            }
            catch
            {
                // 임시 파일 삭제 실패 무시
            }
        }
    }
}