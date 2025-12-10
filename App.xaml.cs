using System.Windows;
using NPOBalance.Data;
using NPOBalance.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace NPOBalance
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                await InitializeDatabaseAsync();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                MessageBox.Show(
                    "데이터베이스 마이그레이션 중 오류가 발생했습니다.\n" +
                    "기존 데이터베이스 파일과 충돌이 발생했습니다.\n\n" +
                    "개발자에게 문의하시거나, 데이터베이스 파일을 삭제 후 다시 시도하세요.\n\n" +
                    $"오류: {ex.Message}",
                    "데이터베이스 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초기화 오류: {ex.Message}\n\n{ex.StackTrace}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            var companySelectionWindow = new CompanySelectionWindow
            {
                Owner = mainWindow
            };

            var dialogResult = companySelectionWindow.ShowDialog();

            if (dialogResult == true && companySelectionWindow.SelectedCompany != null)
            {
                // await를 사용하여 InitializeCompany가 완료될 때까지 기다립니다.
                await mainWindow.InitializeCompany(companySelectionWindow.SelectedCompany);
            }
            else
            {
                Shutdown();
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            using var context = new AccountingDbContext();
            //await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();
        }
    }
}
