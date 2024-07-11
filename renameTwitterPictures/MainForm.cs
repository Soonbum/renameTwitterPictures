using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace renameTwitterPictures
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 폴더 브라우저 대화상자의 설명 설정
                folderBrowserDialog.Description = "X 다운로드 이미지 파일이 들어있는 디렉토리를 선택하세요.";
                folderBrowserDialog.ShowNewFolderButton = true;

                // 다이얼로그 표시
                DialogResult result = folderBrowserDialog.ShowDialog();

                // 선택된 폴더 경로 사용
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    ProcessFiles(selectedPath);

                    // selectedPath 안에 있는 파일 목록을 저장하고, 이름 기준으로 오름차순 정렬

                    // 목록 안에서 iterate
                    // 파일명 문자열에서 "-" 기준으로 나눔
                    // [0] 문자열로 된 디렉토리를 생성함
                    // NeamoSub-1805174439564423582-01
                }
            }
        }

        private static void ProcessFiles(string path)
        {
            // 파일 목록을 가져옵니다.
            var files = Directory.GetFiles(path);

            // Account별로 파일들을 그룹화합니다.
            var groupedFiles = files
                .Select(file => new
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Parts = Path.GetFileName(file).Split('-')
                })
                .Where(file => file.Parts.Length == 3)
                .GroupBy(file => file.Parts[0]);

            foreach (var group in groupedFiles)
            {
                string account = group.Key;
                string accountDirectory = Path.Combine(path, $"@{account}");

                // Account별 디렉토리를 생성합니다.
                if (!Directory.Exists(accountDirectory))
                {
                    Directory.CreateDirectory(accountDirectory);
                }

                // 파일명을 오름차순으로 정렬합니다.
                var sortedFiles = group.OrderBy(file => file.FileName).ToList();
                int counter = 1;

                foreach (var file in sortedFiles)
                {
                    string originalPath = file.FilePath;
                    string extension = Path.GetExtension(originalPath);
                    string newFileName = $"{DateTime.Now:yyyyMMdd}_{counter:D6}{extension}";
                    string newFilePath = Path.Combine(accountDirectory, newFileName);

                    // 파일을 새 이름으로 이동합니다.
                    File.Move(originalPath, newFilePath);
                    counter++;
                }
            }

            Console.WriteLine("File processing completed.");
        }
    }
}
